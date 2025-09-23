using AppTrack.WpfUi.ViewModel;
using System.Windows;

namespace AppTrack.WpfUi.View;

/// <summary>
/// Interaction logic for SetJobApplicationDefaultsView.xaml
/// </summary>
public partial class SetJobApplicationDefaultsView : Window
{
    public SetJobApplicationDefaultsView(SetJobApplicationDefaultsViewModel viewModel)
    {
        InitializeComponent();
        this.DataContext = viewModel;
    }
}
