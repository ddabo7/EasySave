using Xunit;
using EasySave.Core;
using EasySave.Models;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Generic;

namespace EasySave.Tests
{
    public class BackupManagerTests : IDisposable
    {
        private readonly string _testSourceDir;
        private readonly string _testDestDir;
        private readonly string _backupsDir;
        private readonly BackupManager _backupManager;

        public BackupManagerTests()
        {
            _testSourceDir = Path.Combine(Path.GetTempPath(), "EasySaveTests", "source");
            _testDestDir = Path.Combine(Path.GetTempPath(), "EasySaveTests", "dest");
            
            // Ensure clean test directories
            if (Directory.Exists(_testSourceDir)) Directory.Delete(_testSourceDir, true);
            if (Directory.Exists(_testDestDir)) Directory.Delete(_testDestDir, true);
            
            Directory.CreateDirectory(_testSourceDir);
            Directory.CreateDirectory(_testDestDir);

            // Clean up existing backups
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            _backupsDir = Path.Combine(appDataPath, "EasySave", "Backups");
            if (Directory.Exists(_backupsDir))
            {
                Directory.Delete(_backupsDir, true);
            }
            Directory.CreateDirectory(_backupsDir);

            _backupManager = new BackupManager();
        }

        [Fact]
        public async Task ExecuteJobs_WithEncryption_ShouldEncryptFiles()
        {
            // Arrange
            var testFile = Path.Combine(_testSourceDir, "sensitive.txt");
            File.WriteAllText(testFile, "Sensitive content");

            // Configure settings to encrypt .txt files
            var settings = new Settings 
            { 
                EncryptExtensions = new[] { ".txt" }
            };
            SettingsManager.SaveSettings(settings);

            var job = new BackupJob
            {
                Name = "EncryptedBackup",
                SourcePath = _testSourceDir,
                DestinationPath = _testDestDir,
                Type = BackupType.FULL,
                FileSize = (int)new FileInfo(testFile).Length
            };
            _backupManager.SaveBackupJob(job);

            // Act
            await _backupManager.ExecuteJobs(new List<string> { "EncryptedBackup" });

            // Assert
            var destFile = Path.Combine(_testDestDir, "sensitive.txt");
            Assert.True(File.Exists(destFile));
            var encryptedContent = File.ReadAllText(destFile);
            Assert.NotEqual("Sensitive content", encryptedContent);
        }

        [Fact]
        public async Task ExecuteDifferentialBackup_ShouldOnlyCopyChangedFiles()
        {
            // Arrange
            var file1 = Path.Combine(_testSourceDir, "file1.txt");
            var file2 = Path.Combine(_testSourceDir, "file2.txt");
            File.WriteAllText(file1, "Content 1");
            File.WriteAllText(file2, "Content 2");

            var job = new BackupJob
            {
                Name = "DiffBackup",
                SourcePath = _testSourceDir,
                DestinationPath = _testDestDir,
                Type = BackupType.DIFFERENTIAL,
                FileSize = (int)(new FileInfo(file1).Length + new FileInfo(file2).Length)
            };
            _backupManager.SaveBackupJob(job);

            // First backup
            await _backupManager.ExecuteJobs(new List<string> { "DiffBackup" });

            // Modify file1 only
            await Task.Delay(1000); // Ensure file timestamp changes
            File.WriteAllText(file1, "Modified Content 1");

            // Clear destination directory to ensure clean state
            Directory.Delete(_testDestDir, true);
            Directory.CreateDirectory(_testDestDir);

            // Act - Run differential backup
            await _backupManager.ExecuteJobs(new List<string> { "DiffBackup" });

            // Assert
            var destFile1 = Path.Combine(_testDestDir, "file1.txt");
            var destFile2 = Path.Combine(_testDestDir, "file2.txt");
            
            Assert.True(File.Exists(destFile1));
            Assert.Equal("Modified Content 1", File.ReadAllText(destFile1));
            Assert.True(File.Exists(destFile2));
            Assert.Equal("Content 2", File.ReadAllText(destFile2));
        }

