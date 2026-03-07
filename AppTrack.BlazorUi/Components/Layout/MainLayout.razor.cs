using AppTrack.BlazorUi.Components.Dialogs;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using MudBlazor;

namespace AppTrack.BlazorUi.Components.Layout;

public partial class MainLayout
{
    [Inject] private IDialogService DialogService { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;

    internal static readonly MudTheme AzureTheme = new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = "#0078D4",
            PrimaryDarken = "#005A9E",
            PrimaryLighten = "#50A0D8",
            PrimaryContrastText = "#FFFFFF",
            Secondary = "#637381",
            SecondaryContrastText = "#FFFFFF",
            AppbarBackground = "#0078D4",
            AppbarText = "#FFFFFF",
            Background = "#F0F3F7",
            BackgroundGray = "#E4E8EE",
        }
    };

    private static readonly DialogOptions _aiSettingsDialogOptions = new()
    {
        BackdropClick = false,
        MaxWidth = MaxWidth.Medium,
        FullWidth = true,
    };

    private bool _drawerOpen = false;

    private void ToggleDrawer() => _drawerOpen = !_drawerOpen;

    private void LoginAsync() => Navigation.NavigateToLogin("authentication/login");

    private async Task OpenAiSettingsDialogAsync()
    {
        _drawerOpen = false;
        await DialogService.ShowAsync<AiSettingsDialog>("", _aiSettingsDialogOptions);
    }

    private void LogoutAsync() => Navigation.NavigateToLogout("authentication/logout");
}
