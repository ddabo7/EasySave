using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using EasySave.MVVM.ViewModels;

namespace EasySave.MVVM.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
