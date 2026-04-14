using AppTrack.Frontend.Models.Base;
using AppTrack.Shared.Validation.Interfaces;

namespace AppTrack.Frontend.Models;

public class FreelancerProfileModel : ModelBase, IFreelancerProfileValidatable
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public RateKind? SelectedRateType { get; set; }
    public decimal? HourlyRate { get; set; }
    public decimal? DailyRate { get; set; }
    public DateOnly? AvailableFrom { get; set; }
    public RemotePreference? WorkMode { get; set; }
    public string? Skills { get; set; }
    public string? CvFileName { get; set; }
    public DateTime? CvUploadDate { get; set; }
}
