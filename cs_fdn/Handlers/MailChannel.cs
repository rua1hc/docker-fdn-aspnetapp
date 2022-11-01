using System;

namespace cs_fdn
{
    public class MailChannel : INotificationChannel
    {
        public void Send(string message)
        {
            Console.WriteLine("MailChannel: send to mail - " + message);
        }
    }

}
