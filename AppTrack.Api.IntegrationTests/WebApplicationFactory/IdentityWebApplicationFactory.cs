using AppTrack.Identity.DBContext;
using AppTrack.Persistance.DatabaseContext;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.MsSql;

namespace AppTrack.Api.IntegrationTests
{
    /// <summary>
    /// Provides a test web application factory for ASP.NET Core integration tests with identity and application
    /// database contexts configured to use an isolated SQL Server container.
    /// </summary>
    /// <remarks>This factory sets up both identity and application database contexts to use a dedicated SQL
    /// Server container for each test run, ensuring database isolation and repeatability. Implements <see
    /// cref="IAsyncLifetime"/> to support asynchronous initialization and cleanup of the test database environment. Use
    /// <see cref="ResetDatabaseAsync"/> to reset the database state between tests if needed.</remarks>
    public class IdentityWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        private readonly MsSqlContainer _dbContainer;
        private string _connectionString = null!;

        public IdentityWebApplicationFactory()
        {
            _dbContainer = new MsSqlBuilder()
                .WithPassword("Test1234!")
                .WithCleanUp(true)
                .Build();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll(typeof(DbContextOptions<AppTrackDatabaseContext>));
                services.RemoveAll(typeof(DbContextOptions<AppTrackIdentityDbContext>));

                services.AddDbContext<AppTrackIdentityDbContext>(options =>
                    options.UseSqlServer(_connectionString));
                services.AddDbContext<AppTrackDatabaseContext>(options =>
                    options.UseSqlServer(_connectionString));
            });
        }

        public async Task InitializeAsync()
        {
            await _dbContainer.StartAsync();
            _connectionString = _dbContainer.GetConnectionString();

            using var scope = Services.CreateScope();
            var identityDb = scope.ServiceProvider.GetRequiredService<AppTrackIdentityDbContext>();
            await identityDb.Database.MigrateAsync();

            var appDb = scope.ServiceProvider.GetRequiredService<AppTrackDatabaseContext>();
            await appDb.Database.MigrateAsync();
        }

        async Task IAsyncLifetime.DisposeAsync()
        {
            await _dbContainer.StopAsync();
            await _dbContainer.DisposeAsync();
        }

        public async Task ResetDatabaseAsync()
        {
            using var scope = Services.CreateScope();
            var identityDb = scope.ServiceProvider.GetRequiredService<AppTrackIdentityDbContext>();
            var appDb = scope.ServiceProvider.GetRequiredService<AppTrackDatabaseContext>();

            await identityDb.Database.EnsureDeletedAsync();
            await identityDb.Database.MigrateAsync();

            await appDb.Database.EnsureDeletedAsync();
            await appDb.Database.MigrateAsync();
        }
    }
}
