using AppTrack.WpfUi.ViewModel;
using System.Windows;

namespace AppTrack.WpfUi.View;

/// <summary>
/// Interaction logic for CreateJobApplicationView.xaml
/// </summary>
public partial class CreateJobApplicationView : Window
{
    public CreateJobApplicationView(CreateJobApplicationViewModel viewModel)
    {
        InitializeComponent();
        this.DataContext = viewModel;
    }
}
