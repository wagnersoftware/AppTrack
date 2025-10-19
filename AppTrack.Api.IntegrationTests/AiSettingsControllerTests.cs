using AppTrack.Api.IntegrationTests.Seeddata.AiSetttings;
using AppTrack.Api.IntegrationTests.Seeddata.User;
using AppTrack.Api.Models;
using AppTrack.Application.Features.AiSettings.Commands.UpdateAiSettings;
using AppTrack.Application.Features.AiSettings.Dto;
using Shouldly;
using System.Net;
using System.Net.Http.Json;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace AppTrack.Api.IntegrationTests;

public class AiSettingsControllerTests : IClassFixture<FakeAuthWebApplicationFactory>
{
    private readonly FakeAuthWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public AiSettingsControllerTests(FakeAuthWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateAuthenticatedClient();
    }

    [Fact]
    public async Task GetAiSettings_ShouldReturnAiSettings_WhenUserExists()
    {
        // Arrange
        var validUserId = ApplicationUserSeed.User1Id;
        // Act
        var response = await _client.GetAsync($"/api/ai-settings?UserId={validUserId}");
        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var aiSettings = await response.Content.ReadFromJsonAsync<AiSettingsDto>();
        aiSettings.ShouldNotBeNull();
        aiSettings.UserId.ShouldBe(validUserId);
    }

    [Fact]
    public async Task GetAiSettings_ShouldReturn404_WhenUserNotFound()
    {
        // Arrange
        var invalidUserId = "999";
        // Act
        var response = await _client.GetAsync($"/api/ai-settings?UserId={invalidUserId}");
        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        var problem = await response.Content.ReadFromJsonAsync<CustomProblemDetails>();
        problem?.Title.ShouldBe($"user {invalidUserId} not found");
    }

