using AppTrack.Application.Contracts.RssFeed;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AppTrack.Api.IntegrationTests.WebApplicationFactory;

public class FakeRssFeedWebApplicationFactory : FakeAuthWebApplicationFactory
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IRssFeedReader>();
            services.AddScoped<IRssFeedReader, StubRssFeedReader>();
            services.RemoveAll<IRssMatchNotifier>();
            services.AddScoped<IRssMatchNotifier, StubRssMatchNotifier>();
        });
    }
}
