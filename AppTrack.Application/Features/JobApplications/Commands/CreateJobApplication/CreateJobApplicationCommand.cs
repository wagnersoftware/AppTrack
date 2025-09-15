using AppTrack.Domain.Enums;
using MediatR;

namespace AppTrack.Application.Features.JobApplications.Commands.CreateJobApplication;
public class CreateJobApplicationCommand: IRequest<int>
{
    public string Client { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public JobApplicationStatus Status { get; set; } = JobApplicationStatus.New;
    public DateTime? AppliedDate { get; set; }
    public DateTime? FollowUpDate { get; set; }
    public string Notes { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string ApplicationText { get; set; } = string.Empty;
}

