using AppTrack.Application.Contracts.Mediator;
using AppTrack.Domain;
using AppTrack.Domain.Enums;

namespace AppTrack.Application.Features.JobApplications.Commands.CreateJobApplication;
public class CreateJobApplicationCommand : IRequest<JobApplication>
{
    public DateTime CreationDate { get; set; }
    public DateTime ModifiedDate { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public string URL { get; set; } = string.Empty;
    public string ApplicationText { get; set; } = string.Empty;
    public JobApplicationStatus Status { get; set; } = JobApplicationStatus.New;
    public DateTime AppliedDate { get; set; }
}

