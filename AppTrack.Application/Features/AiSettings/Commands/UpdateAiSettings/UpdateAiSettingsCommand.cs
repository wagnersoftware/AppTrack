using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Features.AiSettings.Dto;
using AppTrack.Shared.Validation.Interfaces;

namespace AppTrack.Application.Features.AiSettings.Commands.UpdateAiSettings;

public class UpdateAiSettingsCommand : IRequest<AiSettingsDto>, IAiSettingsValidatable, IUserScopedRequest
{
    public int SelectedChatModelId { get; set; }
    public int Id { get; set; }
    public string ApiKey { get; set; } = string.Empty;
    public string PromptTemplate { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public List<PromptParameterDto> PromptParameter { get; set; } = new List<PromptParameterDto>();

    IEnumerable<IPromptParameterValidatable> IAiSettingsValidatable.PromptParameter => PromptParameter;
}
