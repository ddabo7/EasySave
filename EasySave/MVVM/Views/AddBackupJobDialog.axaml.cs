using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using EasySave.MVVM.ViewModels;

namespace EasySave.MVVM.Views
{
    public partial class AddBackupJobDialog : Window
    {
        public AddBackupJobDialog()
        {
            InitializeComponent();
            DataContext = new AddBackupJobViewModel(this);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
