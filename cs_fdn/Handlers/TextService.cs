using System;

namespace cs_fdn
{
    public class TextService
    {
        public void OnVideoEncoded(object src, VideoEventArgs e)
        {
            Console.WriteLine("Sending text..." + e.Video.Title);
        }
    }
}
