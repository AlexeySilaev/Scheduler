using System.Windows;
using System.Windows.Controls;
using static System.Net.Mime.MediaTypeNames;

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

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            tbSecretPassword.Focus();
            // без этого вызовы чекбоксы инициализируются через раз
            ManagementViewModel managementViewModel = (ManagementViewModel)DataContext;
            managementViewModel.RefreshBindings();
        }
    }
}
