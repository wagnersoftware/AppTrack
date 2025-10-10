namespace AppTrack.Application.Features.AiSettings.Dto;

public class AiSettingsDto
{
    public int Id { get; set; }

    public string ApiKey { get; set; } = string.Empty;

    public string PromptTemplate { get; set; } = string.Empty;

    public string UserId { get; set; } = string.Empty;

    public List<PromptParameterDto> PromptParameter { get; set; } = new List<PromptParameterDto>();
}
