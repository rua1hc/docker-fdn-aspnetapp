using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net.Mail;
using System.Net;
using WebApp1.Config;

namespace WebApp1.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly EmailConfiguration _emailConfig;

        public EmailSender(EmailConfiguration emailConfig)
        {
            _emailConfig = emailConfig;
        }


        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            string fromMail = "[YOUREMAILID]";
            string fromPassword = "[APPPASSWORD]";
            string userState = "userId-1";

            MailMessage message = new MailMessage();
            message.From = new MailAddress(fromMail);
            message.Subject = userState + "-title";
            message.To.Add(new MailAddress(email));
            message.Body = "<html><body> " + htmlMessage + " </body></html>";
            message.IsBodyHtml = true;

            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(fromMail, fromPassword),
                EnableSsl = true,
            };
            //smtpClient.SendAsync(message, userState);
            await smtpClient.SendMailAsync(message);
        }

    }
}
