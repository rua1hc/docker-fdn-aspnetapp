using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using NotificationService.Configuration;
using NotificationService.Models;
//using System.Net.Mail;

namespace NotificationService.Services
{
    public class MailService : IMailService
    {
        private readonly IConfiguration _configuration;
        private readonly Smtp _smtp;

        public MailService(IConfiguration configuration, IOptions<Smtp> smtp)
        {
            _configuration = configuration;
            _smtp = smtp.Value;
        }

        public void SendMail(string mailContent)
        {
            MimeMessage mail = new MimeMessage();
            mail.From.Add(new MailboxAddress("NotificationService", _configuration["Smtp:From"]));
            mail.To.Add(new MailboxAddress("Mr. Test", _configuration["Smtp:To"]));

            mail.Subject = "Course Enrollment Registration";

            BodyBuilder bodyBuilder = new BodyBuilder();
            bodyBuilder.TextBody = mailContent;// order.ToString();
            mail.Body = bodyBuilder.ToMessageBody();

            using (var client = new SmtpClient())
            {
                client.Connect(_configuration["Smtp:Host"], 587, false);
                client.Authenticate(_configuration["Smtp:Username"], _configuration["Smtp:Password"]);

                client.Send(mail);
                client.Disconnect(true);
            }
        }


        public async Task<bool> SendMailAsync(MailMessage mailMessage, CancellationToken ct = default)
        {
            try
            {
                var mail = new MimeMessage();

                #region Sender/Receiver
                mail.From.Add(new MailboxAddress(_smtp.DisplayName, mailMessage.From ?? _smtp.From));
                mail.Sender = new MailboxAddress(mailMessage.DisplayName ?? _smtp.DisplayName, mailMessage.From ?? _smtp.From);

                mail.To.Add(new MailboxAddress(_smtp.To, _smtp.Username));
                if (mailMessage.To != null)
                {
                    foreach (string mailAddress in mailMessage.To)
                        mail.To.Add(MailboxAddress.Parse(mailAddress));
                }

                //
                if (!string.IsNullOrEmpty(mailMessage.ReplyTo))
                    mail.ReplyTo.Add(new MailboxAddress(mailMessage.ReplyToName, mailMessage.ReplyTo));

                if (mailMessage.Bcc != null)
                {
                    foreach (string mailAddress in mailMessage.Bcc.Where(x => !string.IsNullOrWhiteSpace(x)))
                        mail.Bcc.Add(MailboxAddress.Parse(mailAddress.Trim()));
                }
                if (mailMessage.Cc != null)
                {
                    foreach (string mailAddress in mailMessage.Cc.Where(x => !string.IsNullOrWhiteSpace(x)))
                        mail.Cc.Add(MailboxAddress.Parse(mailAddress.Trim()));
                }
                #endregion

                #region Content
                var body = new BodyBuilder();
                mail.Subject = mailMessage.Subject;
                body.HtmlBody = mailMessage.Body;
                mail.Body = body.ToMessageBody();
                #endregion

                #region Send Mail
                using var smtp = new SmtpClient();

                if (_smtp.UseSSL)
                {
                    await smtp.ConnectAsync(_smtp.Host, _smtp.Port, SecureSocketOptions.SslOnConnect, ct);
                }
                else if (_smtp.UseStartTls)
                {
                    await smtp.ConnectAsync(_smtp.Host, _smtp.Port, SecureSocketOptions.StartTls, ct);
                }
                await smtp.AuthenticateAsync(_smtp.Username, _smtp.Password, ct);
                await smtp.SendAsync(mail, ct);
                await smtp.DisconnectAsync(true, ct);
                #endregion

                return true;

            }
            catch (Exception)
            {
                return false;
            }
        }

    }
}
