using AppTrack.Application.Contracts;
using AppTrack.Application.Contracts.AiTextGenerator;
using AppTrack.Application.Contracts.CvStorage;
using AppTrack.Application.Contracts.Email;
using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Models.Email;
using AppTrack.Infrastructure.AiTextGeneration;
using AppTrack.Infrastructure.CvStorage;
using AppTrack.Infrastructure.EmailService;
using AppTrack.Infrastructure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AppTrack.Infrastructure
{
    public static class InfrastructureServicesRegistration
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<EmailSettings>(configuration.GetSection(nameof(EmailSettings)));
            services.AddTransient<IEmailSender, EmailSender>();

            // IHttpContextAccessor is required by HttpContextUserContext to resolve the current user.
            services.AddHttpContextAccessor();

            services.AddScoped<IUserContext, HttpContextUserContext>();
            services.AddScoped<IMediator, Mediator.Mediator>();

            services.AddHttpClient<IAiTextGenerator, OpenAiAiTextGenerator>();

            services.Configure<AzureStorageSettings>(configuration.GetSection(nameof(AzureStorageSettings)));
            services.AddScoped<ICvStorageService, AzureBlobStorageService>();
            services.AddSingleton<IPdfTextExtractor, PdfPigTextExtractor>();

            return services;
        }
    }
}
