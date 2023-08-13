using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DSRemapper.DSLogger
{
    /// <summary>
    /// Enumeration of log types for the DSRemapper Logger
    /// </summary>
    public enum LogLevel { 
        /// <summary>
        /// Mark the log entry as a message
        /// </summary>
        Message,
        /// <summary>
        /// Mark the log entry as a warning
        /// </summary>
        Warning,
        /// <summary>
        /// Mark the log entry as an error
        /// </summary>
        Error
    }
    /// <summary>
    /// Logger class of DSRemapper, used for build in console log
    /// </summary>
    public static class Logger
    {
        /// <summary>
        /// Default delegate for the log event
        /// </summary>
        /// <param name="log"></param>
        public delegate void LoggerEvent(LogEntry log);
        /// <summary>
        /// Occurs when a log function is called
        /// </summary>
        public static event LoggerEvent? OnLog;
        public static int Subcribers => OnLog?.GetInvocationList().Length ?? 0;
        /// <summary>
        /// Structure of a log entry
        /// </summary>
        public struct LogEntry
        {
            /// <summary>
            /// Level/type of the log
            /// </summary>
            public LogLevel Level;
            /// <summary>
            /// Content of the log entry
            /// </summary>
            public string Message;

            /// <summary>
            /// LogEntry struct constructor
            /// </summary>
            /// <param name="message">String containing the log content</param>
            /// <param name="level">Level/type of the log entry</param>
            public LogEntry(string message, LogLevel level = LogLevel.Message)
            {
                Message = message;
                Level = level;
            }
            /// <summary>
            /// XML/HTML to string conversion for the structure
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                return $"<{Level}>{Message}</{Level}>";
            }
        }

        /// <summary>
        /// List containing all the log of the app. Used to restore old log entries on the console.
        /// There has to be a better way, but until I implement it, this works.
        /// </summary>
        public static List<LogEntry> logs = new();

        /// <summary>
        /// Logs a message to the DSRemapper Logger
        /// </summary>
        /// <param name="message">Content of the log</param>
        public static void Log(string message)
        {
            LogEntry entry = new(message);
            logs.Add(entry);
            OnLog?.Invoke(entry);
        }
        /// <summary>
        /// Logs a warning to the DSRemapper Logger
        /// </summary>
        /// <param name="message">Content of the log</param>
        public static void LogWarning(string message)
        {
            LogEntry entry = new(message, LogLevel.Warning);
            logs.Add(entry);
            OnLog?.Invoke(entry);
        }
        /// <summary>
        /// Logs a error to the DSRemapper Logger
        /// </summary>
        /// <param name="message">Content of the log</param>
        public static void LogError(string message)
        {
            LogEntry entry = new(message, LogLevel.Error);
            logs.Add(entry);
            OnLog?.Invoke(entry);
        }
        /// <summary>
        /// Prints all the stored log on the windows console (if it is visible)
        /// </summary>
        public static void PrintLogOnConsole()
        {
            logs.ForEach(l => Console.WriteLine($"{l.Level}: {l.Message}"));
        }
        /// <summary>
        /// Gets all the logs as a XML string array
        /// </summary>
        /// <returns>An string array containing all the logs in the XML form</returns>
        public static string[] GetXmlLog() => logs.Select(l => l.ToString()).ToArray();
    }
}
