using AppTrack.Application.Contracts.Email;
using AppTrack.Application.Contracts.Logging;
using AppTrack.Application.Models;
using AppTrack.Infrastructure.EmailService;
using AppTrack.Infrastructure.Logging;
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
            services.AddScoped(typeof(IAppLogger<>), typeof(LoggingAdapter<>));
            return services;
        }
    }
}
