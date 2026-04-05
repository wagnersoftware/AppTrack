using Microsoft.AspNetCore.Components;

namespace AppTrack.BlazorUi.Components.Pages;

public partial class ProfileSetup
{
    [Inject] private NavigationManager Navigation { get; set; } = null!;

    private void Save() => Navigation.NavigateTo("/");
}
