using AppTrack.Domain.Enums;

namespace AppTrack.Application.Features.JobApplications.Dto;
public class JobApplicationDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public string URL { get; set; } = string.Empty;
    public string ApplicationText { get; set; } = string.Empty;
    public DateTime CreationDate { get; set; }
    public DateTime ModifiedDate { get; set; }
    public JobApplicationStatus Status { get; set; } = JobApplicationStatus.New;
    public DateTime AppliedDate { get; set; }

}

