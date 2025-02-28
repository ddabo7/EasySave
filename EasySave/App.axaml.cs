using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using EasySave.Core;
using EasySave.MVVM.ViewModels;
using EasySave.MVVM.Views;
using EasySave.Services;
using EasySave.UI;
using System.Threading.Tasks;

namespace EasySave
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var settings = SettingsManager.LoadSettings();
                var backupManager = new BackupManager();
                var remoteConsole = new RemoteConsole(backupManager);
                
                var mainWindow = new MainWindow();
                mainWindow.DataContext = new MainViewModel(mainWindow, backupManager);
                desktop.MainWindow = mainWindow;

                // Démarrer la console déportée en arrière-plan
                Task.Run(() => remoteConsole.StartAsync());
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
