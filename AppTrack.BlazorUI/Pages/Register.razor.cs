using AppTrack.Frontend.ApiService.Contracts;
using AppTrack.Frontend.Models;
using Microsoft.AspNetCore.Components;

namespace AppTrack.BlazorUI.Pages;

public partial class Register
{

    public RegistrationModel Model { get; set; }

    [Inject]
    public NavigationManager NavigationManager { get; set; }

    public string Message { get; set; }

    [Inject]
    private IAuthenticationService AuthenticationService { get; set; }

    protected override void OnInitialized()
    {
        Model = new RegistrationModel();
    }

    protected async Task HandleRegister()
    {
        var apiResponse = await AuthenticationService.RegisterAsync(Model);

        if (apiResponse.Success)
        {
            NavigationManager.NavigateTo("/");
        }
        Message = "Something went wrong, please try again.";
    }
}
