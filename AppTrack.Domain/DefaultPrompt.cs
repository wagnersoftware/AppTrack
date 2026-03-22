using AppTrack.Domain.Common;

namespace AppTrack.Domain;

public class DefaultPrompt : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string PromptTemplate { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty; // ISO 639-1, e.g. "de", "en"

    private DefaultPrompt() { }

    public static DefaultPrompt Create(string? name, string? promptTemplate, string? language)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(promptTemplate);
        ArgumentNullException.ThrowIfNull(language);

        return new DefaultPrompt
        {
            Name = name,
            PromptTemplate = promptTemplate,
            Language = language
        };
    }
}
