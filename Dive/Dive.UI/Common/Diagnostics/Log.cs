using System;
using System.Collections.Generic;
using System.Linq;

namespace Dive.UI.Common.Diagnostics
{
    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error
    }

    /// <summary>
    /// A piece of a log message. <see cref="Color"/> is an optional highlight
    /// that only visual sinks (e.g. the console) make use of.
    /// </summary>
    public sealed record LogSegment(string Text, ConsoleColor? Color = null);

    public sealed record LogEntry(DateTime Timestamp, LogLevel Level, IReadOnlyList<LogSegment> Segments)
    {
        public string Message => string.Concat(Segments.Select(s => s.Text));
    }

    /// <summary>
    /// Output target for log entries (console, file, ...).
    /// </summary>
    public interface ILogSink
    {
        void Write(LogEntry entry);
    }

    /// <summary>
    /// Simple line-based logger which forwards every entry to all registered sinks.
    /// </summary>
    public static class Log
    {
        private static readonly object SyncRoot = new();
        private static readonly List<ILogSink> Sinks = new();

        public static LogLevel MinimumLevel { get; set; } = LogLevel.Debug;

        public static void AddSink(ILogSink sink)
        {
            lock (SyncRoot) Sinks.Add(sink);
        }

        public static void RemoveSink(ILogSink sink)
        {
            lock (SyncRoot) Sinks.Remove(sink);
        }

        public static void Debug(string message) => Write(LogLevel.Debug, message);
        public static void Info(string message) => Write(LogLevel.Info, message);
        public static void Warning(string message) => Write(LogLevel.Warning, message);
        public static void Error(string message) => Write(LogLevel.Error, message);

        public static void Write(LogLevel level, string message) =>
            Write(new LogEntry(DateTime.Now, level, new[] { new LogSegment(message) }));

        public static void Write(LogEntry entry)
        {
            if (entry.Level < MinimumLevel) return;

            lock (SyncRoot)
            {
                foreach (var sink in Sinks)
                {
                    try
                    {
                        sink.Write(entry);
                    }
                    catch
                    {
                        // empty
                    }
                }
            }
        }
    }
}
