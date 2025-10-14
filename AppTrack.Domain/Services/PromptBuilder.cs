using AppTrack.Domain.Contracts;
using System.Text.RegularExpressions;

namespace AppTrack.Domain.Services;

public class PromptBuilder : IPromptBuilder
{
    public (string prompt, List<string> unusedKeys) BuildPrompt(IEnumerable<PromptParameter> promptParameter, string prompt)
    {
        var replacements = new Dictionary<string, string>();

        foreach (var parameter in promptParameter)
        {
            var key = $"{{{parameter.Key.Trim()}}}";
            _ = replacements.TryAdd(key, parameter.Value);
        }

        foreach (var kvp in from kvp in replacements
                            where prompt.Contains(kvp.Key, StringComparison.OrdinalIgnoreCase)// ignore upper/lower case
                            select kvp)
        {
            prompt = prompt.Replace(kvp.Key, kvp.Value, StringComparison.OrdinalIgnoreCase);
        }

        var unusedKeys = GetUnusedKeys(prompt);

        return (prompt, unusedKeys);
    }

    private static List<string> GetUnusedKeys(string prompt)
    {
        return Regex.Matches(prompt, @"\{\{(.*?)\}\}")
            .Cast<Match>()
            .Select(m => m.Groups[1].Value)
            .ToList() ?? new List<string>();
    }
}
