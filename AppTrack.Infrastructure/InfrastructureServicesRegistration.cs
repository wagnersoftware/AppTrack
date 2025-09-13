using AppTrack.Application.Contracts.Email;
using AppTrack.Application.Models;
using AppTrack.Infrastructure.EmailService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AppTrack.Infrastructure
{
    public static class InfrastructureServicesRegistration
    {
        public static IServiceCollection RegisterServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<EmailSettings>(configuration.GetSection(nameof(EmailSettings)));
            services.AddTransient<IEmailSender, EmailSender>();
            return services;
        }
    }
}
