using Konsole;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;

namespace deployaUI.Common
{
    public static class Debug
    {
        public static FileVersionInfo VersionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly()!.Location);
        public static IConsole? LogConsole;
        public static IConsole? ActionConsole;

        public static ProgressBar? PrepareDiskProgressBar;
        public static ProgressBar? ApplyImageProgressBar;
        public static ProgressBar? InstallBootloaderProgressBar;
        public static ProgressBar? InstallRecoveryProgressBar;
        public static ProgressBar? InstallUnattendProgressBar;
        public static ProgressBar? InstallDriversProgressBar;
        public static ProgressBar? InstallUefiSevenProgressBar;

        public enum Progress
        {
            PrepareDisk,
            ApplyImage,
            InstallBootloader,
            InstallRecovery,
            InstallUnattend,
            InstallDrivers,
            InstallUefiSeven
        }

        public static void WriteLine(string message, ConsoleColor color = ConsoleColor.White)
        {
            if (LogConsole == null) return;
            LogConsole.ForegroundColor = ConsoleColor.DarkGray;
            LogConsole.Write("[");
            LogConsole.ForegroundColor = ConsoleColor.White;
            LogConsole.Write("*");
            LogConsole.ForegroundColor = ConsoleColor.DarkGray;
            LogConsole.Write("] ");

            LogConsole.ForegroundColor = color;
            LogConsole.WriteLine(message);
        }

        public static void Write(string message, bool continueLine = false, ConsoleColor color = ConsoleColor.White)
        {
            if (LogConsole == null) return;

            if (!continueLine)
            {
                LogConsole.ForegroundColor = ConsoleColor.DarkGray;
                LogConsole.Write("[");
                LogConsole.ForegroundColor = ConsoleColor.White;
                LogConsole.Write("*");
                LogConsole.ForegroundColor = ConsoleColor.DarkGray;
                LogConsole.Write("] ");
            }

            LogConsole.ForegroundColor = color;
            LogConsole.Write(message);
        }

        public static void TestWriteAction()
        {
            if (ActionConsole == null) return;

            var pb = new ProgressBar(ActionConsole, PbStyle.SingleLine, 50);
            pb.Refresh(0, "connecting to server to download 5 files asychronously.");
            Thread.Sleep(1000);
            pb.Refresh(25, "downloading file number 25");
            Thread.Sleep(1000);
            pb.Refresh(50, "finished.");
            var pb1 = new ProgressBar(ActionConsole, PbStyle.SingleLine, 50);
            pb1.Refresh(0, "connecting to server to download 5 files asychronously.");
            Thread.Sleep(1000);
            pb1.Refresh(25, "downloading file number 25");
            Thread.Sleep(1000);
            pb1.Refresh(50, "finished.");
            var pb2 = new ProgressBar(ActionConsole, PbStyle.SingleLine, 50);
            pb2.Refresh(0, "connecting to server to download 5 files asychronously.");
            Thread.Sleep(1000);
            pb2.Refresh(25, "downloading file number 25");
            Thread.Sleep(1000);
            pb2.Refresh(50, "finished.");
            var pb3 = new ProgressBar(ActionConsole, PbStyle.SingleLine, 50);
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
                    PrepareDiskProgressBar ??= new ProgressBar(ActionConsole, PbStyle.SingleLine, 100);
                    progressBar = PrepareDiskProgressBar;
                    break;
                case Progress.ApplyImage:
                    ApplyImageProgressBar ??= new ProgressBar(ActionConsole, PbStyle.SingleLine, 100);
                    progressBar = ApplyImageProgressBar;
                    break;
                case Progress.InstallBootloader:
                    InstallBootloaderProgressBar ??= new ProgressBar(ActionConsole, PbStyle.SingleLine, 100);
                    progressBar = InstallBootloaderProgressBar;
                    break;
                case Progress.InstallRecovery:
                    InstallRecoveryProgressBar ??= new ProgressBar(ActionConsole, PbStyle.SingleLine, 100);
                    progressBar = InstallRecoveryProgressBar;
                    break;
                case Progress.InstallUnattend:
                    InstallUnattendProgressBar ??= new ProgressBar(ActionConsole, PbStyle.SingleLine, 100);
                    progressBar = InstallUnattendProgressBar;
                    break;
                case Progress.InstallDrivers:
                    InstallDriversProgressBar ??= new ProgressBar(ActionConsole, PbStyle.SingleLine, 100);
                    progressBar = InstallDriversProgressBar;
                    break;
                case Progress.InstallUefiSeven:
                    InstallUefiSevenProgressBar ??= new ProgressBar(ActionConsole, PbStyle.SingleLine, 100);
                    progressBar = InstallUefiSevenProgressBar;
                    break;
                default:
                    return;
            }

            progressBar.Refresh(value, action);
        }

        public static void InitializeConsole()
        {
            Console.Title = $@"{VersionInfo.ProductName} - Debug Console";

            // create an 80 by 20 inline window
            var window = new Window(Console.WindowWidth, Console.WindowHeight -1);
            var consoles = window.SplitRows(
                new Split(0, "Log", LineThickNess.Single),
                new Split(8, "Action", LineThickNess.Single)
            );

            LogConsole = consoles[0];
            ActionConsole = consoles[1];

            LogConsole.WriteLine($"{VersionInfo.ProductName} [Version: {VersionInfo.ProductVersion}]"); // Header
            LogConsole.WriteLine(VersionInfo.LegalCopyright + "\n"); // Copyright text
        }
    }
}
