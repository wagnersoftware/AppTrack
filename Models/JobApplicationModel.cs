using AppTrack.Frontend.Models.Base;
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

    [Required]
    public string JobDescription { get; set; } = string.Empty;

    [Required]
    public string Location { get; set; } = string.Empty;

    [Required]
    public string ContactPerson { get; set; } = string.Empty;

    [Required]
    public JobApplicationStatus Status { get; set; } = JobApplicationStatus.New;

    [Required]
    [DataType(DataType.Date)]
    public DateTime StartDate { get; set; }

    public string ApplicationText { get; set; } = string.Empty;

    [DataType(DataType.Duration)]
    [Range(1, 120, ErrorMessage = "Duration must be between 1 and 120 months if provided")]
    public int? DurationInMonths { get; set; }

    public static Array JobApplicationStatusValues => Enum.GetValues(typeof(JobApplicationStatus));
    public enum JobApplicationStatus
    {
        New,
        WaitingForFeedback,
        Rejected
    }
}
