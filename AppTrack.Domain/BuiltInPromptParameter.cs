using AppTrack.Domain.Common;

namespace AppTrack.Domain;

public class BuiltInPromptParameter : BaseEntity
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public int AiSettingsId { get; set; }
    public AiSettings AiSettings { get; set; } = null!;

    private BuiltInPromptParameter()
    {
    }

    public static BuiltInPromptParameter Create(string key, string value) => new()
    {
        Key = key,
        Value = value
    };
}
