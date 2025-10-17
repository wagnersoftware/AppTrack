using AppTrack.Application.Models.Identity;
using Shouldly;
using System.Net;
using System.Net.Http.Json;

namespace AppTrack.Api.IntegrationTests;

public class AuthEndpointsTests : IClassFixture<IdentityWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly string _userName = "testuser";
    private readonly string _password = "Test1234!";

    public AuthEndpointsTests(IdentityWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_ShouldCreateNewUser()
    {
        // Arrange
        var request = new
        {
            UserName = _userName,
            Password = _password,
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Auth/register", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<RegistrationResponse>();
        result.ShouldNotBeNull();
        result.UserId.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_ShouldReturnJwtToken_WhenCredentialsAreValid()
    {
        // Arrange   
        var registerRequest = new
        {
            UserName = _userName,
            Password = _password,
        };

        var loginRequest = new
        {
            UserName = _userName,
            Password = _password,
        };

        // Act
        _ = await _client.PostAsJsonAsync("/api/Auth/register", registerRequest);
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
        result.ShouldNotBeNull();
        result.Token.ShouldNotBeNullOrEmpty();
    }
}
