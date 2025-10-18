using AppTrack.Application.Contracts.ApplicationTextGenerator;
using AppTrack.Infrastructure.ApplicationTextGeneration.OpAiModels;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace AppTrack.Infrastructure.ApplicationTextGeneration;

public class OpenAiApplicationTextGenerator : IApplicationTextGenerator
{
    private readonly HttpClient _httpClient;
    private string? _apiKey;
    private readonly string _openAiUrl;

    public OpenAiApplicationTextGenerator(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _openAiUrl = configuration["OpenAi:ApiUrl"] ?? throw new InvalidOperationException("OpenAI API URL is not configured.");
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

        var response = await _httpClient.SendAsync(request, cancellationToken);// Fehlerbehandlung 429 -> Too Many Requests,
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<ChatCompletionResponse>(cancellationToken: cancellationToken);

        return result?.Choices?.FirstOrDefault()?.Message?.Content ?? string.Empty;
    }
}