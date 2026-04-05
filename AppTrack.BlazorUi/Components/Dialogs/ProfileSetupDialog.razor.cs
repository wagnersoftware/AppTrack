using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace AppTrack.BlazorUi.Components.Dialogs;

public partial class ProfileSetupDialog
{
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = null!;

    private void Skip() => MudDialog.Cancel();
    private void Save() => MudDialog.Close();
}
