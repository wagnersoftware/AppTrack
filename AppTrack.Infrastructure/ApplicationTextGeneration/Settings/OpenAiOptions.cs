using System.ComponentModel.DataAnnotations;

namespace AppTrack.Infrastructure.ApplicationTextGeneration;

/// <summary>
/// Represents configuration options for connecting to an OpenAI API endpoint.
/// </summary>
/// <remarks>This class is typically used to provide settings required for API integration, such as the endpoint
/// URL. All properties should be set before initiating API requests.</remarks>
public class OpenAiOptions
{
    [Required]
    [Url]
    public string ApiUrl { get; set; } = string.Empty;
}

