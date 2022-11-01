using System;

namespace cs_fdn
{
    public class MailSerice
    {
        public void OnVideoEncoded(object src, VideoEventArgs e)
        {
            Console.WriteLine("Sending email..." + e.Video.Title);
        }
    }
}
