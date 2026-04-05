using AppTrack.Frontend.Models;
using Microsoft.AspNetCore.Components;

namespace AppTrack.BlazorUi.Components.Profile;

public partial class FreelancerProfileForm
{
    private readonly FreelancerProfileModel _model = new();
    private string _selectedType = "Freelancer";
    private DateTime? _availableFrom;

    private void SelectFreelancer() => _selectedType = "Freelancer";

    private void OnAvailableFromChanged(DateTime? date)
    {
        _availableFrom = date;
        _model.AvailableFrom = date.HasValue ? DateOnly.FromDateTime(date.Value) : null;
    }

    private string GetCardStyle(string type) =>
        _selectedType == type
            ? "cursor: pointer; border: 2px solid var(--mud-palette-primary);"
            : "cursor: pointer;";
}
