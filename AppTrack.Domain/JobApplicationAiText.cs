namespace AppTrack.Domain;

public class JobApplicationAiText
{
    public int Id { get; set; }
    public int JobApplicationId { get; set; }
    public string PromptKey { get; set; } = string.Empty;
    public string GeneratedText { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }

    public JobApplication JobApplication { get; set; } = null!;
}
