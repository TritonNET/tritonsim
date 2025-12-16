using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TritonSim.GUI.Providers
{
    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error,
        Fatal
    }

    public interface ILogger
    {
        void Log(LogLevel level, string message, Exception? ex = null);

        void Debug(string message);
        void Info(string message);
        void Warning(string message);
        void Error(string message, Exception? ex = null);
        void Fatal(string message, Exception? ex = null);
    }
}
