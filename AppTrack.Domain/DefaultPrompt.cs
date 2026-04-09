using AppTrack.Domain.Common;

namespace AppTrack.Domain;

public class DefaultPrompt : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string PromptTemplate { get; set; } = string.Empty;

    private DefaultPrompt() { }

    public static DefaultPrompt Create(string? name, string? promptTemplate)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(promptTemplate);

        // Seeder code runs outside the FluentValidation pipeline, so guards are the
        // only domain-level enforcement for these invariants.
        if (!name.StartsWith("Default_", StringComparison.Ordinal))
            throw new ArgumentException("Default prompt names must start with 'Default_'.", nameof(name));

        if (name.Contains(' '))
            throw new ArgumentException("Default prompt names must not contain spaces.", nameof(name));

        return new DefaultPrompt
        {
            Name = name,
            PromptTemplate = promptTemplate
        };
    }
}
