using AppTrack.Frontend.ApiService.Base;
using System.Text.Json;

namespace AppTrack.Frontend.ApiService.Helper;

public static class ApiErrorHelper
{
    public static string ExtractErrors(string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            return string.Empty;
        }

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
        $"Message: {title}"
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
                var errorMessage = item.GetString();
                if (errorMessage != null)
                {
                    messages.Add(errorMessage);
                }
            }
        }

        return string.Join(Environment.NewLine, messages);
    }

    public static string ExtractErrors(CustomProblemDetails problem)
    {
        if (problem == null)
            return string.Empty;

        var messages = new List<string>
        {
            $"Status: {problem.Status}",
            $"Message: {problem.Title}"
        };

        if (problem.Errors != null)
        {
            foreach (var kvp in problem.Errors)
            {
                foreach (var msg in kvp.Value)
                {
                    messages.Add($"{kvp.Key}: {msg}");
                }
            }
        }

        return string.Join(Environment.NewLine, messages);
    }
}