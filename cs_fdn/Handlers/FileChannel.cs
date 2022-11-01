using System;
using System.IO;

namespace cs_fdn
{
    public class FileChannel : INotificationChannel
    {
        private readonly string _path;

        public FileChannel(string path)
        {
            this._path = path;
        }
        public void Send(string message)
        {
            Console.WriteLine("FileChannel: send to file - " + message);

            using (var streamWriter = new StreamWriter(_path, append: true))
            {
                streamWriter.Write("FileChannel: send to file - " + message);
            }
        }
    }

}
