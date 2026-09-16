using System.Windows.Controls;

namespace Reminder.Installer.Wizard
{
    /// <summary>
    /// Interaction logic for SecretsPage.xaml
    /// </summary>
    public partial class SecretsPage : Page
    {
        public SecretsPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is MainWindowViewModel viewModel && viewModel.ShowPassword)
                tbUnlockSecretsPassword.FontFamily = sourceFont.FontFamily;
            tbUnlockSecretsPassword.Focus();
        }

        private void OnToggleShowPassword(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            MainWindowViewModel viewModel = (MainWindowViewModel)DataContext;
            bool showPassword = viewModel.ToggleShowPassword();
            if (showPassword)
                tbUnlockSecretsPassword.FontFamily = sourceFont.FontFamily;
            else
                tbUnlockSecretsPassword.FontFamily = (System.Windows.Media.FontFamily)Resources["passwordFont"];
        }
    }
}
