using AppTrack.Application.Contracts.Email;
using AppTrack.Application.Models;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace AppTrack.Infrastructure.EmailService;

public class EmailSender : IEmailSender
{
    public EmailSettings EmailSettings { get; }

    public EmailSender(IOptions<EmailSettings> emailSettings)
    {
        EmailSettings = emailSettings.Value;
    }

    public async Task<bool> SendEmail(EmailMessage email)
    {
        var client = new SendGridClient(EmailSettings.ApiKey);
        var to = new EmailAddress(email.To);

        var from = new EmailAddress
        {
            Email = EmailSettings.FromAddress,
            Name = EmailSettings.FromName
        };

        var message = MailHelper.CreateSingleEmail(from, to, email.Subject, email.Body, email.Body);
        var response = await client.SendEmailAsync(message);

        return response.IsSuccessStatusCode;
    }
}
