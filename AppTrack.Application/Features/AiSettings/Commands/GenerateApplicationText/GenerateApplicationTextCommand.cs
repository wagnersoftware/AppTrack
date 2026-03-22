using System.Text.Json.Serialization;
using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Features.JobApplications.Dto;

namespace AppTrack.Application.Features.AiSettings.Commands.GenerateApplicationText;

public class GenerateApplicationTextCommand : IRequest<GeneratedApplicationTextDto>, IUserScopedRequest
{
    public int JobApplicationId { get; set; }
    public string Prompt { get; set; } = string.Empty;
    [JsonIgnore]
    public string UserId { get; set; } = string.Empty;
}
