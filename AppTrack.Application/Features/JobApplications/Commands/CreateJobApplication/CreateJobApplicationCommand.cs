using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Features.JobApplications.Dto;
using AppTrack.Domain.Enums;

namespace AppTrack.Application.Features.JobApplications.Commands.CreateJobApplication;
public class CreateJobApplicationCommand : IRequest<JobApplicationDto>
{
    public DateTime CreationDate { get; set; }
    public DateTime ModifiedDate { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public string URL { get; set; } = string.Empty;
    public string ApplicationText { get; set; } = string.Empty;
    public JobApplicationStatus Status { get; set; } = JobApplicationStatus.New;
    public string UserId { get; set; } = string.Empty;
    public string JobDescription { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string ContactPerson { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public string DurationInMonths { get; set; } = string.Empty;
}

