using AppTrack.Frontend.Models.Base;
using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace AppTrack.Frontend.Models;

public partial class JobApplicationModel : ModelBase
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Position { get; set; } = string.Empty;

    [Required]
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
    public DateOnly StartDate { get; set; }

    [ObservableProperty]
    private string applicationText = string.Empty;

    [RegularExpression(@"^\d+$", ErrorMessage = "{0} must be a number.")]
    public string DurationInMonths { get; set; } = string.Empty;

    public static Array JobApplicationStatusValues => Enum.GetValues(typeof(JobApplicationStatus));
    public enum JobApplicationStatus
    {
        New,
        WaitingForFeedback,
        Rejected
    }

    public DateTime StartDateAsDateTime
    {
        get => StartDate.ToDateTime(TimeOnly.MinValue);
        set
        {
            StartDate = DateOnly.FromDateTime(value);
        }
    }
}
