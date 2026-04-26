using AppTrack.Application;
using AppTrack.Application.Contracts;
using AppTrack.Functions.Identity;
using AppTrack.Infrastructure;
using AppTrack.Persistance;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((context, services) =>
    {
        services.AddApplicationServices();
        services.AddInfrastructureServices(context.Configuration);
        services.AddPersistanceServices(context.Configuration);

        // Override the HTTP-context-based IUserContext registered by AddInfrastructureServices.
        // Timer-triggered functions run without an HTTP context; the NullUserContext is safe
        // because ScrapePortalsCommand does not implement IUserScopedRequest.
        services.AddScoped<IUserContext, NullUserContext>();
    })
    .Build();

await host.RunAsync();
