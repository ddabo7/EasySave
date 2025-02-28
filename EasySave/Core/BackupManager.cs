using EasySave.Logging;
using EasySave.Models;
using EasySave.Utils;
using EasySave.Services;
using System.Text.Json;
using System.Xml.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using System.Security.Cryptography;
using System.Text;

namespace EasySave.Core;

/// <summary>
/// Gère l'exécution des sauvegardes.
/// </summary>
public class BackupManager
{
    private readonly List<BackupJob> backupJobs;
    private readonly StateManager stateManager;
    private ILogManager? logger;
    private readonly string logsPath;
    private readonly CryptoSoftService cryptoService;
    private BusinessSoftwareService businessSoftwareService;
    private const int n = 1024 * 1024; // 1 Mo
    private const int NetworkLoadThreshold = 80; // Seuil de charge réseau en pourcentage
    private const int MaxParallelTasks = 5; // Nombre maximum de tâches parallèles
    private readonly SynchronizationContext _syncContext;

    // Événement déclenché quand une sauvegarde est terminée
    public event Action<string, bool, string?>? OnBackupComplete;

    // Événement déclenché pour afficher la progression
    public event Action<string, int>? OnProgress;

    public IReadOnlyList<BackupJob> BackupJobs => backupJobs.AsReadOnly();
    public StateManager StateManager => stateManager;

    private bool IsPriorityFile(string filePath, IEnumerable<string> priorityExtensions)
    {
        if (string.IsNullOrEmpty(filePath) || priorityExtensions == null || !priorityExtensions.Any())
            return false;

        var extension = Path.GetExtension(filePath);
        if (string.IsNullOrEmpty(extension))
            return false;

        // Normaliser l'extension (enlever le point si présent)
        extension = extension.TrimStart('.');

        return priorityExtensions.Any(ext =>
        {
            var normalizedExt = ext.TrimStart('.');
            return extension.Equals(normalizedExt, StringComparison.OrdinalIgnoreCase);
        });
    }

    private string GetDailyLogsDirectory()
    {
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var projectRoot = Directory.GetParent(baseDirectory)?.Parent?.Parent?.Parent?.FullName;
        if (projectRoot == null)
            throw new InvalidOperationException("Could not determine project root directory");

        var dailyLogsDirectory = Path.Combine(projectRoot, "Logs", "Daily");
        Directory.CreateDirectory(dailyLogsDirectory);
        return dailyLogsDirectory;
    }

    public void UpdateLogger()
    {
        var settings = SettingsManager.LoadSettings();
        var dailyLogsDirectory = GetDailyLogsDirectory();

        if (string.IsNullOrEmpty(settings.LogFormat))
        {
            logger = null;
            Console.WriteLine("Aucun format de log défini. Veuillez choisir un format dans les paramètres.");
        }
        else
        {
            if (settings.LogFormat.ToUpper() == "XML")
            {
                logger = new XmlLogManager(dailyLogsDirectory);
                Console.WriteLine("Created XML logger");
            }
            else if (settings.LogFormat.ToUpper() == "JSON")
            {
                logger = new JsonLogManager(dailyLogsDirectory);
                Console.WriteLine("Created JSON logger");
            }
        }
    }

    public BackupManager()
    {
        _syncContext = SynchronizationContext.Current ?? new SynchronizationContext();
        backupJobs = new List<BackupJob>();
        
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var projectRoot = Directory.GetParent(baseDirectory)?.Parent?.Parent?.Parent?.FullName;
        if (projectRoot == null)
            throw new InvalidOperationException("Could not determine project root directory");

        logsPath = Path.Combine(projectRoot, "Logs");
        Directory.CreateDirectory(logsPath);

        var settings = SettingsManager.LoadSettings();
        businessSoftwareService = new BusinessSoftwareService(settings.BusinessSoftware);
        cryptoService = new CryptoSoftService();
        stateManager = new StateManager();

        LoadExistingBackups();
        UpdateLogger();
    }

    public void UpdateBusinessSoftware()
    {
        var settings = SettingsManager.LoadSettings();
        var newService = new BusinessSoftwareService(settings.BusinessSoftware);
        businessSoftwareService = newService;
    }

