using System;

namespace TritonSim.GUI.Providers
{
    public class PreviewLogger : ILogger
    {
        private void WriteToOutput(string level, string message, Exception? ex = null)
        {
            // Format: [HH:mm:ss] [LEVEL] Message
            string logEntry = $"[{DateTime.Now:HH:mm:ss}] [{level}] {message}";

            if (ex != null)
            {
                logEntry += $"{Environment.NewLine}Exception: {ex}";
            }

            System.Diagnostics.Debug.WriteLine(logEntry); // This writes to the VS Output Window
        }

        public void Debug(string message)
        {
            WriteToOutput("DEBUG", message);
        }

        public void Info(string message)
        {
            WriteToOutput("INFO", message);
        }

        public void Warning(string message)
        {
            WriteToOutput("WARN", message);
        }

        public void Error(string message, Exception? ex = null)
        {
            WriteToOutput("ERROR", message, ex);
        }

        public void Fatal(string message, Exception? ex = null)
        {
            WriteToOutput("FATAL", message, ex);
        }

        public void Log(LogLevel level, string message, Exception? ex = null)
        {
            // Converts the enum (e.g., LogLevel.Info) to a string "INFO"
            WriteToOutput(level.ToString().ToUpper(), message, ex);
        }
    }
}
