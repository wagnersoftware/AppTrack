using AppTrack.Domain.Common;
using AppTrack.Domain.Enums;

namespace AppTrack.Domain;

public class JobApplication : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public string URL { get; set; } = string.Empty;
    public string ApplicationText { get; set; } = string.Empty;
    public JobApplicationStatus Status { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string JobDescription { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string ContactPerson { get; set; } = string.Empty;
    public string DurationInMonths { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }

}