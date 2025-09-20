using AppTrack.Frontend.ApiService.Contracts;
using AppTrack.Frontend.Models;
using Microsoft.AspNetCore.Components;

namespace AppTrack.BlazorUI.Pages;

public partial class Register
{

    public RegisterModel Model { get; set; }

    [Inject]
    public NavigationManager NavigationManager { get; set; }

    public string Message { get; set; }

    [Inject]
    private IAuthenticationService AuthenticationService { get; set; }

    protected override void OnInitialized()
    {
        Model = new RegisterModel();
    }

    protected async Task HandleRegister()
    {
        var result = await AuthenticationService.RegisterAsync(Model);

        if (result)
        {
            NavigationManager.NavigateTo("/");
        }
        Message = "Something went wrong, please try again.";
    }
}
