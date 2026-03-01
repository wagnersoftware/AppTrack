using AppTrack.Frontend.Models.Base;
using AppTrack.Shared.Validation.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace AppTrack.Frontend.Models;

public partial class JobApplicationModel : ModelBase, IJobApplicationValidatable
{
    public string Name { get; set; } = string.Empty;

    public string Position { get; set; } = string.Empty;

    public string URL { get; set; } = string.Empty;

    public string JobDescription { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public string ContactPerson { get; set; } = string.Empty;

    [Required]
    public JobApplicationStatus Status { get; set; } = JobApplicationStatus.New;

    [DataType(DataType.Date)]
    public DateOnly StartDate { get; set; }

    [ObservableProperty]
    private string applicationText = string.Empty;

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
