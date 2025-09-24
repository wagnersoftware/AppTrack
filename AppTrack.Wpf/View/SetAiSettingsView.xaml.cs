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
    }
}
