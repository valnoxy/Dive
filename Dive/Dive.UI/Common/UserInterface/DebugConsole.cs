using Dive.Core.Action.DiskPreparation;
using Dive.Core.Common;
using Dive.UI.Common.Diagnostics;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using LogLevel = Dive.UI.Common.Diagnostics.LogLevel;
using Progress = Dive.Core.Common.Progress;

namespace Dive.UI.Common.UserInterface
{
    /// <summary>
    /// Compatibility facade over <see cref="Log"/>.
    /// Fragments written with <see cref="Write"/> are buffered until a line break
    /// and then emitted as a single log entry.
    /// </summary>
    public static class Debug
    {
        private static readonly FileVersionInfo VersionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly()!.Location);
        private static readonly object SyncRoot = new();
        private static readonly List<LogSegment> PendingSegments = new();
        private static readonly Dictionary<Progress, (int Step, string Action)> ProgressState = new();
        private static bool _initialized;

        static Debug()
        {
            // Ensure output works even if InitializeConsole() was never called.
            EnsureConsoleSink();
        }

        public static void WriteLine(string message, ConsoleColor color = ConsoleColor.White)
        {
            lock (SyncRoot)
            {
                FlushPending();
                Log.Write(new LogEntry(DateTime.Now, LevelFromColor(color), new[] { new LogSegment(message.TrimEnd('\r', '\n'), color) }));
            }
        }

        public static void Write(string message, bool continueLine = false, ConsoleColor color = ConsoleColor.White)
        {
            lock (SyncRoot)
            {
                if (!continueLine) FlushPending();

                var parts = message.Replace("\r", string.Empty).Split('\n');
                for (var i = 0; i < parts.Length; i++)
                {
                    if (parts[i].Length > 0) PendingSegments.Add(new LogSegment(parts[i], color));
                    if (i < parts.Length - 1) FlushPending();
                }
            }
        }

        /// <summary>
        /// Reports the status of a running process. Only logged when the action changes
        /// or the progress crosses another 10% step to keep the log readable.
        /// </summary>
        public static void RefreshProgressBar(Progress progress, int value, string action)
        {
            var step = Math.Clamp(value, 0, 100) / 10;
            lock (SyncRoot)
            {
                if (ProgressState.TryGetValue(progress, out var last) && last.Step == step && last.Action == action)
                    return;

                ProgressState[progress] = (step, action);
                FlushPending();
                Log.Write(new LogEntry(DateTime.Now, LogLevel.Info, new[]
                {
                    new LogSegment($"[{progress}] "),
                    new LogSegment($"{value,3}%", ConsoleColor.DarkYellow),
                    new LogSegment(string.IsNullOrWhiteSpace(action) ? string.Empty : $" - {action}")
                }));
            }
        }

        public static void InitializeConsole()
        {
            lock (SyncRoot)
            {
                if (_initialized) return;
                _initialized = true;
            }

            try
            {
                Console.Title = $"{VersionInfo.ProductName} - Debug Console";
            }
            catch
            {
                // No console attached
            }

            AnsiConsole.Write(new Rule($"[bold]{Markup.Escape(VersionInfo.ProductName ?? "Dive")}[/] [grey]{Markup.Escape(VersionInfo.ProductVersion ?? string.Empty)}[/]").LeftJustified());
            AnsiConsole.MarkupLine($"[grey]{Markup.Escape(VersionInfo.LegalCopyright ?? string.Empty)}[/]");
            AnsiConsole.WriteLine();

            // Route the output of the native DiskMgr library into the log.
            DiskMgrLog.MessageLogged += OnDiskMgrMessage;
#if DEBUG
            Log.Warning("This is a Debug build. This is not a production ready version.");
            Logging.Log("This is a Debug build. This is not a production ready version.", Logging.LogLevel.WARNING);
#endif
        }

        private static void OnDiskMgrMessage(DiskMgrLog.Level level, string message)
        {
            var logLevel = level switch
            {
                DiskMgrLog.Level.Debug => LogLevel.Debug,
                DiskMgrLog.Level.Warning => LogLevel.Warning,
                DiskMgrLog.Level.Error => LogLevel.Error,
                _ => LogLevel.Info
            };

            Log.Write(new LogEntry(DateTime.Now, logLevel, new[]
            {
                new LogSegment("[DiskMgr] ", ConsoleColor.DarkGray),
                new LogSegment(message)
            }));
        }

        private static ConsoleLogSink? _consoleSink;

        private static void EnsureConsoleSink()
        {
            if (_consoleSink != null) return;
            _consoleSink = new ConsoleLogSink();
            Log.AddSink(_consoleSink);
        }

        private static void FlushPending()
        {
            if (PendingSegments.Count == 0) return;

            var level = LogLevel.Info;
            foreach (var s in PendingSegments)
            {
                var l = LevelFromColor(s.Color ?? ConsoleColor.White);
                if (l > level) level = l;
            }

            Log.Write(new LogEntry(DateTime.Now, level, PendingSegments.ToArray()));
            PendingSegments.Clear();
        }

        private static LogLevel LevelFromColor(ConsoleColor color) => color switch
        {
            ConsoleColor.Red or ConsoleColor.DarkRed => LogLevel.Error,
            ConsoleColor.Yellow => LogLevel.Warning,
            ConsoleColor.DarkGray => LogLevel.Debug,
            _ => LogLevel.Info
        };
    }
}
