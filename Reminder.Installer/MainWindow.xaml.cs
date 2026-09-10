using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Page = System.Windows.Controls.Page;

namespace Reminder.Installer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private new MainWindowViewModel DataContext
        {
            get { return (MainWindowViewModel)base.DataContext; }
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = DataContext.IsInstalling;
        }

        private void Frame_OnNavigated(object sender, NavigationEventArgs e)
        {
            var page = (Page)e.Content;
            DataContext.Title = page.Title;
            page.DataContext = DataContext;
        }

        private void FrameworkElement_OnLoaded(object sender, RoutedEventArgs e)
        {
            DataContext.NavigationService = ((Frame)sender).NavigationService;
        }
    }
}