    [Fact]
    public async Task GetAiSettings_ShouldReturn400_WhenUserIdIsEmpty()
    {
        // Arrange
        var emptyUserId = string.Empty;

        // Act
        var response = await _client.GetAsync($"/api/ai-settings?UserId={emptyUserId}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var problem = await response.Content.ReadFromJsonAsync<CustomProblemDetails>();
        problem.ShouldNotBeNull();
        problem.Title.ShouldBe("One or more validation errors occurred.");

        problem.Errors.ShouldContainKey("UserId");
        problem.Errors["UserId"].ShouldContain("The UserId field is required.");
    }

    [Fact]
    public async Task UpdateAiSettings_ShouldReturn400_WhenIdIsZero()
    {
        // Arrange
        var command = new UpdateAiSettingsCommand
        {
            Id = 0,
            UserId = ApplicationUserSeed.User1Id,
            ApiKey = "sk-validkey1234567890"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/ai-settings/{command.Id}", command);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var problem = await response.Content.ReadFromJsonAsync<CustomProblemDetails>();
        problem.ShouldNotBeNull();
        problem.Title.ShouldBe("One or more validation errors occurred.");
        problem.Errors.ShouldContainKey("Id");
        problem.Errors["Id"].ShouldContain("Id must be greater than 0.");
    }

    [Fact]
    public async Task UpdateAiSettings_ShouldReturn400_WhenUserIdIsEmpty()
    {
        // Arrange
        var command = new UpdateAiSettingsCommand
        {
            Id = AiSettingsSeed.AiSettings1Id,
            UserId = "",
            ApiKey = "sk-validkey1234567890"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/ai-settings/{command.Id}", command);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var problem = await response.Content.ReadFromJsonAsync<CustomProblemDetails>();
        problem.ShouldNotBeNull();
        problem.Errors.ShouldContainKey("UserId");
        problem.Errors["UserId"].ShouldContain("UserId is required");
    }

    [Fact]
    public async Task UpdateAiSettings_ShouldReturn400_WhenUserIdHasInvalidCharacters()
    {
        // Arrange
        var command = new UpdateAiSettingsCommand
        {
            Id = AiSettingsSeed.AiSettings1Id,
            UserId = "user@#!",
            ApiKey = "sk-validkey1234567890"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/ai-settings/{command.Id}", command);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var problem = await response.Content.ReadFromJsonAsync<CustomProblemDetails>();
        problem.ShouldNotBeNull();
        problem.Errors.ShouldContainKey("UserId");
        problem.Errors["UserId"].ShouldContain("UserId contains invalid characters.");
    }

    [Fact]
    public async Task UpdateAiSettings_ShouldReturn400_WhenApiKeyIsInvalid()
    {
        // Arrange
        var command = new UpdateAiSettingsCommand
        {
            Id = AiSettingsSeed.AiSettings1Id,
            UserId = ApplicationUserSeed.User1Id,
            ApiKey = "invalid-key"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/ai-settings/{command.Id}", command);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var problem = await response.Content.ReadFromJsonAsync<CustomProblemDetails>();
        problem.ShouldNotBeNull();
        problem.Errors.ShouldContainKey("ApiKey");
        problem.Errors["ApiKey"].ShouldContain("ApiKey must be a valid OpenAI API key.");
    }

    [Fact]
    public async Task UpdateAiSettings_ShouldReturn400_WhenAiSettingsDoesNotExist()
    {
        // Arrange
        var command = new UpdateAiSettingsCommand
        {
            Id = 9999, // non-existent
            UserId = ApplicationUserSeed.User1Id,
            ApiKey = "sk-validkey1234567890"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/ai-settings/{command.Id}", command);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var problem = await response.Content.ReadFromJsonAsync<CustomProblemDetails>();
        problem.ShouldNotBeNull();
        problem.Title.ShouldBe("One or more validation errors occurred.");
        problem.Errors.ShouldContainKey("Id");
        problem.Errors["Id"].ShouldContain("Ai settings not found.");
    }

    [Fact]
    public async Task UpdateAiSettings_ShouldReturn400_WhenAiSettingsUserMismatch()
    {
        // Arrange
        var command = new UpdateAiSettingsCommand
        {
            Id = AiSettingsSeed.AiSettings2Id,
            UserId = ApplicationUserSeed.User1Id, // belongs to another user
            ApiKey = "sk-validkey1234567890"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/ai-settings/{command.Id}", command);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var problem = await response.Content.ReadFromJsonAsync<CustomProblemDetails>();
        problem.ShouldNotBeNull();
        problem.Title.ShouldBe("One or more validation errors occurred.");
        problem.Errors.ShouldContainKey("UserId");
        problem.Errors["UserId"].ShouldContain("Ai settings not assigned to this user.");
    }

    [Fact]
    public async Task UpdateAiSettings_ShouldReturn200_WhenRequestIsValid()
    {
        // Arrange
        var validRequest = new UpdateAiSettingsCommand
        {
            Id = AiSettingsSeed.AiSettings1Id,
            UserId = ApplicationUserSeed.User1Id,
            ApiKey = "sk-ABCDEFGHIJKLMNOPQRST",
            PromptParameter = new List<PromptParameterDto>
                {
                    new() { Key = "Temperature", Value = "0.8" },
                    new() { Key = "MaxTokens", Value = "1000" }
                }
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/ai-settings/{validRequest.Id}", validRequest);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task UpdateAiSettings_ShouldReturn400_WhenDuplicateKeysExist()
    {
        // Arrange
        var invalidRequest = new UpdateAiSettingsCommand
        {
            Id = AiSettingsSeed.AiSettings1Id,
            UserId = ApplicationUserSeed.User1Id,
            ApiKey = "sk-ABCDEFGHIJKLMNOPQRST",
            PromptParameter = new List<PromptParameterDto>
                {
                    new() { Key = "Temperature", Value = "0.8" },
                    new() { Key = "Temperature", Value = "0.9" }
                }
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/ai-settings/{invalidRequest.Id}", invalidRequest);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var problem = await response.Content.ReadFromJsonAsync<CustomProblemDetails>();
        problem.ShouldNotBeNull();
        problem.Title.ShouldBe("One or more validation errors occurred.");
        problem.Errors.ShouldContainKey("PromptParameter");
        problem.Errors["PromptParameter"].ShouldContain("Each prompt parameter key must be unique.");
    }

    [Fact]
    public async Task UpdateAiSettings_ShouldReturn400_WhenValueIsEmpty()
    {
        // Arrange
        var invalidRequest = new UpdateAiSettingsCommand
        {
            Id = AiSettingsSeed.AiSettings1Id,
            UserId = ApplicationUserSeed.User1Id,
            ApiKey = "sk-ABCDEFGHIJKLMNOPQRST",
            PromptParameter = new List<PromptParameterDto>
                {
                    new() { Key = "Temperature", Value = "" }
                }
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/ai-settings/{invalidRequest.Id}", invalidRequest);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var problem = await response.Content.ReadFromJsonAsync<CustomProblemDetails>();
        problem.ShouldNotBeNull();
        problem.Errors.ShouldContainKey("PromptParameter[0].Value");
        problem.Errors["PromptParameter[0].Value"].ShouldContain("Value is required.");
    }

    [Fact]
    public async Task UpdateAiSettings_ShouldReturn400_WhenKeyIsEmpty()
    {
        // Arrange
        var invalidRequest = new UpdateAiSettingsCommand
        {
            Id = AiSettingsSeed.AiSettings1Id,
            UserId = ApplicationUserSeed.User1Id,
            ApiKey = "sk-ABCDEFGHIJKLMNOPQRST",
            PromptParameter = new List<PromptParameterDto>
                {
                    new() { Key = "", Value = "TestValue" }
                }
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/ai-settings/{invalidRequest.Id}", invalidRequest);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var problem = await response.Content.ReadFromJsonAsync<CustomProblemDetails>();
        problem.ShouldNotBeNull();
        problem.Errors.ShouldContainKey("PromptParameter[0].Key");
        problem.Errors["PromptParameter[0].Key"].ShouldContain("Key is required.");
    }
}
