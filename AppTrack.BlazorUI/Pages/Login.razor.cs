using AppTrack.Frontend.ApiService.Contracts;
using AppTrack.Frontend.Models;
using Microsoft.AspNetCore.Components;

namespace AppTrack.BlazorUI.Pages
{
    public partial class Login
    {
        public LoginModel Model { get; set; }

        [Inject]
        public NavigationManager NavigationManager { get; set; }
        public string Message { get; set; }

        [Inject]
        private IAuthenticationService AuthenticationService { get; set; }

        public Login()
        {

        }

        protected override void OnInitialized()
        {
            Model = new LoginModel();
        }

        protected async Task HandleLogin()
        {
            if (await AuthenticationService.AuthenticateAsync(Model))
            {
                NavigationManager.NavigateTo("/");
            }
            Message = "Username/password combination unknown";
        }
    }
}
