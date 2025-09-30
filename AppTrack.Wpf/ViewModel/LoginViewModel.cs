using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AppTrack.WpfUi.ViewModel;

public partial class LoginViewModel : ObservableObject
{
    [ObservableProperty]
    private string username;

    [ObservableProperty]
    private string password;

    public Action<bool?>? CloseAction { get; set; }

    [RelayCommand]
    private async Task Login()
    {

    }

    [RelayCommand]
    private async Task Register()
    {

    }
}


