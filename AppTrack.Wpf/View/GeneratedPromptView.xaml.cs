using AppTrack.WpfUi.ViewModel;
using System.Windows;

namespace AppTrack.WpfUi.View;

/// <summary>
/// Interaction logic for GeneratedPromptView.xaml
/// </summary>
public partial class GeneratedPromptView : Window
{
    public GeneratedPromptView(GeneratedPromptViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
