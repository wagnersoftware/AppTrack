using System.ComponentModel.DataAnnotations;

namespace AppTrack.Frontend.Models;

public class JobApplicationModel
{
    public int Id { get; set; }
    [Required]
    [Display(Name = "Company name")]
    public string Client { get; set; } = string.Empty;
    [Required]
    public string Position { get; set; } = string.Empty;
    public JobApplicationStatus Status { get; set; }
    public DateTimeOffset? AppliedDate { get; set; }
    public DateTimeOffset? FollowUpDate { get; set; }
    public string Notes { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string ApplicationText { get; set; } = string.Empty;

    public enum JobApplicationStatus
    {
        New,
        WaitingForFeedback,
        Rejected
    }
}
