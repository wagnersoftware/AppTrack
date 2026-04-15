using AppTrack.Domain.Common;

namespace AppTrack.Domain;

public class BuiltInPrompt : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string PromptTemplate { get; set; } = string.Empty;

    private BuiltInPrompt() { }

    public static BuiltInPrompt Create(string? name, string? promptTemplate)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(promptTemplate);

        // Seeder code runs outside the FluentValidation pipeline, so guards are the
        // only domain-level enforcement for these invariants.
        if (!name.StartsWith(BuiltInParameterKeys.Prefix, StringComparison.Ordinal))
            throw new ArgumentException($"Default prompt names must start with '{BuiltInParameterKeys.Prefix}'.", nameof(name));

        if (name.Contains(' '))
            throw new ArgumentException("Default prompt names must not contain spaces.", nameof(name));

        return new BuiltInPrompt
        {
            Name = name,
            PromptTemplate = promptTemplate
        };
    }
}
