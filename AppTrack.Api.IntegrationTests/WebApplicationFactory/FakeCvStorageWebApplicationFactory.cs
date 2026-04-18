using AppTrack.Application.Contracts.CvStorage;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AppTrack.Api.IntegrationTests.WebApplicationFactory;

/// <summary>
/// Extends <see cref="FakeAuthWebApplicationFactory"/> by replacing <see cref="ICvStorageService"/>
/// and <see cref="IPdfTextExtractor"/> with stubs so tests do not touch Azure Blob Storage.
/// </summary>
public class FakeCvStorageWebApplicationFactory : FakeAuthWebApplicationFactory
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<ICvStorageService>();
            services.AddScoped<ICvStorageService, StubCvStorageService>();

            services.RemoveAll<IPdfTextExtractor>();
            services.AddSingleton<IPdfTextExtractor, StubPdfTextExtractor>();
        });
    }
}
