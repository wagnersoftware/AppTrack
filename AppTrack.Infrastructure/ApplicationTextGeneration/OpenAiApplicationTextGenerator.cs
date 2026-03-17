using AppTrack.Application.Contracts.ApplicationTextGenerator;
using AppTrack.Application.Exceptions;
using AppTrack.Infrastructure.ApplicationTextGeneration.OpAiModels;
using Microsoft.Extensions.Options;
using System.Net;
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

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.SendAsync(request, cancellationToken);
        }
        catch (TaskCanceledException ex)
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                throw new ExternalServiceException("The request to OpenAI timed out.", HttpStatusCode.GatewayTimeout, ex);
            }

            throw;
        }

        if (!response.IsSuccessStatusCode)
        {
            throw response.StatusCode switch
            {
                HttpStatusCode.Unauthorized =>
                    new ExternalServiceException("OpenAI API key is invalid or expired.", HttpStatusCode.Unauthorized),
                HttpStatusCode.TooManyRequests =>
                    new ExternalServiceException("OpenAI rate limit exceeded. Please try again later.", HttpStatusCode.TooManyRequests),
                HttpStatusCode.ServiceUnavailable or HttpStatusCode.GatewayTimeout =>
                    new ExternalServiceException("OpenAI service is currently unavailable.", response.StatusCode),
                _ =>
                    new ExternalServiceException($"OpenAI returned an unexpected error: {(int)response.StatusCode}", response.StatusCode)
            };
        }

        var result = await response.Content.ReadFromJsonAsync<ChatCompletionResponse>(cancellationToken: cancellationToken);
        var content = result?.Choices?.FirstOrDefault()?.Message?.Content;

        if (string.IsNullOrWhiteSpace(content))
        {
            throw new ExternalServiceException("OpenAI returned an empty response.");
        }

        return content;
    }
}
