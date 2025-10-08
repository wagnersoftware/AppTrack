using AppTrack.WpfUi.ViewModel;
using System.Windows;

namespace AppTrack.WpfUi.View;

/// <summary>
/// Interaction logic for TextView.xaml
/// </summary>
public partial class TextView : Window
{
    public TextView(TextViewModel ViewModel)
    {
        InitializeComponent();
        this.DataContext = ViewModel;
    }
}
