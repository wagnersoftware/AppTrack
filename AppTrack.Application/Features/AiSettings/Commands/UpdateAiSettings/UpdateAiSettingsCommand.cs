using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Shared;

namespace AppTrack.Application.Features.AiSettings.Commands.UpdateAiSettings;

public class UpdateAiSettingsCommand : IRequest<Unit>
{
    public int Id { get; set; }
    public string ApiKey { get; set; } = string.Empty;
    public string MySkills { get; set; } = string.Empty;
    public string Prompt { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
}
