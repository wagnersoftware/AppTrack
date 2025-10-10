using AppTrack.Domain.Contracts;

namespace AppTrack.Domain.Services;

public class PromptBuilder: IPromptBuilder
{
    public (string prompt, List<string> unusedKeys) BuildPrompt(IEnumerable<PromptParameter> promptParameter, string prompt)
    {
        var replacements = new Dictionary<string, string>();
        var unusedKeys = new List<string>();

        foreach (var parameter in promptParameter)
        {
            var key = $"{{{parameter.Key.Trim()}}}";
            _ = replacements.TryAdd(key, parameter.Value);
        }

        foreach (var kvp in replacements)
        {
            if (prompt.Contains(kvp.Key, StringComparison.OrdinalIgnoreCase)) // ignore upper/lower case
            {
                prompt = prompt.Replace(kvp.Key, kvp.Value, StringComparison.OrdinalIgnoreCase);
            }
            else
            {
                unusedKeys.Add(kvp.Key);
            }
        }

        return (prompt, unusedKeys);
    }
}
