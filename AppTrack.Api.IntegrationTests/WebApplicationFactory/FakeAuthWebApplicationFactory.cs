using AppTrack.Api.IntegrationTests.Auth;
using AppTrack.Persistance.DatabaseContext;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.MsSql;

namespace AppTrack.Api.IntegrationTests;

public class FakeAuthWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MsSqlContainer _dbContainer;

    public FakeAuthWebApplicationFactory()
    {
        _dbContainer = new MsSqlBuilder()
            .WithPassword("Test1234!")
            .Build();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("OpenAiSettings:ApiKey", "test-api-key-integration");

        builder.ConfigureTestServices(services =>
        {
            // Replace SQL with Testcontainer DB
            services.RemoveAll(typeof(DbContextOptions<AppTrackDatabaseContext>));

            services.AddDbContext<AppTrackDatabaseContext>(options =>
                options.UseSqlServer(_dbContainer.GetConnectionString()));

            // Add Fake Authentication
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
        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue(TestAuthHandler.AuthenticationScheme);
        return client;
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();

        using var scope = Services.CreateScope();
        var mainDb = scope.ServiceProvider.GetRequiredService<AppTrackDatabaseContext>();

        await mainDb.Database.MigrateAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _dbContainer.StopAsync();
        await _dbContainer.DisposeAsync();
    }
}
