using AppTrack.Api.Models;
using AppTrack.Application.Models.Identity;
using Shouldly;
using System.Net;
using System.Net.Http.Json;

namespace AppTrack.Api.IntegrationTests;

public class AuthEndpointsTests : IClassFixture<IdentityWebApplicationFactory>
{
    private readonly string _userName = "testuser";
    private readonly string _password = "Test1234!";

    [Fact]
    public async Task Register_ShouldCreateNewUser()
    {
        await using var factory = new IdentityWebApplicationFactory();
        await factory.InitializeAsync();
        var client = factory.CreateClient();

        // Arrange
        var request = new
        {
            UserName = _userName,
            Password = _password,
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/authentication/register", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<RegistrationResponse>();
        result.ShouldNotBeNull();
        result.UserId.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task Register_ShouldFail_WhenUsernameOrPasswordInvalid()
    {
        // Initialize test factory and HTTP client
        await using var factory = new IdentityWebApplicationFactory();
        await factory.InitializeAsync();
        var client = factory.CreateClient();

        // ---------- USERNAME RULES ----------

        // Test 1: Username is empty
        var request1 = new { UserName = "", Password = _password };
        var response1 = await client.PostAsJsonAsync("/api/authentication/register", request1);
        response1.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var problem1 = await response1.Content.ReadFromJsonAsync<CustomProblemDetails>();
        problem1!.Errors["UserName"].ShouldContain("Username is required");

        // Test 2: Username too short
        var request2 = new { UserName = "ab", Password = _password };
        var response2 = await client.PostAsJsonAsync("/api/authentication/register", request2);
        response2.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var problem2 = await response2.Content.ReadFromJsonAsync<CustomProblemDetails>();
        problem2!.Errors["UserName"].ShouldContain("Username must be at least 3 characters long");

        // Test 3: Username too long
        var request3 = new { UserName = "ThisUserNameIsWayTooLong123", Password = _password };
        var response3 = await client.PostAsJsonAsync("/api/authentication/register", request3);
        response3.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var problem3 = await response3.Content.ReadFromJsonAsync<CustomProblemDetails>();
        problem3!.Errors["UserName"].ShouldContain("Username must not exceed 20 characters");

        // Test 4: Username contains invalid characters
        var request4 = new { UserName = "Invalid!User", Password = _password };
        var response4 = await client.PostAsJsonAsync("/api/authentication/register", request4);
        response4.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var problem4 = await response4.Content.ReadFromJsonAsync<CustomProblemDetails>();
        problem4!.Errors["UserName"].ShouldContain("Username can only contain letters, numbers, hyphens and underscores");

        // ---------- PASSWORD RULES ----------

        // Test 5 Password is empty
        var request5 = new { UserName = "User1", Password = "" };
        var response5 = await client.PostAsJsonAsync("/api/authentication/register", request5);
        response5.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var problem5 = await response5.Content.ReadFromJsonAsync<CustomProblemDetails>();
        problem5!.Errors["Password"].ShouldContain("Password is required");

        // Test 6: Password too short
        var request6 = new { UserName = "User2", Password = "Ab1!" };
        var response6 = await client.PostAsJsonAsync("/api/authentication/register", request6);
        response6.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var problem6 = await response6.Content.ReadFromJsonAsync<CustomProblemDetails>();
        problem6!.Errors["Password"].ShouldContain("Password must be at least 6 characters long");

        // Test 7: Password missing uppercase
        var request7 = new { UserName = "User3", Password = "abc123!" };
        var response7 = await client.PostAsJsonAsync("/api/authentication/register", request7);
        response7.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var problem7 = await response7.Content.ReadFromJsonAsync<CustomProblemDetails>();
        problem7!.Errors["Password"].ShouldContain("Password must contain at least one uppercase letter");

        // Test 8: Password missing lowercase
        var request8 = new { UserName = "User4", Password = "ABC123!" };
        var response8 = await client.PostAsJsonAsync("/api/authentication/register", request8);
        response8.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var problem8 = await response8.Content.ReadFromJsonAsync<CustomProblemDetails>();
        problem8!.Errors["Password"].ShouldContain("Password must contain at least one lowercase letter");

        // Test 9: Password missing number
        var request9 = new { UserName = "User5", Password = "Abcdef!" };
        var response9 = await client.PostAsJsonAsync("/api/authentication/register", request9);
        response9.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var problem9 = await response9.Content.ReadFromJsonAsync<CustomProblemDetails>();
        problem9!.Errors["Password"].ShouldContain("Password must contain at least one number");

        // Test 10: Password missing special character
        var request10 = new { UserName = "User6", Password = "Abc1234" };
        var response10 = await client.PostAsJsonAsync("/api/authentication/register", request10);
        response10.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var problem10 = await response10.Content.ReadFromJsonAsync<CustomProblemDetails>();
        problem10!.Errors["Password"].ShouldContain("Password must contain at least one special character");

        // ---------- DUPLICATE USER ----------

        // Test 11: Username already exists
        var request11 = new { UserName = _userName, Password = _password };
        var response11a = await client.PostAsJsonAsync("/api/authentication/register", request11);
        response11a.StatusCode.ShouldBe(HttpStatusCode.Created);

        var response11b = await client.PostAsJsonAsync("/api/authentication/register", request11);
        response11b.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        var problem11 = await response11b.Content.ReadFromJsonAsync<CustomProblemDetails>();
        problem11!.Title.ShouldBe("User with this username or email already exists.");
    }

    [Fact]
    public async Task Login_ShouldReturnJwtToken_WhenCredentialsAreValid()
    {
        await using var factory = new IdentityWebApplicationFactory();
        await factory.InitializeAsync();
        var client = factory.CreateClient();

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
        _ = await client.PostAsJsonAsync("/api/authentication/register", registerRequest);
        var response = await client.PostAsJsonAsync("/api/authentication/login", loginRequest);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
        result.ShouldNotBeNull();
        result.Token.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_ShouldReturnBadRequest_WhenPasswordIsInvalid()
    {
        await using var factory = new IdentityWebApplicationFactory();
        await factory.InitializeAsync();
        var client = factory.CreateClient();

        // Arrange   
        var registerRequest = new
        {
            UserName = _userName,
            Password = _password,
        };

        var loginRequest = new
        {
            UserName = _userName,
            Password = "InvalidPassword123!",
        };

        // Act
        _ = await client.PostAsJsonAsync("/api/authentication/register", registerRequest);
        var response = await client.PostAsJsonAsync("/api/authentication/login", loginRequest);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<CustomProblemDetails>();
        problem.ShouldNotBeNull();
        problem.Status.ShouldBe((int)HttpStatusCode.BadRequest);
        problem.Title.ShouldBe($"Wrong credentials for user {_userName}");
    }

    [Fact]
    public async Task Login_ShouldReturnNotFound_WhenUserDoesntExist()
    {
        await using var factory = new IdentityWebApplicationFactory();
        await factory.InitializeAsync();
        var client = factory.CreateClient();

        var loginRequest = new
        {
            UserName = _userName,
            Password = _password,
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/authentication/login", loginRequest);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        var problem = await response.Content.ReadFromJsonAsync<CustomProblemDetails>();
        problem.ShouldNotBeNull();
        problem.Status.ShouldBe((int)HttpStatusCode.NotFound);
        problem.Title.ShouldBe($"User {_userName} not found");
    }

    [Fact]
    public async Task Login_ShouldReturnBadRequest_WhenValidationRulesViolated()
    {
        // Initialize test factory and HTTP client
        await using var factory = new IdentityWebApplicationFactory();
        await factory.InitializeAsync();
        var client = factory.CreateClient();

        // ---------- USERNAME RULES ----------

        // 1. Empty username
        var request1 = new { UserName = "", Password = _password };
        var response1 = await client.PostAsJsonAsync("/api/authentication/login", request1);
        response1.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var problem1 = await response1.Content.ReadFromJsonAsync<CustomProblemDetails>();
        problem1!.Errors["UserName"].ShouldContain("Username is required");

        // 2. Too short
        var request2 = new { UserName = "ab", Password = _password };
        var response2 = await client.PostAsJsonAsync("/api/authentication/login", request2);
        response2.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var problem2 = await response2.Content.ReadFromJsonAsync<CustomProblemDetails>();
        problem2!.Errors["UserName"].ShouldContain("Username must be at least 3 characters long");

        // 3. Too long
        var request3 = new { UserName = "ThisUserNameIsWayTooLong123", Password = _password };
        var response3 = await client.PostAsJsonAsync("/api/authentication/login", request3);
        response3.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var problem3 = await response3.Content.ReadFromJsonAsync<CustomProblemDetails>();
        problem3!.Errors["UserName"].ShouldContain("Username must not exceed 20 characters");

        // 4. Invalid characters
        var request4 = new { UserName = "Invalid!User", Password = _password };
        var response4 = await client.PostAsJsonAsync("/api/authentication/login", request4);
        response4.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var problem4 = await response4.Content.ReadFromJsonAsync<CustomProblemDetails>();
        problem4!.Errors["UserName"].ShouldContain("Username can only contain letters, numbers, hyphens and underscores");

        // ---------- PASSWORD RULES ----------

        // 5. Empty password
        var request5 = new { UserName = _userName, Password = "" };
        var response5 = await client.PostAsJsonAsync("/api/authentication/login", request5);
        response5.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var problem5 = await response5.Content.ReadFromJsonAsync<CustomProblemDetails>();
        problem5!.Errors["Password"].ShouldContain("Password is required");

        // 6. Too short
        var request6 = new { UserName = _userName, Password = "Ab1!" };
        var response6 = await client.PostAsJsonAsync("/api/authentication/login", request6);
        response6.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var problem6 = await response6.Content.ReadFromJsonAsync<CustomProblemDetails>();
        problem6!.Errors["Password"].ShouldContain("Password must be at least 6 characters long");

        // 7. Missing uppercase
        var request7 = new { UserName = _userName, Password = "abc123!" };
        var response7 = await client.PostAsJsonAsync("/api/authentication/login", request7);
        response7.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var problem7 = await response7.Content.ReadFromJsonAsync<CustomProblemDetails>();
        problem7!.Errors["Password"].ShouldContain("Password must contain at least one uppercase letter");

        // 8. Missing lowercase
        var request8 = new { UserName = _userName, Password = "ABC123!" };
        var response8 = await client.PostAsJsonAsync("/api/authentication/login", request8);
        response8.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var problem8 = await response8.Content.ReadFromJsonAsync<CustomProblemDetails>();
        problem8!.Errors["Password"].ShouldContain("Password must contain at least one lowercase letter");

        // 9. Missing number
        var request9 = new { UserName = _userName, Password = "Abcdef!" };
        var response9 = await client.PostAsJsonAsync("/api/authentication/login", request9);
        response9.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var problem9 = await response9.Content.ReadFromJsonAsync<CustomProblemDetails>();
        problem9!.Errors["Password"].ShouldContain("Password must contain at least one number");

        // 10. Missing special character
        var request10 = new { UserName = _userName, Password = "Abc1234" };
        var response10 = await client.PostAsJsonAsync("/api/authentication/login", request10);
        response10.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var problem10 = await response10.Content.ReadFromJsonAsync<CustomProblemDetails>();
        problem10!.Errors["Password"].ShouldContain("Password must contain at least one special character");
    }
}
