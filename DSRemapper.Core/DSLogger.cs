using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSRemapper.DSLogger
{
    public enum LogLevel { Message, Warning, Error }
    public static class Logger
    { 
        public struct LogEntry
        {
            public LogLevel Level;
            public string Message;

            public LogEntry(string message,LogLevel level=LogLevel.Message)
            {
                Message = message;
                Level = level;
            }

            public override string ToString()
            {
                return $"<{Level}>{Message}</{Level}>";
            }
        }

        public static List<LogEntry> logs = new List<LogEntry>();

        public static void Log(string message)
        {
            logs.Add(new LogEntry(message));
        }
        public static void LogWarning(string message)
        {
            logs.Add(new LogEntry(message, LogLevel.Warning));
        }
        public static void LogError(string message)
        {
            logs.Add(new LogEntry(message,LogLevel.Error));
        }
        public static void PrintLogOnConsole()
        {
            logs.ForEach(l => Console.WriteLine($"{l.Level}: {l.Message}"));
        }
        public static string[] GetXmlLog() => logs.Select(l => l.ToString()).ToArray();
    }
}
