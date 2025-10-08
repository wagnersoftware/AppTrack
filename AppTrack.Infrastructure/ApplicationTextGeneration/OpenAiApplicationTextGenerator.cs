using AppTrack.Application.Contracts.ApplicationTextGenerator;
using AppTrack.Infrastructure.ApplicationTextGeneration.OpAiModels;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace AppTrack.Infrastructure.ApplicationTextGeneration;

public class OpenAiApplicationTextGenerator : IApplicationTextGenerator
{
    private readonly HttpClient _httpClient;
    private string? _apiKey;
    private readonly string _openAiUrl = "https://api.openai.com/v1/chat/completions";

    public OpenAiApplicationTextGenerator(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public void SetApiKey(string apiKey) => _apiKey = apiKey;

    public async Task<string> GenerateApplicationTextAsync(string prompt, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            throw new InvalidOperationException("API key not set.");
        }
        var request = new HttpRequestMessage(HttpMethod.Post, _openAiUrl);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

        request.Content = JsonContent.Create(new
        {
            model = "gpt-4o-mini",
            messages = new[]
            {
                new { role = "system", content = "You are an assistant that writes professional job applications." },
                new { role = "user", content = prompt }
            },
            max_tokens = 400 // limits the response message
        });

        var response = await _httpClient.SendAsync(request, cancellationToken);// todo Hehlerbehandlung 429 -> Too Many Requests,
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<ChatCompletionResponse>(cancellationToken: cancellationToken);

        return result?.Choices?.FirstOrDefault()?.Message?.Content ?? string.Empty;
    }
}