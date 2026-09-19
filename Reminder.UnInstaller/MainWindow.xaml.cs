using System.ComponentModel;
using System.Windows;

namespace Reminder.UnInstaller
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            MainWindowViewModel viewModel = (MainWindowViewModel)DataContext;
            e.Cancel = viewModel.IsUninslalling;
        }
    }
}