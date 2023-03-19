using Konsole;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
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
            if (_logConsole == null) return;
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
            if (_logConsole == null) return;

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

        public static void TestWriteAction()
        {
            if (_actionConsole == null) return;

            var pb = new ProgressBar(_actionConsole, PbStyle.SingleLine, 50);
            pb.Refresh(0, "connecting to server to download 5 files asychronously.");
            Thread.Sleep(1000);
            pb.Refresh(25, "downloading file number 25");
            Thread.Sleep(1000);
            pb.Refresh(50, "finished.");
            var pb1 = new ProgressBar(_actionConsole, PbStyle.SingleLine, 50);
            pb1.Refresh(0, "connecting to server to download 5 files asychronously.");
            Thread.Sleep(1000);
            pb1.Refresh(25, "downloading file number 25");
            Thread.Sleep(1000);
            pb1.Refresh(50, "finished.");
            var pb2 = new ProgressBar(_actionConsole, PbStyle.SingleLine, 50);
            pb2.Refresh(0, "connecting to server to download 5 files asychronously.");
            Thread.Sleep(1000);
            pb2.Refresh(25, "downloading file number 25");
            Thread.Sleep(1000);
            pb2.Refresh(50, "finished.");
            var pb3 = new ProgressBar(_actionConsole, PbStyle.SingleLine, 50);
            pb3.Refresh(0, "connecting to server to download 5 files asychronously.");
            Thread.Sleep(1000);
            pb3.Refresh(25, "downloading file number 25");
            Thread.Sleep(1000);
            pb3.Refresh(50, "finished.");
        }

        public static void RefreshProgressBar(Progress progress, int value, string action)
        {
            ProgressBar progressBar = null!;
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

            // create an 80 by 20 inline window
            var window = new Window(Console.WindowWidth, Console.WindowHeight -1);
            var consoles = window.SplitRows(
                new Split(0, "Log", LineThickNess.Single),
                new Split(8, "Action", LineThickNess.Single)
            );

            _logConsole = consoles[0];
            _actionConsole = consoles[1];

            _logConsole.WriteLine($"{_versionInfo.ProductName} [Version: {_versionInfo.ProductVersion}]"); // Header
            _logConsole.WriteLine(_versionInfo.LegalCopyright + "\n"); // Copyright text
#if DEBUG
            Debug.Write("Warning! ", false, ConsoleColor.Red);
            Debug.Write("This is a Debug build. This is not a production ready version.\n", true);
#endif
        }
    }
}
