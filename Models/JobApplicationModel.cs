using System.ComponentModel.DataAnnotations;

namespace AppTrack.Frontend.Models;

public class JobApplicationModel : ModelBase
{
    [Required]
    [MaxLength(50, ErrorMessage = "Maximum length for name is 50 characters")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(30, ErrorMessage = "Maximum length for name is 30 characters")]
    public string Position { get; set; } = string.Empty;

    [Required]
    [MaxLength(500, ErrorMessage = "Maximum length for name is 500 characters")]
    [Url]
    public string URL { get; set; } = string.Empty;

    public string ApplicationText { get; set; } = string.Empty;

    [Required]
    public JobApplicationStatus Status { get; set; } = JobApplicationStatus.New;
    public DateTime AppliedDate { get; set; }
    public static Array JobApplicationStatusValues => Enum.GetValues(typeof(JobApplicationStatus));
    public enum JobApplicationStatus
    {
        New,
        WaitingForFeedback,
        Rejected
    }
}
