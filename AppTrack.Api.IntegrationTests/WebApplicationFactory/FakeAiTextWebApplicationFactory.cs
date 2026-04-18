using AppTrack.Application.Contracts.AiTextGenerator;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AppTrack.Api.IntegrationTests.WebApplicationFactory;

/// <summary>
/// Extends <see cref="FakeAuthWebApplicationFactory"/> by replacing <see cref="IAiTextGenerator"/>
/// with a stub so that integration tests do not call the real OpenAI API.
/// </summary>
public class FakeAiTextWebApplicationFactory : FakeAuthWebApplicationFactory
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IAiTextGenerator>();
            services.AddScoped<IAiTextGenerator, StubAiTextGenerator>();
        });
    }
}
