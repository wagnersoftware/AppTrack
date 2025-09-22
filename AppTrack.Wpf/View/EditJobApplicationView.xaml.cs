using AppTrack.WpfUi.ViewModel;
using System.Windows;

namespace AppTrack.WpfUi.View;

/// <summary>
/// Interaction logic for EditJobApplicationView.xaml
/// </summary>
public partial class EditJobApplicationView : Window
{
    public EditJobApplicationView(EditJobApplicationViewModel viewModel)
    {
        InitializeComponent();
        this.DataContext = viewModel;
    }
}
