using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using MudBlazor;

namespace AppTrack.BlazorUi.Components.Layout;

public partial class MainLayout : IDisposable
{
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

    private bool _drawerOpen = false;

    protected override void OnInitialized()
    {
        Navigation.LocationChanged += OnLocationChanged;
    }

    private void ToggleDrawer() => _drawerOpen = !_drawerOpen;

    private void Login() => Navigation.NavigateToLogin("authentication/login");

    private void Logout() => Navigation.NavigateToLogout("authentication/logout");

    private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        _drawerOpen = false;
        InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
            Navigation.LocationChanged -= OnLocationChanged;
    }
}
