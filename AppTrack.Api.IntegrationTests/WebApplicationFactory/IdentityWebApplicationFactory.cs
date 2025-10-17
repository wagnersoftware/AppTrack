using AppTrack.Identity.DBContext;
using AppTrack.Persistance.DatabaseContext;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.MsSql;

namespace AppTrack.Api.IntegrationTests;

/// <summary>
/// Provides a test web application factory for integration testing with ASP.NET Core Identity, using a containerized
/// SQL Server database and support for authenticated HTTP clients.
/// </summary>
/// <remarks>This factory configures the test host to use a dedicated SQL Server container for database isolation
/// and applies migrations automatically on initialization. It also provides methods to create HTTP clients with
/// authentication headers for testing secured endpoints. The class implements IAsyncLifetime to ensure proper startup
/// and disposal of the database container during test execution.</remarks>
public class IdentityWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MsSqlContainer _dbContainer;

    public IdentityWebApplicationFactory()
    {
        _dbContainer = new MsSqlBuilder()
            .WithPassword("Test1234!")
            .Build();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<AppTrackDatabaseContext>));
            services.RemoveAll(typeof(DbContextOptions<AppTrackIdentityDbContext>));

            services.AddDbContext<AppTrackDatabaseContext>(options =>
                options.UseSqlServer(_dbContainer.GetConnectionString()));

            services.AddDbContext<AppTrackIdentityDbContext>(options =>
                options.UseSqlServer(_dbContainer.GetConnectionString()));
        });
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();

        using var scope = Services.CreateScope();
        var mainDb = scope.ServiceProvider.GetRequiredService<AppTrackDatabaseContext>();
        var identityDb = scope.ServiceProvider.GetRequiredService<AppTrackIdentityDbContext>();

        await mainDb.Database.MigrateAsync();
        await identityDb.Database.MigrateAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _dbContainer.StopAsync();
        await _dbContainer.DisposeAsync();
    }
}
