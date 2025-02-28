using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using ComboBoxItem = Avalonia.Controls.ComboBoxItem;
using Avalonia.Threading;
using EasySave.Core;
using EasySave.Models;
using EasySave.MVVM.Commands;
using EasySave.MVVM.Views;
using EasySave.Services;

namespace EasySave.MVVM.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly Window _mainWindow;
        private readonly BackupManager _backupManager;
        private readonly ResourceManager _resourceManager;
        private readonly ILogService _logService;
        private ObservableCollection<BackupJob> _backupJobs = new();
        private ObservableCollection<LogEntry> _logEntries = new();
        private string[] _businessSoftware = new string[0];
        private string[] _encryptExtensions = new string[0];
        private string[] _priorityExtensions = Array.Empty<string>();
        private string _logFormat = "";
        private string _language = "en";
        private bool _isRunning;
        private int _selectedTabIndex;
        private int _maxParallelJobs = 5;
        private long _largeFileThreshold = 1024 * 1024;

        public event PropertyChangedEventHandler? PropertyChanged;

        public MainViewModel(Window mainWindow, BackupManager backupManager)
        {
            _mainWindow = mainWindow;
            _backupManager = backupManager;
            _resourceManager = ResourceManager.Instance;
            _logService = new LogService();

            // S'abonner au changement de langue
            _resourceManager.LanguageChanged += (s, e) => Dispatcher.UIThread.Post(() => OnPropertyChanged(string.Empty));

            AddJobCommand = new RelayCommand(ExecuteAddJob);
            RunAllCommand = new RelayCommand(ExecuteRunAll, () => !_isRunning);
            StopAllCommand = new RelayCommand(ExecuteStopAll, () => _isRunning);
            RunJobCommand = new RelayCommand<BackupJob?>(ExecuteRunJob);
            StopJobCommand = new RelayCommand<BackupJob?>(ExecuteStopJob);
            DeleteJobCommand = new RelayCommand<BackupJob?>(ExecuteDeleteJob);
            SaveSettingsCommand = new RelayCommand(ExecuteSaveSettings);
            ChangeLanguageCommand = new RelayCommand<string?>(ExecuteChangeLanguage);
            RefreshLogsCommand = new RelayCommand(ExecuteRefreshLogs);
            ClearLogsCommand = new RelayCommand(ExecuteClearLogs);
            ExitCommand = new RelayCommand(() => _mainWindow.Close());
            GoToSettingsCommand = new RelayCommand(() => SelectedTabIndex = 1);

            LoadSettings();
            LoadLogs();
            _backupManager.OnBackupComplete += OnBackupComplete;
            _backupManager.OnProgress += OnProgress;
        }

        public ObservableCollection<BackupJob> BackupJobs
        {
            get => _backupJobs;
            set
            {
                _backupJobs = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<LogEntry> LogEntries
        {
            get => _logEntries;
            set
            {
                _logEntries = value;
                OnPropertyChanged();
            }
        }

        public string BusinessSoftware
        {
            get => string.Join(",", _businessSoftware);
            set
            {
                _businessSoftware = value.Split(',', StringSplitOptions.RemoveEmptyEntries);
                OnPropertyChanged();
            }
        }

        public string EncryptExtensions
        {
            get => string.Join(",", _encryptExtensions);
            set
            {
                _encryptExtensions = value.Split(',', StringSplitOptions.RemoveEmptyEntries);
                OnPropertyChanged();
            }
        }

        public string PriorityExtensions
        {
            get => string.Join(",", _priorityExtensions);
            set
            {
                _priorityExtensions = value.Split(',', StringSplitOptions.RemoveEmptyEntries);
                OnPropertyChanged();
            }
        }

        public string LogFormat
        {
            get => _logFormat;
            set
            {
                if (value != _logFormat)
                {
                    _logFormat = value;
                    OnPropertyChanged();
                    Console.WriteLine($"Format de log défini sur : {_logFormat}");
                    _backupManager.UpdateLogger(); // Mettre à jour le logger quand le format change
                }
            }
        }

        public string Language
        {
            get => _language;
            set
            {
                if (_language != value)
                {
                    _language = value;
                    _resourceManager.SetLanguage(value);
                    OnPropertyChanged();
                }
            }
        }

        public ICommand AddJobCommand { get; }
        public ICommand RunAllCommand { get; }
        public ICommand StopAllCommand { get; }
        public ICommand RunJobCommand { get; }
        public ICommand StopJobCommand { get; }
        public ICommand DeleteJobCommand { get; }
        public ICommand SaveSettingsCommand { get; }
        public ICommand ChangeLanguageCommand { get; }
        public ICommand RefreshLogsCommand { get; }
        public ICommand ClearLogsCommand { get; }
        public ICommand ExitCommand { get; }
        public ICommand GoToSettingsCommand { get; }

        public int SelectedTabIndex
        {
            get => _selectedTabIndex;
            set
            {
                _selectedTabIndex = value;
                OnPropertyChanged();
            }
        }

        public int MaxParallelJobs
        {
            get => _maxParallelJobs;
            set
            {
                _maxParallelJobs = value;
                OnPropertyChanged();
            }
        }

        public long LargeFileThreshold
        {
            get => _largeFileThreshold / (1024 * 1024); // Convertir bytes en MB
            set
            {
                _largeFileThreshold = value * 1024 * 1024; // Convertir MB en bytes
                OnPropertyChanged();
            }
        }

        public void LoadSettings()
        {
            var settings = SettingsManager.LoadSettings();
            BusinessSoftware = string.Join(",", settings.BusinessSoftware);
            EncryptExtensions = string.Join(",", settings.EncryptExtensions);
            PriorityExtensions = string.Join(",", settings.PriorityExtensions);
            LogFormat = settings.LogFormat;
            Language = settings.Language;
            MaxParallelJobs = settings.MaxParallelJobs;
            LargeFileThreshold = settings.LargeFileThreshold;
            OnPropertyChanged(nameof(BusinessSoftware));
            OnPropertyChanged(nameof(EncryptExtensions));
            OnPropertyChanged(nameof(PriorityExtensions));
        }

        private void LoadLogs()
        {
            var logs = _logService.LoadLogs();
            LogEntries = new ObservableCollection<LogEntry>(logs);
        }

        private void ExecuteSaveSettings()
        {
            var settings = new Settings
            {
                BusinessSoftware = BusinessSoftware.Split(',', StringSplitOptions.RemoveEmptyEntries),
                EncryptExtensions = EncryptExtensions.Split(',', StringSplitOptions.RemoveEmptyEntries),
                PriorityExtensions = PriorityExtensions.Split(',', StringSplitOptions.RemoveEmptyEntries),
                LogFormat = LogFormat,
                Language = Language,
                MaxParallelJobs = MaxParallelJobs,
                LargeFileThreshold = LargeFileThreshold
            };
            SettingsManager.SaveSettings(settings);
            _backupManager.UpdateLogger(); // Mettre à jour le logger après la sauvegarde
        }

        private async void ExecuteAddJob()
        {
            var dialog = new AddBackupJobDialog();
            var result = await dialog.ShowDialog<BackupJob>(_mainWindow);
            
            if (result != null)
            {
                BackupJobs.Add(result);
                _backupManager.SaveBackupJob(result);
            }
        }

        private async void ExecuteRunAll()
        {
            if (_isRunning) return;
            
            _isRunning = true;
            foreach (var job in BackupJobs)
            {
                job.Status = BackupStatus.Pending;
            }

            try
            {
                await _backupManager.ExecuteJobs(BackupJobs.Select(j => j.Name).ToList());
            }
            catch (Exception ex)
            {
                // Une erreur globale s'est produite
                foreach (var job in BackupJobs.Where(j => j.Status == BackupStatus.Pending))
                {
                    job.Status = BackupStatus.Failed;
                }
                Console.WriteLine($"Error during backup execution: {ex.Message}");
            }
            finally
            {
                _isRunning = false;
            }
        }

        private void ExecuteStopAll()
        {
            _isRunning = false;
            _backupManager.StopAll();
            foreach (var job in BackupJobs.Where(j => j.Status == BackupStatus.Running))
            {
                job.Status = BackupStatus.Stopped;
            }
        }

        private async void ExecuteRunJob(BackupJob? job)
        {
            if (job == null || _isRunning) return;

            _isRunning = true;
            job.Status = BackupStatus.Pending;

            try
            {
                await _backupManager.ExecuteJobs(new[] { job.Name });
            }
            catch (Exception ex)
            {
                job.Status = BackupStatus.Failed;
                Console.WriteLine($"Error executing job {job.Name}: {ex.Message}");
            }
            finally
            {
                _isRunning = false;
            }
        }

        private void ExecuteStopJob(BackupJob? job)
        {
            if (job == null) return;
            _backupManager.StopJob(job.Name);
            if (job.Status == BackupStatus.Running)
            {
                job.Status = BackupStatus.Stopped;
            }
        }

        private void ExecuteDeleteJob(BackupJob? job)
        {
            if (job == null) return;
            _backupManager.RemoveBackup(job.Name);
            BackupJobs.Remove(job);
        }

        private void ExecuteChangeLanguage(string? language)
        {
            if (string.IsNullOrEmpty(language)) return;

            // Changer la langue et sauvegarder les paramètres
            Language = language;
            ExecuteSaveSettings();
        }

        private void ExecuteRefreshLogs()
        {
            LoadLogs();
        }

        private void ExecuteClearLogs()
        {
            _logService.ClearLogs();
            LogEntries.Clear();
        }

        private void OnBackupComplete(string jobName, bool success, string? error)
        {
            var job = BackupJobs.FirstOrDefault(j => j.Name == jobName);
            if (job != null)
            {
                job.Status = success ? BackupStatus.Completed : BackupStatus.Stopped;

                var logEntry = new LogEntry
                {
                    Timestamp = DateTime.Now,
                    JobName = jobName,
                    Status = job.Status,
                    FileSource = job.SourceFile,
                    FileTarget = job.TargetFile
                };

                _logService.LogEntry(logEntry);
                LogEntries.Add(logEntry);

                if (!success && error != null)
                {
                    Console.WriteLine($"Backup failed for job {jobName}: {error}");
                }
            }
        }

        private void OnProgress(string jobName, int progress)
        {
            var job = BackupJobs.FirstOrDefault(j => j.Name == jobName);
            if (job != null)
            {
                job.Status = BackupStatus.Running;
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
