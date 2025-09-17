using AppTrack.Application.Models.Email;

namespace AppTrack.Application.Contracts.Email;

public interface IEmailSender
{
    Task<bool> SendEmail(EmailMessage email);
}
