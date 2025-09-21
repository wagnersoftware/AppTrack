using System.ComponentModel.DataAnnotations;

namespace AppTrack.Frontend.Models;

public class JobApplicationModel : ModelBase
{
    [Required]
    public string ClientName { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public string URL { get; set; } = string.Empty;
    public string ApplicationText { get; set; } = string.Empty;
    [Required]
    public JobApplicationStatus Status { get; set; } = JobApplicationStatus.New;
    public DateTime AppliedDate { get; set; }

    public enum JobApplicationStatus
    {
        New,
        WaitingForFeedback,
        Rejected
    }
}
