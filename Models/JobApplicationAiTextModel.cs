namespace AppTrack.Frontend.Models;

public class JobApplicationAiTextModel
{
    public string PromptKey { get; set; } = string.Empty;
    public string GeneratedText { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
}
