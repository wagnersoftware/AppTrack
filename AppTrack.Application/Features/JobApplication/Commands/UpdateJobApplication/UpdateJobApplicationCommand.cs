using AppTrack.Domain.Enums;
using MediatR;

namespace AppTrack.Application.Features.JobApplication.Commands.UpdateJobApplication;

public  class UpdateJobApplicationCommand: IRequest<Unit>
{
    public int Id { get; set; }
    public string Client { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public JobApplicationStatus Status { get; set; }
    public DateTime? AppliedDate { get; set; }
    public DateTime? FollowUpDate { get; set; }
    public string Notes { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string ApplicationText { get; set; } = string.Empty;
    public DateTime DateCreated { get; set; }
}
