using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            tbSecretPassword.Focus();
            // без этого вызовы чекбоксы инициализируются через раз
            ManagementViewModel managementViewModel = (ManagementViewModel)DataContext;
            managementViewModel.RefreshBindings();
        }
        private void OnSecretsPasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordBox pb = (PasswordBox)sender;
            ManagementViewModel managementViewModel = (ManagementViewModel)DataContext;
            managementViewModel.OnSecretsPasswordChanged(pb.Password.ToString());
        }

        private void OnKeyDownInSecrectPassword(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) // или Key.Return
            {
                ManagementViewModel managementViewModel = (ManagementViewModel)DataContext;
                if (managementViewModel.LockOrUnlockSecrectsCommand.CanExecute(null))
                {
                    managementViewModel.LockOrUnlockSecrectsCommand.Execute(null);
                    e.Handled = true;
                }
            }
        }

    }
}