    private void LoadExistingBackups()
    {
        var backupsPath = Path.Combine(logsPath, "Backups");
        if (!Directory.Exists(backupsPath))
        {
            Directory.CreateDirectory(backupsPath);
            return;
        }

        // Charger les fichiers JSON
        var jsonFiles = Directory.GetFiles(backupsPath, "*.json");
        foreach (var file in jsonFiles)
        {
            try
            {
                var json = File.ReadAllText(file);
                var backup = JsonSerializer.Deserialize<BackupJob>(json);
                if (backup != null && !backupJobs.Any(b => b.Name == backup.Name))
                {
                    backupJobs.Add(backup);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading backup from {file}: {ex.Message}");
            }
        }

        // Charger les fichiers XML
        var xmlFiles = Directory.GetFiles(backupsPath, "*.xml");
        foreach (var file in xmlFiles)
        {
            try
            {
                var doc = XDocument.Load(file);
                var jobElement = doc.Root;
                if (jobElement != null)
                {
                    var backup = new BackupJob
                    {
                        Name = jobElement.Element("Name")?.Value ?? "",
                        SourcePath = jobElement.Element("SourcePath")?.Value ?? "",
                        DestinationPath = jobElement.Element("DestinationPath")?.Value ?? "",
                        FileSize = (int)long.Parse(jobElement.Element("FileSize")?.Value ?? "0"),
                        Status = Enum.TryParse<BackupStatus>(jobElement.Element("Status")?.Value, out var status) ? status : BackupStatus.Pending,
                        IsPriority = bool.Parse(jobElement.Element("IsPriority")?.Value ?? "false"),
                        Type = Enum.TryParse<BackupType>(jobElement.Element("Type")?.Value, out var type) ? type : BackupType.FULL
                    };
                    
                    if (!backupJobs.Any(b => b.Name == backup.Name))
                    {
                        backupJobs.Add(backup);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading backup from {file}: {ex.Message}");
            }
        }
    }

    public void SaveBackupJob(BackupJob job)
    {
        var backupsPath = Path.Combine(logsPath, "Backups");
        Directory.CreateDirectory(backupsPath);
        
        // Toujours sauvegarder les jobs en JSON
        SaveBackupJobAsJson(job);

        if (!backupJobs.Any(b => b.Name == job.Name))
        {
            backupJobs.Add(job);
        }
    }

    private void SaveBackupJobAsJson(BackupJob job)
    {
        var filePath = Path.Combine(logsPath, "Backups", $"{job.Name}.json");
        var json = JsonSerializer.Serialize(job, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(filePath, json);
    }

    public void RemoveBackup(string name)
    {
        var job = backupJobs.FirstOrDefault(j => j.Name == name);
        if (job != null)
        {
            backupJobs.Remove(job);
            var filePath = Path.Combine(logsPath, "Backups", $"{name}.json");
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }

    public async Task ExecuteJobs(IEnumerable<string> jobNames)
    {
        var jobs = backupJobs.Where(j => jobNames.Contains(j.Name)).ToList();
        if (!jobs.Any())
        {
            return;
        }

        // Vérifier immédiatement si un logiciel métier est en cours d'exécution
        if (businessSoftwareService.IsBusinessSoftwareRunning())
        {
            foreach (var job in jobs)
            {
                _syncContext.Post(_ => 
                {
                    OnBackupComplete?.Invoke(job.Name, false, "Un logiciel métier est en cours d'exécution. La sauvegarde ne peut pas démarrer.");
                }, null);
            }
            return;
        }

        var settings = SettingsManager.LoadSettings();
        using var cts = new CancellationTokenSource();

        try
        {
            // Trier les jobs par priorité
            var priorityJobs = new List<BackupJob>();
            var nonPriorityJobs = new List<BackupJob>();

            foreach (var job in jobs)
            {
                if (string.IsNullOrEmpty(job.SourcePath)) continue;
                
                var sourceDir = new DirectoryInfo(job.SourcePath);
                if (!sourceDir.Exists) continue;

                var hasPriorityFiles = sourceDir.GetFiles("*.*", SearchOption.AllDirectories)
                    .Any(file => IsPriorityFile(file.FullName, settings.PriorityExtensions ?? Array.Empty<string>()));

                if (hasPriorityFiles)
                    priorityJobs.Add(job);
                else
                    nonPriorityJobs.Add(job);
            }

            // Exécuter les jobs prioritaires d'abord
            foreach (var job in priorityJobs.Concat(nonPriorityJobs))
            {
                // Vérifier à nouveau avant chaque job
                if (businessSoftwareService.IsBusinessSoftwareRunning())
                {
                    _syncContext.Post(_ => 
                    {
                        OnBackupComplete?.Invoke(job.Name, false, "Un logiciel métier est en cours d'exécution. La sauvegarde est annulée.");
                    }, null);
                    return;
                }

                try
                {
                    job.Status = BackupStatus.Running;
                    await ExecuteBackupJob(job);
                }
                catch (Exception ex)
                {
                    job.Status = BackupStatus.Failed;
                    _syncContext.Post(_ => 
                    {
                        OnBackupComplete?.Invoke(job.Name, false, $"Erreur pendant l'exécution : {ex.Message}");
                    }, null);
                }
            }
        }
        finally
        {
            cts.Cancel();
        }
    }

    private async Task ExecuteBackupJob(BackupJob job)
    {
        if (string.IsNullOrEmpty(job.SourcePath) || string.IsNullOrEmpty(job.DestinationPath))
        {
            job.Status = BackupStatus.Failed;
            UpdateJobState(job, 0, "Chemins source ou destination invalides");
            _syncContext.Post(_ => 
            {
                OnBackupComplete?.Invoke(job.Name, false, "Chemins source ou destination invalides");
            }, null);
            return;
        }

        var sourceDir = new DirectoryInfo(job.SourcePath);
        var destDir = new DirectoryInfo(job.DestinationPath);

        if (!sourceDir.Exists)
        {
            job.Status = BackupStatus.Failed;
            UpdateJobState(job, 0, "Le répertoire source n'existe pas");
            _syncContext.Post(_ => 
            {
                OnBackupComplete?.Invoke(job.Name, false, "Le répertoire source n'existe pas");
            }, null);
            return;
        }

        Directory.CreateDirectory(destDir.FullName);
        job.Status = BackupStatus.Running;
        UpdateJobState(job, 0, "Démarrage de la sauvegarde");

        try
        {
            var files = sourceDir.GetFiles("*.*", SearchOption.AllDirectories);
            var totalSize = files.Sum(f => f.Length);
            var processedSize = 0L;

            foreach (var file in files)
            {
                // Vérifier si un logiciel métier est lancé pendant la copie
                if (businessSoftwareService.IsBusinessSoftwareRunning())
                {
                    job.Status = BackupStatus.Failed;
                    UpdateJobState(job, (int)((processedSize * 100.0) / totalSize), "Un logiciel métier a été détecté. La sauvegarde est annulée.");
                    _syncContext.Post(_ => 
                    {
                        OnBackupComplete?.Invoke(job.Name, false, "Un logiciel métier a été détecté. La sauvegarde est annulée.");
                    }, null);
                    
                    logger.LogTransfer(new TransferInfo
                    {
                        Name = job.Name,
                        SourceFile = file.FullName,
                        TargetFile = Path.Combine(destDir.FullName, file.Name),
                        FileSize = file.Length,
                        TransferTime = 0,
                        CryptTime = 0,
                        BackupType = job.Type.ToString(),
                        Status = "Failed",
                        Message = "Sauvegarde annulée : logiciel métier détecté",
                        SourceFileHash = CalculateFileHash(file.FullName),
                        TargetFileHash = null
                    });
                    return;
                }

                var startTime = DateTime.Now;
                var relativePath = file.FullName.Substring(sourceDir.FullName.Length + 1);
                var targetPath = Path.Combine(destDir.FullName, relativePath);
                var targetDir = Path.GetDirectoryName(targetPath);
                long cryptTime = 0;

                if (!string.IsNullOrEmpty(targetDir))
                    Directory.CreateDirectory(targetDir);

                try
                {
                    // Vérifier si le fichier doit être crypté
                    string sourceHash = CalculateFileHash(file.FullName);

                    if (ShouldEncryptFile(file.Extension))
                    {
                        var cryptStartTime = DateTime.Now;
                        await cryptoService.EncryptFile(file.FullName, targetPath);
                        cryptTime = (long)(DateTime.Now - cryptStartTime).TotalMilliseconds;
                    }
                    else
                    {
                        file.CopyTo(targetPath, true);
                    }

                    string targetHash = CalculateFileHash(targetPath);
                    processedSize += file.Length;
                    var transferTime = (long)(DateTime.Now - startTime).TotalMilliseconds;

                    logger.LogTransfer(new TransferInfo
                    {
                        Name = job.Name,
                        SourceFile = file.FullName,
                        TargetFile = targetPath,
                        FileSize = file.Length,
                        TransferTime = transferTime,
                        CryptTime = cryptTime,
                        BackupType = job.Type.ToString(),
                        Status = "Success",
                        SourceFileHash = sourceHash,
                        TargetFileHash = targetHash
                    });

                    var progress = (int)((processedSize * 100.0) / totalSize);
                    UpdateJobState(job, progress, $"Copie en cours : {progress}%");
                    _syncContext.Post(_ => 
                    {
                        OnProgress?.Invoke(job.Name, progress);
                    }, null);
                }
                catch (Exception ex)
                {
                    logger.LogTransfer(new TransferInfo
                    {
                        Name = job.Name,
                        SourceFile = file.FullName,
                        TargetFile = targetPath,
                        FileSize = file.Length,
                        TransferTime = 0,
                        CryptTime = 0,
                        BackupType = job.Type.ToString(),
                        Status = "Failed",
                        Message = ex.Message,
                        SourceFileHash = null,
                        TargetFileHash = null
                    });
                    throw;
                }
            }

            job.Status = BackupStatus.Completed;
            UpdateJobState(job, 100, "Sauvegarde terminée avec succès");
            _syncContext.Post(_ => 
            {
                OnBackupComplete?.Invoke(job.Name, true, null);
            }, null);
        }
        catch (Exception ex)
        {
            job.Status = BackupStatus.Failed;
            UpdateJobState(job, 0, $"Erreur : {ex.Message}");
            _syncContext.Post(_ => 
            {
                OnBackupComplete?.Invoke(job.Name, false, ex.Message);
            }, null);
            throw;
        }
    }

    private void UpdateJobState(BackupJob job, int progress, string message)
    {
        var state = new JobState
        {
            JobName = job.Name,
            Status = job.Status,
            Progress = progress,
            Message = message,
            LastUpdate = DateTime.Now
        };
        stateManager.SaveState(state);
    }

    public void StopAll()
    {
        // TODO: implémenter la méthode StopAll
    }

    public void StopJob(string jobName)
    {
        // TODO: implémenter la méthode StopJob
    }

    private bool ShouldEncryptFile(string extension)
    {
        var settings = SettingsManager.LoadSettings();
        return settings.EncryptExtensions.Contains(extension.ToLower());
    }

    private async Task CopyFileAsync(string sourcePath, string targetPath)
    {
        using var sourceStream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read);
        using var targetStream = new FileStream(targetPath, FileMode.Create, FileAccess.Write);
        await sourceStream.CopyToAsync(targetStream);
    }

    private int GetCurrentNetworkLoad()
    {
        var interfaces = NetworkInterface.GetAllNetworkInterfaces();
        long bytesSent = 0;
        long bytesReceived = 0;

        foreach (var ni in interfaces)
        {
            var stats = ni.GetIPv4Statistics();
            bytesSent += stats.BytesSent;
            bytesReceived += stats.BytesReceived;
        }

        // définir un seuil de référence pour calculer la charge réseau
        long totalBytes = bytesSent + bytesReceived;
        long referenceBytes = 1000000000; // Par exemple, 1 Go

        int networkLoad = (int)((totalBytes * 100) / referenceBytes);
        return Math.Min(networkLoad, 100); // on s'Assure que la charge ne dépasse pas 100%
    }

    private string CalculateFileHash(string filePath)
    {
        try
        {
            using var md5 = MD5.Create();
            using var stream = File.OpenRead(filePath);
            var hash = md5.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
        catch (Exception)
        {
            return "hash_error";
        }
    }

    private string CalculateDirectoryHash(string directoryPath)
    {
        try
        {
            var directory = new DirectoryInfo(directoryPath);
            if (!directory.Exists) return "directory_not_found";

            using var md5 = MD5.Create();
            var files = directory.GetFiles("*.*", SearchOption.AllDirectories)
                               .OrderBy(f => f.FullName)
                               .ToList();

            foreach (var file in files)
            {
                var relativePath = file.FullName.Substring(directory.FullName.Length);
                var pathBytes = Encoding.UTF8.GetBytes(relativePath.ToLowerInvariant());
                md5.TransformBlock(pathBytes, 0, pathBytes.Length, pathBytes, 0);

                var fileHash = CalculateFileHash(file.FullName);
                var hashBytes = Encoding.UTF8.GetBytes(fileHash);
                md5.TransformBlock(hashBytes, 0, hashBytes.Length, hashBytes, 0);
            }

            md5.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
            return BitConverter.ToString(md5.Hash!).Replace("-", "").ToLowerInvariant();
        }
        catch (Exception)
        {
            return "directory_hash_error";
        }
    }
}
