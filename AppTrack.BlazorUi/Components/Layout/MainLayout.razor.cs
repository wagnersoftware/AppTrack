using AppTrack.BlazorUi.Components.Dialogs;
using AppTrack.Frontend.ApiService.ApiAuthenticationProvider;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;

namespace AppTrack.BlazorUi.Components.Layout;

public partial class MainLayout
{
    [Inject] private IDialogService DialogService { get; set; } = null!;
    [Inject] private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = null!;

    internal static readonly MudTheme AzureTheme = new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = "#0078D4",
            PrimaryDarken = "#005A9E",
            PrimaryLighten = "#50A0D8",
            PrimaryContrastText = "#FFFFFF",
            AppbarBackground = "#0078D4",
            AppbarText = "#FFFFFF",
            Background = "#F0F3F7",
            BackgroundGray = "#E4E8EE",
        }
    };

    private static readonly DialogOptions _dialogOptions = new() { BackdropClick = true };

    private static readonly DialogOptions _aiSettingsDialogOptions = new()
    {
        BackdropClick = false,
        MaxWidth = MaxWidth.Medium,
        FullWidth = true,
    };

    private bool _drawerOpen = false;

    private void ToggleDrawer() => _drawerOpen = !_drawerOpen;

    private async Task OpenLoginDialogAsync() =>
        await DialogService.ShowAsync<LoginDialog>("", _dialogOptions);

    private async Task OpenRegisterDialogAsync() =>
        await DialogService.ShowAsync<RegisterDialog>("", _dialogOptions);

    private async Task OpenAiSettingsDialogAsync()
    {
        _drawerOpen = false;
        await DialogService.ShowAsync<AiSettingsDialog>("", _aiSettingsDialogOptions);
    }

    private async Task LogoutAsync()
    {
        var provider = (ApiAuthenticationStateProvider)AuthenticationStateProvider;
        await provider.LoggedOut();
    }
}
