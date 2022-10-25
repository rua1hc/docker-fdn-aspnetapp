using NotificationService.Models;

namespace NotificationService.Services
{
    public interface IMailService
    {
        public void SendMail(string mailContent);
        public Task<bool> SendMailAsync(MailMessage mailMessage, CancellationToken ct);
    }
}
