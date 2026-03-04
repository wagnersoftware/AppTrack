using AppTrack.BlazorUi.Components.Dialogs;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace AppTrack.BlazorUi.Components.Layout;

public partial class MainLayout
{
    [Inject] private IDialogService DialogService { get; set; } = null!;

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
        }
    };

    private async Task OpenLoginDialogAsync()
    {
        var options = new DialogOptions { BackdropClick = false };
        await DialogService.ShowAsync<LoginDialog>("", options);
    }
}
