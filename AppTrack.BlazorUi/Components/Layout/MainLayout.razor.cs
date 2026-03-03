using AppTrack.BlazorUi.Components.Dialogs;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace AppTrack.BlazorUi.Components.Layout;

public partial class MainLayout
{
    [Inject] private IDialogService DialogService { get; set; } = null!;

    private async Task OpenLoginDialogAsync()
    {
        var options = new DialogOptions { BackdropClick = false };
        await DialogService.ShowAsync<LoginDialog>("", options);
    }
}
