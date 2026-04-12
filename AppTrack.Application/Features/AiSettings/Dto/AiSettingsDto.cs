using AppTrack.Domain.Enums;

namespace AppTrack.Application.Features.AiSettings.Dto;

public class AiSettingsDto
{
    public int SelectedChatModelId { get; set; }
    public AiResponseLanguage Language { get; set; } = AiResponseLanguage.English;
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public List<PromptParameterDto> PromptParameter { get; set; } = new List<PromptParameterDto>();
    public List<PromptDto> Prompts { get; set; } = new List<PromptDto>();
    public List<PromptDto> DefaultPrompts { get; set; } = new List<PromptDto>();
}
