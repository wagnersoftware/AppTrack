using AppTrack.Application.Contracts.ApplicationTextGenerator;
using AppTrack.Infrastructure.ApplicationTextGeneration.OpAiModels;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace AppTrack.Infrastructure.ApplicationTextGeneration;

public class OpenAiApplicationTextGenerator : IApplicationTextGenerator
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _openAiUrl;

    public OpenAiApplicationTextGenerator(HttpClient httpClient, IOptions<OpenAiOptions> openAiOptions)
    {
        _httpClient = httpClient;
        _httpClient.Timeout = TimeSpan.FromSeconds(openAiOptions.Value.TimeoutInSeconds);
        _openAiUrl = openAiOptions.Value.ApiUrl ?? throw new InvalidOperationException("OpenAI API URL is not configured.");
        _apiKey = openAiOptions.Value.ApiKey ?? throw new InvalidOperationException("OpenAI API key is not configured.");
    }

    public async Task<string> GenerateApplicationTextAsync(string prompt, string modelName, CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, _openAiUrl);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

        request.Content = JsonContent.Create(new
        {
            model = modelName,
            messages = new[]
            {
                new { role = "system", content = "You are an assistant that writes professional job applications." },
                new { role = "user", content = prompt }
            },
            max_tokens = 400
        });

        var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<ChatCompletionResponse>(cancellationToken: cancellationToken);

        return result?.Choices?.FirstOrDefault()?.Message?.Content ?? string.Empty;
    }
}
