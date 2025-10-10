using AppTrack.Domain.Common;
using System.Runtime.InteropServices;

namespace AppTrack.Domain;

public class PromptParameter : BaseEntity
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public int AISettingsId { get; set; }
    public AiSettings AISettings { get; set; } = null!;

    private PromptParameter()
    {
            
    }

    public static PromptParameter Create(string? key, string? value)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(value);

        return new PromptParameter() { Key = key, Value = value };
    }
}
