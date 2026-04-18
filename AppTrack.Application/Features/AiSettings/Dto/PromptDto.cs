using AppTrack.Shared.Validation.Interfaces;

namespace AppTrack.Application.Features.AiSettings.Dto;

public class PromptDto : IPromptValidatable
{
    public int Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string PromptTemplate { get; set; } = string.Empty;
}
