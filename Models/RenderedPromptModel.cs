namespace AppTrack.Frontend.Models;

public class RenderedPromptModel : TextModel
{
    public List<string> UnusedKeys { get; set; } = new List<string>();
}
