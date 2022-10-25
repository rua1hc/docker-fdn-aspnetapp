namespace NotificationService.Models
{
    public class MailMessage
    {
        public List<string> To { get; }
        public List<string> Cc { get; }
        public List<string> Bcc { get; }

        public string? From { get; }
        public string? DisplayName { get; }
        public string? ReplyTo { get; }
        public string? ReplyToName { get; }

        public string Subject { get; }
        public string? Body { get; }

        public MailMessage(string subject,
            List<string>? to = null,
            string? from = null,
            string? body = null,
            string? displayName = null,
            string? replyTo = null,
            string? replyToName = null,
            List<string>? bcc = null,
            List<string>? cc = null)
        {
            To = to ?? new List<string>();
            Bcc = bcc ?? new List<string>();
            Cc = cc ?? new List<string>();

            From = from;
            DisplayName = displayName;
            ReplyTo = replyTo;
            ReplyToName = replyToName;

            Subject = subject;
            Body = body;
        }
    }
}
