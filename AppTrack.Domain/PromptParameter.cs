namespace AppTrack.Domain;

public class PromptParameter
{
    public int Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;

    public int AISettingsId { get; set; }
    public AiSettings AISettings { get; set; } = null!;
}
