using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Data
{
    internal class Log : LogAbstract
    {
        private string filename;
        private readonly Queue<LogText> messages;
        private readonly Mutex mutex = new Mutex();
        private StreamWriter sr;
        private Task task;

        public Log(string filename)
        {
            this.filename = filename + ".log";
            messages = new Queue<LogText>();
            task = Run(50, CancellationToken.None, sr);
        }

        private Queue<LogText> GetLogs()
        {
            mutex.WaitOne();
            var logs = new Queue<LogText>(messages);
            messages.Clear();
            mutex.ReleaseMutex();
            return logs;
        }

        private async Task Run(int interval, CancellationToken cancellationToken, StreamWriter sr)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var logs = GetLogs();
                while (logs.Count > 0)
                {
                    File.AppendAllText(filename, logs.Dequeue().SaveToString() + "\n");
                }

                await Task.Delay((int)(interval), cancellationToken);
            }
        }

        public override void Write(string msg, object obj)
        {
            mutex.WaitOne();
            messages.Enqueue(new LogText($"{DateTime.Now:yyyy-MM-dd HH-mm-ss-ffff}", msg, obj));
            mutex.ReleaseMutex();
        }

        public override void Write(string msg, List<object> objects)
        {
            mutex.WaitOne();
            messages.Enqueue(new LogText($"{DateTime.Now:yyyy-MM-dd HH-mm-ss-ffff}", msg, objects));
            mutex.ReleaseMutex();
        }
    }
    internal class LogText
    {
        // order of these varible matters for serialization
        public string Time;
        public string Text;
        public object Obj;

        public LogText(string text, string time, object obj)
        {
            Text = text;
            Time = time;
            Obj = obj;
        }

        public string SaveToString()
        {
            // here we serialize the LogText object which contains all data about the log we want to save
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }
    }
}
