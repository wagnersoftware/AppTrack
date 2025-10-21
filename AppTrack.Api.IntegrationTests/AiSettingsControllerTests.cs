﻿using AppTrack.Api.IntegrationTests.Seeddata;
using AppTrack.Api.IntegrationTests.Seeddata.User;
using AppTrack.Api.Models;
using AppTrack.Application.Features.AiSettings.Commands.UpdateAiSettings;
using AppTrack.Application.Features.AiSettings.Dto;
using Shouldly;
using System.Net;
using System.Net.Http.Json;

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
    public async Task UpdateAiSettings_ShouldReturn400_WhenIdIsZero()
    {
        var userId = await ApplicationUserSeedHelper.CreateTestUserAsync(_factory.Services);

        // Arrange
        var command = new UpdateAiSettingsCommand
        {
            Id = 0,
            UserId = userId,
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
        var (_, aiSettingsId) = await SeedHelper.CreateUserWithAiSettingsAsync(_factory.Services);

        // Arrange
        var command = new UpdateAiSettingsCommand
        {
            Id = aiSettingsId,
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
        var (_, aiSettingsId) = await SeedHelper.CreateUserWithAiSettingsAsync(_factory.Services);

        // Arrange
        var command = new UpdateAiSettingsCommand
        {
            Id = aiSettingsId,
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
        var (userId, aiSettingsId) = await SeedHelper.CreateUserWithAiSettingsAsync(_factory.Services);

        // Arrange
        var command = new UpdateAiSettingsCommand
        {
            Id = aiSettingsId,
            UserId = userId,
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
        var userId = await ApplicationUserSeedHelper.CreateTestUserAsync(_factory.Services);

        // Arrange
        var command = new UpdateAiSettingsCommand
        {
            Id = 9999, // non-existent
            UserId = userId,
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
        var randomUserId = await ApplicationUserSeedHelper.CreateTestUserAsync(_factory.Services);
        var (_, aiSettingsId) = await SeedHelper.CreateUserWithAiSettingsAsync(_factory.Services);

        // Arrange
        var command = new UpdateAiSettingsCommand
        {
            Id = aiSettingsId,
            UserId = randomUserId, // belongs to another user
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
        var (userId, aiSettingsId) = await SeedHelper.CreateUserWithAiSettingsAsync(_factory.Services);
        var validRequest = new UpdateAiSettingsCommand
        {
            Id = aiSettingsId,
            UserId = userId,
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
        var (userId, aiSettingsId) = await SeedHelper.CreateUserWithAiSettingsAsync(_factory.Services);

        var invalidRequest = new UpdateAiSettingsCommand
        {
            Id = aiSettingsId,
            UserId = userId,
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
        var (userId, aiSettingsId) = await SeedHelper.CreateUserWithAiSettingsAsync(_factory.Services);

        var invalidRequest = new UpdateAiSettingsCommand
        {
            Id = aiSettingsId,
            UserId = userId,
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
        var (userId, aiSettingsId) = await SeedHelper.CreateUserWithAiSettingsAsync(_factory.Services);

        var invalidRequest = new UpdateAiSettingsCommand
        {
            Id = aiSettingsId,
            UserId = userId,
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
