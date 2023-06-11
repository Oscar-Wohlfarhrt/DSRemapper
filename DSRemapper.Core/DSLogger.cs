using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DSRemapper.DSLogger
{
    public enum LogLevel { Message, Warning, Error }
    public static class Logger
    {
        public delegate void LoggerEvent(LogEntry log);
        public static event LoggerEvent OnLog;
        public struct LogEntry
        {
            public LogLevel Level;
            public string Message;

            public LogEntry(string message, LogLevel level = LogLevel.Message)
            {
                Message = message;
                Level = level;
            }

            public override string ToString()
            {
                return $"<{Level}>{Message}</{Level}>";
            }
        }

        public static List<LogEntry> logs = new();

        public static int Subcribers => OnLog.GetInvocationList().Length;

        public static void Log(string message)
        {
            LogEntry entry = new(message);
            logs.Add(entry);
            OnLog?.Invoke(entry);
        }
        public static void LogWarning(string message)
        {
            LogEntry entry = new(message, LogLevel.Warning);
            logs.Add(entry);
            OnLog?.Invoke(entry);
        }
        public static void LogError(string message)
        {
            LogEntry entry = new(message, LogLevel.Error);
            logs.Add(entry);
            OnLog?.Invoke(entry);
        }
        public static void PrintLogOnConsole()
        {
            logs.ForEach(l => Console.WriteLine($"{l.Level}: {l.Message}"));
        }
        public static string[] GetXmlLog() => logs.Select(l => l.ToString()).ToArray();
    }
}
