using AppTrack.Application.Contracts.Email;
using AppTrack.Application.Models.Email;
using Azure;
using Azure.Communication.Email;
using Azure.Identity;
using Microsoft.Extensions.Options;
using AcsEmailMessage = Azure.Communication.Email.EmailMessage;

namespace AppTrack.Infrastructure.EmailService;

public class EmailSender : IEmailSender
{
    public EmailSettings EmailSettings { get; }

    public EmailSender(IOptions<EmailSettings> emailSettings)
    {
        EmailSettings = emailSettings.Value;
    }

    public async Task<bool> SendEmail(Application.Models.Email.EmailMessage email)
    {
        var client = new EmailClient(new Uri(EmailSettings.Endpoint), new DefaultAzureCredential());
        var message = new AcsEmailMessage(
            senderAddress: EmailSettings.FromAddress,
            content: new EmailContent(email.Subject) { Html = email.Body },
            recipients: new EmailRecipients([new EmailAddress(email.To)])
        );
        var operation = await client.SendAsync(WaitUntil.Completed, message);
        return operation.Value.Status == EmailSendStatus.Succeeded;
    }
}
