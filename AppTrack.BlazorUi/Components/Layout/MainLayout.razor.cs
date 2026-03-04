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

    private static readonly DialogOptions _dialogOptions = new() { BackdropClick = true };

    private async Task OpenLoginDialogAsync() =>
        await DialogService.ShowAsync<LoginDialog>("", _dialogOptions);

    private async Task OpenRegisterDialogAsync() =>
        await DialogService.ShowAsync<RegisterDialog>("", _dialogOptions);
}
