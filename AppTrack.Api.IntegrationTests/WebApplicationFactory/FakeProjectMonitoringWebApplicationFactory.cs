using AppTrack.Application.Contracts.ProjectMonitoring;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AppTrack.Api.IntegrationTests.WebApplicationFactory;

public class FakeProjectMonitoringWebApplicationFactory : FakeAuthWebApplicationFactory
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IProjectMatchNotifier>();
            services.AddScoped<IProjectMatchNotifier, StubProjectMatchNotifier>();
        });
    }
}
