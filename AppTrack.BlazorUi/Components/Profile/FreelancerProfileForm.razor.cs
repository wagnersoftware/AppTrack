using AppTrack.Frontend.Models;

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

    private void OnRateKindChanged(RateKind? newKind)
    {
        if (_model.SelectedRateType == newKind) return;
        _model.SelectedRateType = newKind;
        if (newKind == RateKind.Hourly) _model.DailyRate = null;
        else if (newKind == RateKind.Daily) _model.HourlyRate = null;
        else { _model.HourlyRate = null; _model.DailyRate = null; }
    }

    private void OnRateValueChanged(decimal? value)
    {
        if (_model.SelectedRateType == RateKind.Hourly) _model.HourlyRate = value;
        else if (_model.SelectedRateType == RateKind.Daily) _model.DailyRate = value;
    }
}
