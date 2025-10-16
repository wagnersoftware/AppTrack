using AppTrack.Api.IntegrationTests.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;

namespace AppTrack.Api.IntegrationTests.WebApplicationFactory;

/// <summary>
/// Provides a custom web application factory for integration testing, configured to use a test authentication handler
/// for simulating authenticated requests.
/// </summary>
/// <remarks>Use this factory to create test server instances with authentication overridden for test scenarios.
/// The factory configures the test host to use a custom authentication scheme, allowing tests to bypass real
/// authentication and simulate authenticated users. This is useful for testing endpoints that require authentication
/// without relying on external identity providers.</remarks>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // Override authentication with our TestAuthHandler
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = TestAuthHandler.AuthenticationScheme;
                options.DefaultChallengeScheme = TestAuthHandler.AuthenticationScheme;
            })
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                TestAuthHandler.AuthenticationScheme, _ => { });
        });
    }

    public HttpClient CreateAuthenticatedClient()
    {
        var client = this.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(TestAuthHandler.AuthenticationScheme);
        return client;
    }

}
