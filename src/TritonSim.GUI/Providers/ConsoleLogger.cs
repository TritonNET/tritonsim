using System;

namespace TritonSim.GUI.Providers
{
    public class ConsoleLogger : ILogger
    {
        private readonly object _lock = new object();

        public void Log(LogLevel level, string message, Exception? ex = null)
        {
            lock (_lock)
            {
                var originalColor = Console.ForegroundColor;
                Console.ForegroundColor = GetColorForLevel(level);

                string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");

                Console.WriteLine($"[{timestamp}] [{level.ToString().ToUpper()}] {message}");

                if (ex != null)
                {
                    Console.WriteLine($"\tException: {ex.Message}");
                    Console.WriteLine($"\tStack Trace: {ex.StackTrace}");
                }

                Console.ForegroundColor = originalColor;
            }
        }

        private ConsoleColor GetColorForLevel(LogLevel level)
        {
            return level switch
            {
                LogLevel.Debug => ConsoleColor.Gray,
                LogLevel.Info => ConsoleColor.White,
                LogLevel.Warning => ConsoleColor.Yellow,
                LogLevel.Error => ConsoleColor.Red,
                LogLevel.Fatal => ConsoleColor.DarkRed,
                _ => ConsoleColor.White
            };
        }

        public void Debug(string message) => Log(LogLevel.Debug, message);

        public void Info(string message) => Log(LogLevel.Info, message);

        public void Warning(string message) => Log(LogLevel.Warning, message);

        public void Error(string message, Exception? ex = null) => Log(LogLevel.Error, message, ex);

        public void Fatal(string message, Exception? ex = null) => Log(LogLevel.Fatal, message, ex);
    }
}
