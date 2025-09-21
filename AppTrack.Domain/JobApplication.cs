using AppTrack.Domain.Common;
using AppTrack.Domain.Enums;

namespace AppTrack.Domain;
public class JobApplication: BaseEntity
{
    public string ClientName { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public string URL { get; set; } = string.Empty;
    public string ApplicationText { get; set; } = string.Empty;
    public JobApplicationStatus Status { get; set; } = JobApplicationStatus.New;
    public DateTime AppliedDate { get; set; }

}