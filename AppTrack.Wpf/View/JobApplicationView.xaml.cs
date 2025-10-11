using AppTrack.WpfUi.ViewModel;
using AppTrack.WpfUi.ViewModel.Base;
using System.Windows;

namespace AppTrack.WpfUi.View;

/// <summary>
/// Interaction logic for CreateJobApplicationView.xaml
/// </summary>
public partial class JobApplicationView : Window
{
    public JobApplicationView(JobApplicationViewModelBase viewModel)
    {
        InitializeComponent();
        this.DataContext = viewModel;
    }
}
