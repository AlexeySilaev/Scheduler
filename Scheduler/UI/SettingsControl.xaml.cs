using System.Windows;
using System.Windows.Controls;

namespace Scheduler.UI
{
    /// <summary>
    /// Interaction logic for SettingsControl.xaml
    /// </summary>
    public partial class SettingsControl : UserControl
    {
        public SettingsControl()
        {
            InitializeComponent();
        }

        private void SettingsControl_OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            SettingsViewModel viewModel = DataContext as SettingsViewModel;
            if (viewModel != null)
                viewModel.Media = Media;
        }
    }
}
