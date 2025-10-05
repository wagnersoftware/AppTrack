using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Features.JobApplications.Dto;

namespace AppTrack.Application.Features.JobApplications.Commands.GenerateApplicationText;

public class GenerateApplicationTextCommand : IRequest<GeneratedApplicationTextDto>
{
    public int ApplicationId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string URL { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
}
