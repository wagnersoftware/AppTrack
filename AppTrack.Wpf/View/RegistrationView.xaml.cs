using AppTrack.WpfUi.ViewModel;
using System.Windows;

namespace AppTrack.WpfUi.View;

/// <summary>
/// Interaction logic for RegistrationView.xaml
/// </summary>
public partial class RegistrationView : Window
{
    public RegistrationView(RegistrationViewModel viewModel)
    {
        InitializeComponent();
        this.DataContext = viewModel;

        PasswordBox.PasswordChanged += (s, e) =>
        {
            viewModel.Model.Password = PasswordBox.Password;
        };

        ConfirmPasswordBox.PasswordChanged += (s, e) =>
        {
            viewModel.Model.ConfirmPassword = ConfirmPasswordBox.Password;
        };
    }
}
