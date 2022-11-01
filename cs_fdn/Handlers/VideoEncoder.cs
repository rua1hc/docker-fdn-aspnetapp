using System;
using System.Collections.Generic;

namespace cs_fdn
{
    public class VideoEventArgs
    {
        public Video Video { get; set; }
    }
    public class VideoEncoder
    {
        //Interface
        private readonly IList<INotificationChannel> _notificationChannels;

        public VideoEncoder()
        {
            _notificationChannels = new List<INotificationChannel>();
        }

        public void AddChannel(INotificationChannel channel)
        {
            _notificationChannels.Add(channel);
        }
        public void VideoPlaying(Video video)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("[VideoPlayer]: " + video.Title);
            //...
            foreach (var channel in _notificationChannels)
            {
                channel.Send(video.Title);
            }
        }

        //EventHandler
        //1st 
        public delegate void VideoEncodeEventHandler(object src, EventArgs e);
        public event VideoEncodeEventHandler VideoEncoded;
        //2nd
        public event EventHandler<VideoEventArgs> VideoEncodedEH;

        public void Encode(Video video)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("[Encoding]: " + video.Title);
            OnVideoEncoded(video);
        }

        protected private void OnVideoEncoded(Video video)
        {
            //if (VideoEncoded != null)
            //{
            //    VideoEncoded(this, EventArgs.Empty);
            //}
            if (VideoEncodedEH != null)
            {
                VideoEncodedEH(this, new VideoEventArgs() { Video = video });
            }
        }

        //Delegate
        public delegate void VideoFilterHandler(Video video);
        public void Filter(Video video, VideoFilterHandler filter)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("[Filter]:");
            filter(video);
        }

        //Action<>
        public void FilterAction(Video video, Action<Video> filter)
        {
            Console.WriteLine("[FilterAction]:");
            filter(video);
        }
    }

}
