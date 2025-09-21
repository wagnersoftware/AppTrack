using AppTrack.Domain;
using AppTrack.Domain.Enums;
using MediatR;

namespace AppTrack.Application.Features.JobApplications.Commands.CreateJobApplication;
public class CreateJobApplicationCommand: IRequest<JobApplication>
{
    public int Id { get; set; }
    public DateTime CreationDate { get; set; }
    public DateTime ModifiedDate { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public string URL { get; set; } = string.Empty;
    public string ApplicationText { get; set; } = string.Empty;
    public JobApplicationStatus Status { get; set; } = JobApplicationStatus.New;
    public DateTime AppliedDate { get; set; }
}

