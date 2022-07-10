/* 
 * deploya - Fast and Easy way to deploy Windows
 * Copyright (c) 2018 - 2022 Exploitox.
 * 
 * deploya is licensed under MIT License (https://github.com/valnoxy/deploya/blob/main/LICENSE). 
 * So you are allowed to use freely and modify the application. 
 * I will not be responsible for any outcome. 
 * Proceed with any action at your own risk.
 * 
 * Source code: https://github.com/valnoxy/deploya
 */

using Microsoft.Wim;
using System;
using System.ComponentModel;
using System.Diagnostics;

namespace deploya_core
{
    public class Entities
    {
        public enum Firmware
        {
            BIOS,
            EFI,
        }

        public enum Bootloader
        {
            BOOTMGR,
            NTLDR,
        }

        public enum UI
        {
            Graphical,
            Command,
        }
    }

    public static class Output
    {
        public static void WriteLine(string message, ConsoleColor color = ConsoleColor.White)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("[");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("*");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("] ");

            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        public static void Write(string message, ConsoleColor color = ConsoleColor.White)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("[");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("*");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("] ");

            Console.ForegroundColor = color;
            Console.Write(message);
            Console.ResetColor();
        }
    }

    public class Actions
    {
        public static BackgroundWorker progBar = null;

        public static void PrepareDisk(Entities.Firmware firmware, Entities.Bootloader bootloader, Entities.UI ui, int disk, BackgroundWorker worker = null)
        {
            Output.Write("Partitioning disk ...         ");
            ConsoleUtility.WriteProgressBar(0);
            if (ui == Entities.UI.Graphical) { worker.ReportProgress(102, ""); }

            Process partDest = new Process();
            partDest.StartInfo.FileName = "diskpart.exe";
            partDest.StartInfo.UseShellExecute = false;
            partDest.StartInfo.CreateNoWindow = true;
            partDest.StartInfo.RedirectStandardInput = true;
            partDest.StartInfo.RedirectStandardOutput = true;
            partDest.Start();

            if (firmware == Entities.Firmware.BIOS)
            {
                if (bootloader == Entities.Bootloader.NTLDR)
                {
                    partDest.StandardInput.WriteLine("select disk " + disk);
                    partDest.StandardInput.WriteLine("clean");
                    partDest.StandardInput.WriteLine("create partition primary");
                    partDest.StandardInput.WriteLine("format quick fs=ntfs label=Windows");
                    partDest.StandardInput.WriteLine("active");
                    partDest.StandardInput.WriteLine("assign letter=W");
                    partDest.StandardInput.WriteLine("exit");
                    partDest.WaitForExit();
                }
                if (bootloader == Entities.Bootloader.BOOTMGR)
                {
                    partDest.StandardInput.WriteLine("select disk " + disk);
                    partDest.StandardInput.WriteLine("clean");
                    partDest.StandardInput.WriteLine("create partition primary size=100");
                    partDest.StandardInput.WriteLine("format quick fs=ntfs label=System");
                    partDest.StandardInput.WriteLine("assign letter=S");
                    partDest.StandardInput.WriteLine("active");
                    partDest.StandardInput.WriteLine("create partition primary");
                    partDest.StandardInput.WriteLine("shrink minimum=650");
                    partDest.StandardInput.WriteLine("format quick fs=ntfs label=Windows");
                    partDest.StandardInput.WriteLine("assign letter=W");
                    partDest.StandardInput.WriteLine("create partition primary");
                    partDest.StandardInput.WriteLine("format quick fs=ntfs label=Recovery");
                    partDest.StandardInput.WriteLine("assign letter=R");
                    partDest.StandardInput.WriteLine("set id=27");
                    partDest.StandardInput.WriteLine("exit");
                    partDest.WaitForExit();
                }
            }

            if (firmware == Entities.Firmware.EFI)
            {
                if (bootloader == Entities.Bootloader.NTLDR)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("");
                    Console.WriteLine("   An Error has occurred.");
                    Console.WriteLine("   Error: You cannot use NTLDR as bootloader on EFI.");
                    if (ui == Entities.UI.Command)
                        Console.WriteLine(); // Only write new line if ui mode is disabled, so that the ui can read the error code above.
                    Console.ResetColor();
                    if (ui == Entities.UI.Graphical) { worker.ReportProgress(301, ""); }
                    return;
                }

                partDest.StandardInput.WriteLine("select disk " + disk);
                partDest.StandardInput.WriteLine("clean");
                partDest.StandardInput.WriteLine("convert gpt");
                partDest.StandardInput.WriteLine("create partition efi size=100");
                partDest.StandardInput.WriteLine("format quick fs=fat32 label=System");
                partDest.StandardInput.WriteLine("assign letter=S");
                partDest.StandardInput.WriteLine("create partition msr size=16");
                partDest.StandardInput.WriteLine("create partition primary");
                partDest.StandardInput.WriteLine("shrink minimum=650");
                partDest.StandardInput.WriteLine("format quick fs=ntfs label=Windows");
                partDest.StandardInput.WriteLine("assign letter=W");
                partDest.StandardInput.WriteLine("create partition primary");
                partDest.StandardInput.WriteLine("format quick fs=ntfs label=Recovery");
                partDest.StandardInput.WriteLine("assign letter=R");
                partDest.StandardInput.WriteLine("set id=de94bba4-06d1-4d40-a16a-bfd50179d6ac");
                partDest.StandardInput.WriteLine("gpt attributes=0x8000000000000001");
                partDest.StandardInput.WriteLine("exit");
                partDest.WaitForExit();                    
            }

            if (partDest.ExitCode != 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("");
                Console.WriteLine("   An Error has occurred.");
                Console.WriteLine("   Error: " + partDest.ExitCode.ToString());
                if (ui == Entities.UI.Command)
                    Console.WriteLine(); // Only write new line if ui mode is disabled, so that the ui can read the error code above.
                Console.ResetColor();
                if (ui == Entities.UI.Graphical) { worker.ReportProgress(301, ""); }
                return;
            }
            
            ConsoleUtility.WriteProgressBar(100, true);
            Console.WriteLine();
            if (ui == Entities.UI.Graphical) { worker.ReportProgress(101, ""); worker.ReportProgress(100, ""); }
        }

        public static void ApplyWIM(Entities.UI ui, string path, string wimfile, int index, BackgroundWorker worker = null)
        {
            Output.Write("Applying Image ...            ");
            ConsoleUtility.WriteProgressBar(0);
            if (ui == Entities.UI.Graphical) { worker.ReportProgress(101, ""); worker.ReportProgress(0, ""); }
            
            Apply.WriteToDisk(wimfile, index, path, worker);
            Console.WriteLine();
        }

        public static void InstallBootloader(Entities.Firmware firmware, Entities.Bootloader bootloader, Entities.UI ui, string WindowsPath, string BootloaderLetter, BackgroundWorker worker = null)
        {
            Output.Write("Installing Bootloader ...     ");
            ConsoleUtility.WriteProgressBar(0);
            if (ui == Entities.UI.Graphical) { worker.ReportProgress(102, ""); worker.ReportProgress(0, ""); }

            Process bootld = new Process();

            #region Legacy check
            if (bootloader == Entities.Bootloader.NTLDR)
            {
                string StrBl = BootloaderLetter.Substring(0, 2);
                bootld.StartInfo.FileName = "bootsect.exe";
                bootld.StartInfo.Arguments = $"/nt52 {StrBl} /force /mbr";
            }
            #endregion

            #region BIOS / EFI check
            if (bootloader == Entities.Bootloader.BOOTMGR)
            {
                bootld.StartInfo.FileName = "bcdboot.exe";

                if (firmware == Entities.Firmware.BIOS) // BIOS
                    bootld.StartInfo.Arguments = $"{WindowsPath} /s {BootloaderLetter} /f BIOS";

                if (firmware == Entities.Firmware.EFI) // EFI
                    bootld.StartInfo.Arguments = $"{WindowsPath} /s {BootloaderLetter} /f UEFI";
            }
            #endregion

            bootld.StartInfo.UseShellExecute = false;
            bootld.StartInfo.RedirectStandardOutput = true;
            bootld.StartInfo.CreateNoWindow = true;
            bootld.Start();
            bootld.WaitForExit();

            if (bootld.ExitCode != 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("");
                Console.WriteLine("   An Error has occurred.");
                Console.WriteLine("   Error: " + bootld.ExitCode.ToString());
                if (ui == Entities.UI.Command)
                    Console.WriteLine(); // Only write new line if ui mode is disabled, so that the ui can read the error code above.
                Console.ResetColor();
                if (ui == Entities.UI.Graphical) { worker.ReportProgress(303, ""); }
                if (ui == Entities.UI.Command) { Environment.Exit(bootld.ExitCode); }
            }

            ConsoleUtility.WriteProgressBar(100, true);
            Console.WriteLine();
            if (ui == Entities.UI.Graphical) { worker.ReportProgress(101, ""); worker.ReportProgress(100, ""); }
        }

        public static void InstallRecovery(Entities.UI ui, string WindowsPath, string RecoveryLetter, BackgroundWorker worker = null)
        {
            Output.Write("Installing Recovery ...       ");
            ConsoleUtility.WriteProgressBar(0);
            if (ui == Entities.UI.Graphical) { worker.ReportProgress(102, ""); worker.ReportProgress(0, ""); }

            // Create Recovery directory
            try
            {
                System.IO.Directory.CreateDirectory(System.IO.Path.Combine(RecoveryLetter, "Recovery", "WindowsRE"));
            }
            catch 
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("");
                Console.WriteLine("   An Error has occurred.");
                Console.WriteLine("   Error: Cannot create recovery directory.");
                if (ui == Entities.UI.Command)
                    Console.WriteLine(); // Only write new line if ui mode is disabled, so that the ui can read the error code above.
                Console.ResetColor();
                if (ui == Entities.UI.Graphical) { worker.ReportProgress(304, ""); }
                if (ui == Entities.UI.Command) { Environment.Exit(1); }
            }

            // Copy WinRE image to Recovery partition
            try
            {
                if (System.IO.File.Exists(System.IO.Path.Combine(WindowsPath, "System32", "Recovery", "Winre.wim")))
                {
                    System.IO.File.Copy(
                        System.IO.Path.Combine(WindowsPath, "System32", "Recovery", "Winre.wim"),
                        System.IO.Path.Combine(RecoveryLetter, "Recovery", "WindowsRE", "Winre.wim"),
                        true
                    );
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("");
                    Console.WriteLine("   An Error has occurred.");
                    Console.WriteLine("   Error: Cannot find WindowsRE.wim.");
                    if (ui == Entities.UI.Command)
                        Console.WriteLine(); // Only write new line if ui mode is disabled, so that the ui can read the error code above.
                    Console.ResetColor();
                    if (ui == Entities.UI.Graphical) { worker.ReportProgress(304, ""); }
                    if (ui == Entities.UI.Command) { Environment.Exit(1); }
                }
            }
            catch
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("");
                Console.WriteLine("   An Error has occurred.");
                Console.WriteLine("   Error: Cannot copy recovery image.");
                if (ui == Entities.UI.Command)
                    Console.WriteLine(); // Only write new line if ui mode is disabled, so that the ui can read the error code above.
                Console.ResetColor();
                if (ui == Entities.UI.Graphical) { worker.ReportProgress(304, ""); }
                if (ui == Entities.UI.Command) { Environment.Exit(1); }
            }

            // Register recovery partition
            try
            {
                Process p = new Process();
                p.StartInfo.FileName = System.IO.Path.Combine(WindowsPath, "System32", "Reagentc.exe");
                p.StartInfo.Arguments = $"/Setreimage /Path {RecoveryLetter}\\Recovery\\WindowsRE /Target {WindowsPath}";
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.CreateNoWindow = true;
                p.Start();
                p.WaitForExit();
            }
            catch
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("");
                Console.WriteLine("   An Error has occurred.");
                Console.WriteLine("   Error: Cannot register recovery partition.");
                if (ui == Entities.UI.Command)
                    Console.WriteLine(); // Only write new line if ui mode is disabled, so that the ui can read the error code above.
                Console.ResetColor();
                if (ui == Entities.UI.Graphical) { worker.ReportProgress(304, ""); }
                if (ui == Entities.UI.Command) { Environment.Exit(1); }
            }

            ConsoleUtility.WriteProgressBar(100, true);
            Console.WriteLine();
            if (ui == Entities.UI.Graphical) { worker.ReportProgress(101, ""); worker.ReportProgress(100, ""); }
        }


        public static string GetInfo(string ImagePath)
        {
            using (WimHandle file = WimgApi.CreateFile(ImagePath, WimFileAccess.Read, WimCreationDisposition.OpenExisting, WimCreateFileOptions.None, WimCompressionType.None))
            {
                string a = WimgApi.GetImageInformationAsString(file);
                return a;
            }
        }
    }

    internal class Apply
    {
        internal static BackgroundWorker BW = null;

        internal static void WriteToDisk(string ImagePath, int Index, string Drive, BackgroundWorker worker = null)
        {
            BW = worker;

            string path = Drive;
            using (WimHandle file = WimgApi.CreateFile(ImagePath, WimFileAccess.Read, WimCreationDisposition.OpenExisting, WimCreateFileOptions.None, WimCompressionType.None))
            {
                WimgApi.SetTemporaryPath(file, Environment.GetEnvironmentVariable("TEMP"));
                WimgApi.RegisterMessageCallback(file, new WimMessageCallback(ApplyCallbackMethod));
                try
                {
                    using (WimHandle imageHandle = WimgApi.LoadImage(file, Index))
                        WimgApi.ApplyImage(imageHandle, path, WimApplyImageOptions.None);
                }
                finally
                {
                    WimgApi.UnregisterMessageCallback(file, new WimMessageCallback(ApplyCallbackMethod));
                }
            }
        }

        private static WimMessageResult ApplyCallbackMethod(WimMessageType messageType, object message, object userData)
        {
            switch (messageType)
            {
                case WimMessageType.Progress:
                    WimMessageProgress wimMessageProgress = (WimMessageProgress)message;
                    if (BW != null)
                    {
                        BW.ReportProgress(wimMessageProgress.PercentComplete, ""); // Update progress bar
                        BW.ReportProgress(202, ""); // Update progress text
                    }
                    ConsoleUtility.WriteProgressBar(wimMessageProgress.PercentComplete, true);
                    break;
                        
                case WimMessageType.Error:
                    WimMessageError wimMessageError = (WimMessageError)message;
                    Console.WriteLine($"Error: {0} ({1})", (object)wimMessageError.Path, (object)wimMessageError.Win32ErrorCode);
                    if (BW != null) { BW.ReportProgress(302, ""); }
                    break;
                    
                case WimMessageType.Warning:
                    WimMessageWarning wimMessageWarning = (WimMessageWarning)message;
                    Console.WriteLine($"Warning: {0} ({1})", (object)wimMessageWarning.Path, (object)wimMessageWarning.Win32ErrorCode);
                    break;
            }
            return WimMessageResult.Success;
        }
    }
}
