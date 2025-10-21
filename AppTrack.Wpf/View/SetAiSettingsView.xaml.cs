using AppTrack.WpfUi.Helpers;
using AppTrack.WpfUi.ViewModel;
using System.Windows;

namespace AppTrack.WpfUi.View;

/// <summary>
/// Interaction logic for SetAiSettingsView.xaml
/// </summary>
public partial class SetAiSettingsView : Window
{
    public SetAiSettingsView(SetAiSettingsViewModel viewModel)
    {
        InitializeComponent();
        this.DataContext = viewModel;

        PasswordBox.PasswordChanged += (s, e) =>
        {
            PasswordBoxHelper.PasswordBox_PasswordChanged(s, e);
            viewModel.Model.ApiKey = PasswordBox.Password;
        };
    }
}
