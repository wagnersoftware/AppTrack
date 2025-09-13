using AppTrack.Application.Models;

namespace AppTrack.Application.Contracts.Email;

public interface IEmailSender
{
    Task<bool> SendEmail(EmailMessage email);
}
