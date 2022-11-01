
using System;

namespace cs_fdn
{
    public class VideoFilters
    {
        public void ApplyBrightness(Video video)
        {
            Console.WriteLine("Apply brightness - " + video.Title);
        }

        public void ApplyContrast(Video video)
        {
            Console.WriteLine("Apply contrast - " + video.Title);
        }
    }
}
