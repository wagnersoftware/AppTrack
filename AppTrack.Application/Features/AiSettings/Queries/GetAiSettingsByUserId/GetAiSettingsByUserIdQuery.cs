using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Features.AiSettings.Dto;

namespace AppTrack.Application.Features.AiSettings.Queries.GetAiSettingsByUserId;

public class GetAiSettingsByUserIdQuery : IRequest<AiSettingsDto>
{
    public int UserId { get; set; }
}
