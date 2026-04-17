using System.Text.Json.Serialization;
using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Shared;

namespace AppTrack.Application.Features.AiSettings.Commands.DeleteAiText;

public class DeleteAiTextCommand : IRequest<Unit>, IUserScopedRequest
{
    public int Id { get; set; }

    [JsonIgnore]
    public string UserId { get; set; } = string.Empty;
}
