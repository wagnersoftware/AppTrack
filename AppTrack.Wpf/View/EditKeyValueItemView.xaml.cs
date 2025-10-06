using AppTrack.WpfUi.ViewModel;
using System.Windows;

namespace AppTrack.WpfUi.View;

/// <summary>
/// Interaction logic for AddKeyValueItemView.xaml
/// </summary>
public partial class EditKeyValueItemView : Window
{
    public EditKeyValueItemView(EditKeyValueItemViewModel viewModel)
    {
        InitializeComponent();
        this.DataContext = viewModel;
    }
}
