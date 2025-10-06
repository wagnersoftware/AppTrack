using AppTrack.WpfUi.ViewModel;
using System.Windows;

namespace AppTrack.WpfUi.View;

/// <summary>
/// Interaction logic for EditPromptParameterView.xaml
/// </summary>
public partial class EditPromptParameterView : Window
{
    public EditPromptParameterView(EditPromptParameterViewModel viewModel)
    {
        InitializeComponent();
        this.DataContext = viewModel;
    }
}
