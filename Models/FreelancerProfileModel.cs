namespace AppTrack.Frontend.Models;

public class FreelancerProfileModel
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public decimal? DailyRate { get; set; }
    public decimal? HourlyRate { get; set; }
    public DateOnly? AvailableFrom { get; set; }
    public RemotePreference? WorkMode { get; set; }
    public string? Skills { get; set; }
}
