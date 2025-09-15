using AppTrack.Domain.Enums;

namespace AppTrack.Application.Features.JobApplications.Queries.GetAllJobApplications;
public class JobApplicationDto
{
    public string Client { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public JobApplicationStatus Status { get; set; }
    public DateTime AppliedDate { get; set; }
    public DateTime? FollowUpDate { get; set; }
    public string Notes { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string ApplicationText { get; set; } = string.Empty;
    public int Id { get; set; }
    public DateTime DateCreated { get; set; }
    public DateTime DateModified { get; set; }
}

