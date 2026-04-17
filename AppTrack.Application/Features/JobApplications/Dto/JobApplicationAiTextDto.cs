namespace AppTrack.Application.Features.JobApplications.Dto;

public class JobApplicationAiTextDto
{
    public string PromptKey { get; set; } = string.Empty;
    public string GeneratedText { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
}
