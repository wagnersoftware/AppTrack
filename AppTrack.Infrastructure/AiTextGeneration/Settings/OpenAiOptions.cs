using System.ComponentModel.DataAnnotations;

namespace AppTrack.Infrastructure.AiTextGeneration;

public class OpenAiOptions
{
    [Required]
    [Url]
    public string ApiUrl { get; set; } = string.Empty;

    [Required]
    public int TimeoutInSeconds { get; set; }

    [Required]
    public string ApiKey { get; set; } = string.Empty;

    [Required]
    [Range(1, 32000)]
    public int MaxTokens { get; set; } = 1000;
}
