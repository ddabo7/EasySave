using System.Windows.Input;
using Avalonia.Controls;
using EasySave.Models;
using EasySave.MVVM.Commands;

namespace EasySave.MVVM.ViewModels;

public class AddBackupJobViewModel : ViewModelBase
{
    private string name = string.Empty;
    private string sourcePath = string.Empty;
    private string destinationPath = string.Empty;
    private BackupType backupType = BackupType.FULL;
    private readonly Window dialog;

    public AddBackupJobViewModel(Window dialogWindow)
    {
        dialog = dialogWindow;
        SaveCommand = new RelayCommand(ExecuteSave, CanExecuteSave);
        CancelCommand = new RelayCommand(ExecuteCancel);
        BrowseSourceCommand = new RelayCommand(ExecuteBrowseSource);
        BrowseTargetCommand = new RelayCommand(ExecuteBrowseTarget);
    }

    public string Name
    {
        get => name;
        set
        {
            name = value;
            OnPropertyChanged(nameof(Name));
            (SaveCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }
    }

    public string SourcePath
    {
        get => sourcePath;
        set
        {
            sourcePath = value;
            OnPropertyChanged(nameof(SourcePath));
            (SaveCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }
    }

    public string DestinationPath
    {
        get => destinationPath;
        set
        {
            destinationPath = value;
            OnPropertyChanged(nameof(DestinationPath));
            (SaveCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }
    }

    public BackupType BackupType
    {
        get => backupType;
        set
        {
            backupType = value;
            OnPropertyChanged(nameof(BackupType));
        }
    }

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand BrowseSourceCommand { get; }
    public ICommand BrowseTargetCommand { get; }

    private bool CanExecuteSave()
    {
        return !string.IsNullOrWhiteSpace(Name) &&
               !string.IsNullOrWhiteSpace(SourcePath) &&
               !string.IsNullOrWhiteSpace(DestinationPath);
    }

    private void ExecuteSave()
    {
        var job = new BackupJob
        {
            Name = Name,
            SourcePath = SourcePath,
            DestinationPath = DestinationPath,
            Type = BackupType,
            Status = BackupStatus.Pending
        };

        dialog.Close(job);
    }

    private void ExecuteCancel()
    {
        dialog.Close(null);
    }

    private async void ExecuteBrowseSource()
    {
        var result = await dialog.StorageProvider.OpenFolderPickerAsync(new Avalonia.Platform.Storage.FolderPickerOpenOptions
        {
            Title = "Select Source Directory"
        });
    
        if (result != null && result.Count > 0)
        {
            SourcePath = result[0].Path.LocalPath;
        }
    }
    
    private async void ExecuteBrowseTarget()
    {
        var result = await dialog.StorageProvider.OpenFolderPickerAsync(new Avalonia.Platform.Storage.FolderPickerOpenOptions
        {
            Title = "Select Target Directory"
        });
    
        if (result != null && result.Count > 0)
        {
            DestinationPath = result[0].Path.LocalPath;
        }
    }
}
