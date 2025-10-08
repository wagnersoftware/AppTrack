namespace AppTrack.Frontend.Models;

/// <summary>
/// Model for the Text Window that displays long texts. It is not persisted in the DB, that why no base class is needed.
/// </summary>
public class TextModel
{
    public string Text { get; set; } = string.Empty;

    public string UserMessage { get; set; } = string.Empty;

    public string WindowTitle { get; set; } = string.Empty;
}
