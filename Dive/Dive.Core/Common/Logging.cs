﻿using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Dive.Core.Common
{
    public class Logging
    {
        private static readonly string LogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
        private static string? _logFile;
        private const bool OutputToDebugConsole = false;

        public enum LogLevel { INFO, ERROR, WARNING, DEBUG }

        public static void Initialize()
        {
            // Ensure the log directory exists
            if (!Directory.Exists(LogPath))
            {
                Directory.CreateDirectory(LogPath);
            }

            // Fetching the current log file
            _logFile = Path.Combine(LogPath, $"log_{DateTime.Now:yyyy-MM-dd}.txt");

            var version = Assembly.GetExecutingAssembly().GetName().Version!;
            Log($"Dive Core {version.Major}.{version.Minor}.{version.Build} (Build {version.Revision})");
            Log("Logging system initialized");

            // Delete old log files
            DeleteOldLogFiles();
        }

        public static void Log(string message, LogLevel level = LogLevel.INFO)
        {
            if (string.IsNullOrEmpty(_logFile))
                throw new Exception("Logging class not initialized!");

            // Get calling method information
            var stackTrace = new StackTrace();
            var callingMethod = stackTrace.GetFrame(1)?.GetMethod();
            var callingClass = callingMethod?.DeclaringType?.FullName ?? "UnknownClass";
            var methodName = callingMethod?.Name ?? "UnknownMethod";

            // Construct the log message with calling class and method
            var logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {level} - {callingClass}.{methodName} - {message}";

            using (var fileStream = new FileStream(_logFile, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
            using (var writer = new StreamWriter(fileStream))
            {
                writer.WriteLine(logMessage);
            }

            // Output log message to Debug console
            if (OutputToDebugConsole) System.Diagnostics.Debug.WriteLine(logMessage);
        }

        private static void DeleteOldLogFiles()
        {
            var logFiles = Directory.GetFiles(LogPath, "log_*.txt");
            foreach (var logFile in logFiles)
            {
                var creationTime = File.GetCreationTime(logFile);
                if (creationTime < DateTime.Now.AddDays(-7))
                {
                    File.Delete(logFile);
                    Log("Deleted old log file: " + logFile);
                }
            }
        }
    }
}