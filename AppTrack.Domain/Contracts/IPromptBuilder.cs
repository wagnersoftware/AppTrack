namespace AppTrack.Domain.Contracts;

public interface IPromptBuilder
{
    (string prompt, List<string> unusedKeys) BuildPrompt(IEnumerable<PromptParameter> promptParameter, string prompt);
}
