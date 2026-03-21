using AppTrack.Shared.Validation.Interfaces;

namespace AppTrack.Application.Features.AiSettings.Dto;

public class PromptDto : IPromptValidatable
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PromptTemplate { get; set; } = string.Empty;
}
