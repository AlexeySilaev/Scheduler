using System.Windows;
using System.Windows.Controls;

namespace Reminder.UI
{
    /// <summary>
    /// Interaction logic for ManagementControl.xaml
    /// </summary>
    public partial class ManagementControl : UserControl
    {
        public ManagementControl()
        {
            InitializeComponent();
        }

        private void OnSecretsPasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordBox pb = (PasswordBox)sender;
            ManagementViewModel managementViewModel = (ManagementViewModel)DataContext;
            managementViewModel.OnSecretsPasswordChanged(pb.Password.ToString());
        }
    }
}
