using System;
using System.Collections.Generic;

namespace Data
{
    public abstract class LogAbstract
    { 
        public abstract void Write(string msg, object obj);
        public abstract void Write(string msg, List<object> objects);

        public static LogAbstract New()
        {
            return new Log($"Balls {DateTime.Now:yyyy-MM-dd HH-mm-ss-ffff}");
        }
    }
}