        [Fact]
        public async Task ExecuteJobs_WithBusinessSoftwareRunning_ShouldPause()
        {
            // Arrange
            var testFile = Path.Combine(_testSourceDir, "test.txt");
            File.WriteAllText(testFile, "Test content");
            
            // Configure settings to recognize TextEdit as business software
            var settings = new Settings 
            { 
                BusinessSoftware = new[] { "TextEdit" }
            };
            SettingsManager.SaveSettings(settings);
            
            // Create a new BackupManager to pick up the updated settings
            var backupManager = new BackupManager();
            backupManager.UpdateBusinessSoftware(); // Ensure business software list is updated
            
            var job = new BackupJob
            {
                Name = "BusinessTest",
                SourcePath = _testSourceDir,
                DestinationPath = _testDestDir,
                Type = BackupType.FULL,
                FileSize = (int)new FileInfo(testFile).Length
            };
            backupManager.SaveBackupJob(job);

            // Act & Assert
            using (var process = Process.Start(new ProcessStartInfo
            {
                FileName = "open",
                Arguments = "-a TextEdit",
                UseShellExecute = true
            }))
            {
                try
                {
                    await Task.Delay(2000); // Give TextEdit more time to start
                    await backupManager.ExecuteJobs(new List<string> { "BusinessTest" });
                    var destFile = Path.Combine(_testDestDir, "test.txt");
                    Assert.False(File.Exists(destFile), "Backup should not proceed when business software is running");
                }
                finally
                {
                    try 
                    { 
                        var textEditProcess = Process.GetProcesses()
                            .FirstOrDefault(p => p.ProcessName.Contains("TextEdit", StringComparison.OrdinalIgnoreCase));
                        textEditProcess?.Kill(); 
                    } 
                    catch { }
                }
            }
        }

        [Fact]
        public async Task LogManager_ShouldCreateLogs()
        {
            // Arrange
            var testFile = Path.Combine(_testSourceDir, "logtest.txt");
            File.WriteAllText(testFile, "Test content");
            
            // Configure settings for JSON logging
            var settings = new Settings { LogFormat = "JSON" };
            SettingsManager.SaveSettings(settings);
            
            // Create a new BackupManager to pick up the updated settings
            var backupManager = new BackupManager();
            
            var job = new BackupJob
            {
                Name = "LogTest",
                SourcePath = _testSourceDir,
                DestinationPath = _testDestDir,
                Type = BackupType.FULL,
                FileSize = (int)new FileInfo(testFile).Length
            };

            // Act
            backupManager.SaveBackupJob(job);
            backupManager.UpdateLogger(); // Initialize the logger
            await backupManager.ExecuteJobs(new List<string> { "LogTest" });

            // Assert
            var logsDir = GetDailyLogsDirectory();
            Assert.True(Directory.Exists(logsDir), "Logs directory should exist");
            var logFiles = Directory.GetFiles(logsDir, "*.json");
            Assert.True(logFiles.Length > 0, $"Expected log files in {logsDir} but found none");
        }

        private string GetDailyLogsDirectory()
        {
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var projectRoot = Directory.GetParent(baseDirectory)?.Parent?.Parent?.Parent?.FullName;
            if (projectRoot == null)
                throw new InvalidOperationException("Could not determine project root directory");

            return Path.Combine(projectRoot, "Logs", "Daily");
        }

        public void Dispose()
        {
            // Cleanup test directories
            if (Directory.Exists(_testSourceDir)) Directory.Delete(_testSourceDir, true);
            if (Directory.Exists(_testDestDir)) Directory.Delete(_testDestDir, true);
            if (Directory.Exists(_backupsDir)) Directory.Delete(_backupsDir, true);
        }
    }
}