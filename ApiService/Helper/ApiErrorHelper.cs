using System.Text.Json;

namespace AppTrack.Frontend.ApiService.Helper;

public static class ApiErrorHelper
{
    public static string ExtractErrors(string json)
    {
        using var doc = JsonDocument.Parse(json);

        string? title = doc.RootElement.TryGetProperty("title", out var titleProp)
            ? titleProp.GetString()
            : "Unknown title";

        string status = doc.RootElement.TryGetProperty("status", out var statusProp)
            ? statusProp.GetRawText()
            : "Unknown status";

        var messages = new List<string>
    {
        $"Status: {status}",
        $"Title: {title}"
    };

        if (doc.RootElement.TryGetProperty("errors", out var errors) && errors.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in errors.EnumerateObject())
            {
                foreach (var message in property.Value.EnumerateArray())
                {
                    messages.Add($"{property.Name}: {message.GetString()}");
                }
            }
        }
        else if (errors.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in errors.EnumerateArray())
            {
                messages.Add(item.GetString());             
            }
        }

        return string.Join(Environment.NewLine, messages);
    }
}