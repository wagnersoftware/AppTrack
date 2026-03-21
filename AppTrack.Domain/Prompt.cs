using AppTrack.Domain.Common;

namespace AppTrack.Domain;

public class Prompt : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string PromptTemplate { get; set; } = string.Empty;
    public int AiSettingsId { get; set; }
    public AiSettings AiSettings { get; set; } = null!;

    private Prompt()
    {
    }

    public static Prompt Create(string? name, string? promptTemplate)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(promptTemplate);

        return new Prompt { Name = name, PromptTemplate = promptTemplate };
    }
}
