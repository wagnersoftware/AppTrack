namespace AppTrack.Application.Features.ApplicationText.Dto
{
    public class RenderedPromptDto
    {
        public string Prompt { get; set; } = string.Empty;

        public List<string> UnusedKeys { get; set; } = new List<string>();
    }
}
