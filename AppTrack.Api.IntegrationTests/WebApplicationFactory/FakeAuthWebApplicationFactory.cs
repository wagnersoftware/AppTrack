using AppTrack.Api.IntegrationTests.Auth;
using AppTrack.Api.IntegrationTests.Seeddata;
using AppTrack.Identity.DBContext;
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

/// <summary>
/// Provides a test web application factory for integration testing with a SQL Server test container and fake
/// authentication configured.
/// </summary>
/// <remarks>This factory replaces the application's database context with a test container instance and
/// configures a test authentication scheme, allowing tests to run against an isolated database and bypass real
/// authentication. Use this class to create test clients that simulate authenticated requests and ensure database state
/// isolation between tests.</remarks>
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
        builder.ConfigureTestServices(services =>
        {
            // Replace SQL with Testcontainer DB
            services.RemoveAll(typeof(DbContextOptions<AppTrackDatabaseContext>));
            services.RemoveAll(typeof(DbContextOptions<AppTrackIdentityDbContext>));

            services.AddDbContext<AppTrackDatabaseContext>(options =>
                options.UseSqlServer(_dbContainer.GetConnectionString()));

            services.AddDbContext<AppTrackIdentityDbContext>(options =>
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
        var identityDb = scope.ServiceProvider.GetRequiredService<AppTrackIdentityDbContext>();

        await mainDb.Database.MigrateAsync();
        await identityDb.Database.MigrateAsync();

        await SeedTestData.SeedDataAsync(mainDb, identityDb);
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _dbContainer.StopAsync();
        await _dbContainer.DisposeAsync();
    }
}
