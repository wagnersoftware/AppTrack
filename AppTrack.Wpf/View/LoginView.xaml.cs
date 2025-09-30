using AppTrack.WpfUi.ViewModel;
using System.Windows;
using System.Windows.Controls;

namespace AppTrack.WpfUi.View
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginView : Window
    {
        public LoginView(LoginViewModel viewModel)
        {
            InitializeComponent();
            this.DataContext = viewModel;

            viewModel.CloseAction = dlgResult =>
            {
                this.DialogResult = dlgResult;
                this.Close();
            };
            
            PasswordBox.PasswordChanged += (s, e) =>
            {
                viewModel.Password = PasswordBox.Password;              
            };
        }
    }
}
