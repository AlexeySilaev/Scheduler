using Scheduler.ViewModels;
using System.ComponentModel;
using System.Windows;

namespace Scheduler
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private new MainViewModel DataContext
        {
            get { return (MainViewModel)base.DataContext; }
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            DataContext.InitializeAsync(Dispatcher);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            DataContext.OnClosing();
        }
    }
}
