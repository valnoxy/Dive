using Konsole;
using System;
using System.Diagnostics;
using System.Reflection;
using deployaCore.Common;

namespace deployaUI.Common
{
    public static class Debug
    {
        private static readonly FileVersionInfo _versionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly()!.Location);
        private static IConsole? _logConsole;
        private static IConsole? _actionConsole;

        private static ProgressBar? _prepareDiskProgressBar;
        private static ProgressBar? _applyImageProgressBar;
        private static ProgressBar? _installBootloaderProgressBar;
        private static ProgressBar? _installRecoveryProgressBar;
        private static ProgressBar? _installUnattendProgressBar;
        private static ProgressBar? _installDriversProgressBar;
        private static ProgressBar? _installUefiSevenProgressBar;
        private static ProgressBar? _captureDiskProgressBar;

        public static void WriteLine(string message, ConsoleColor color = ConsoleColor.White)
        {
            if (_logConsole == null)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("[");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("*");
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("] ");

                Console.ForegroundColor = color;
                Console.WriteLine(message);
                return;
            }

            _logConsole.ForegroundColor = ConsoleColor.DarkGray;
            _logConsole.Write("[");
            _logConsole.ForegroundColor = ConsoleColor.White;
            _logConsole.Write("*");
            _logConsole.ForegroundColor = ConsoleColor.DarkGray;
            _logConsole.Write("] ");

            _logConsole.ForegroundColor = color;
            _logConsole.WriteLine(message);
        }

        public static void Write(string message, bool continueLine = false, ConsoleColor color = ConsoleColor.White)
        {
            if (_logConsole == null)
            {
                if (!continueLine)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write("[");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write("*");
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write("] ");
                }

                Console.ForegroundColor = color;
                Console.Write(message);
                return;
            }

            if (!continueLine)
            {
                _logConsole.ForegroundColor = ConsoleColor.DarkGray;
                _logConsole.Write("[");
                _logConsole.ForegroundColor = ConsoleColor.White;
                _logConsole.Write("*");
                _logConsole.ForegroundColor = ConsoleColor.DarkGray;
                _logConsole.Write("] ");
            }

            _logConsole.ForegroundColor = color;
            _logConsole.Write(message);
        }

        public static void RefreshProgressBar(Progress progress, int value, string action)
        {
            ProgressBar progressBar;
            switch (progress)
            {
                case Progress.PrepareDisk:
                    _prepareDiskProgressBar ??= new ProgressBar(_actionConsole, PbStyle.SingleLine, 100);
                    progressBar = _prepareDiskProgressBar;
                    break;
                case Progress.ApplyImage:
                    _applyImageProgressBar ??= new ProgressBar(_actionConsole, PbStyle.SingleLine, 100);
                    progressBar = _applyImageProgressBar;
                    break;
                case Progress.InstallBootloader:
                    _installBootloaderProgressBar ??= new ProgressBar(_actionConsole, PbStyle.SingleLine, 100);
                    progressBar = _installBootloaderProgressBar;
                    break;
                case Progress.InstallRecovery:
                    _installRecoveryProgressBar ??= new ProgressBar(_actionConsole, PbStyle.SingleLine, 100);
                    progressBar = _installRecoveryProgressBar;
                    break;
                case Progress.InstallUnattend:
                    _installUnattendProgressBar ??= new ProgressBar(_actionConsole, PbStyle.SingleLine, 100);
                    progressBar = _installUnattendProgressBar;
                    break;
                case Progress.InstallDrivers:
                    _installDriversProgressBar ??= new ProgressBar(_actionConsole, PbStyle.SingleLine, 100);
                    progressBar = _installDriversProgressBar;
                    break;
                case Progress.InstallUefiSeven:
                    _installUefiSevenProgressBar ??= new ProgressBar(_actionConsole, PbStyle.SingleLine, 100);
                    progressBar = _installUefiSevenProgressBar;
                    break;
                case Progress.CaptureDisk:
                    _captureDiskProgressBar ??= new ProgressBar(_actionConsole, PbStyle.SingleLine, 100);
                    progressBar = _captureDiskProgressBar;
                    break;
                default:
                    return;
            }

            progressBar.Refresh(value, action);
        }

        public static void InitializeConsole()
        {
            Console.Title = $@"{_versionInfo.ProductName} - Debug Console";

            try
            {
                var window = new Window(Console.WindowWidth, Console.WindowHeight - 1);
                var consoles = window.SplitRows(
                    new Split(0, "Log", LineThickNess.Single),
                    new Split(8, "Action", LineThickNess.Single)
                );

                _logConsole = consoles[0];
                _actionConsole = consoles[1];
            }
            catch (System.ArgumentOutOfRangeException ex)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("[");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("*");
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("] ");

                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("Error: ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("The console window is too small. Please resize the window and try again.");
                Environment.Exit(1);
            }
            


            _logConsole.WriteLine($"{_versionInfo.ProductName} [Version: {_versionInfo.ProductVersion}]"); // Header
            _logConsole.WriteLine(_versionInfo.LegalCopyright + "\n"); // Copyright text
#if DEBUG
            Debug.Write("Warning! ", false, ConsoleColor.Red);
            Debug.Write("This is a Debug build. This is not a production ready version.\n", true);
#endif
        }
    }
}
