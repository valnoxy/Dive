/* 
 * deploya - Fast and Easy way to deploy Windows
 * Copyright (c) 2018 - 2023 Exploitox.
 * 
 * deploya is licensed under MIT License (https://github.com/valnoxy/deploya/blob/main/LICENSE). 
 * So you are allowed to use freely and modify the application. 
 * I will not be responsible for any outcome. 
 * Proceed with any action at your own risk.
 * 
 * Source code: https://github.com/valnoxy/deploya
 */

using deployaCore.Common;
using Microsoft.Wim;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using static deployaCore.Common.WIMDescription;
using static System.Net.Mime.MediaTypeNames;

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

        public enum PartitionStyle
        {
            Single,
            SeparateBoot,
            Full
        }
    }

    public class Output
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

        /// <summary>
        /// Prepare and format the specified disk for Windows deployment.
        /// </summary>
        /// <param name="firmware">Firmware type of the device</param>
        /// <param name="bootloader">Windows Bootloader</param>
        /// <param name="ui">User Interface type</param>
        /// <param name="disk">Disk Identifier</param>
        /// <param name="useRecovery">Install native recovery partition</param>
        /// <param name="windowsDrive">Drive letter of the Windows partition</param>
        /// <param name="bootDrive">Drive letter of the Boot partition</param>
        /// <param name="recoveryDrive">Drive letter of the Recovery partition</param>
        /// <param name="worker">Background worker for Graphical user interface</param>
        public static void PrepareDisk(Entities.Firmware firmware, Entities.Bootloader bootloader, Entities.UI ui, int disk, Entities.PartitionStyle partitionStyle, bool useRecovery, string windowsDrive, string bootDrive = "\0", string recoveryDrive = "\0", BackgroundWorker worker = null)
        {
            // General message
            Output.Write("Partitioning disk ...         ");
            ConsoleUtility.WriteProgressBar(0);
            if (ui == Entities.UI.Graphical) { worker.ReportProgress(102, ""); }

            // Validate parsed arguments
            if (useRecovery && (windowsDrive == "\0" || bootDrive == "\0" || recoveryDrive == "\0"))
                throw new ArgumentException("Invalid arguments");
            if (bootloader == Entities.Bootloader.BOOTMGR && firmware == Entities.Firmware.EFI && (windowsDrive == "\0" || bootDrive == "\0"))
                throw new ArgumentException("Invalid arguments");

            // Start Diskpart tool
            Process partDest = new Process();
            partDest.StartInfo.FileName = "diskpart.exe";
            partDest.StartInfo.UseShellExecute = false;
            partDest.StartInfo.CreateNoWindow = true;
            partDest.StartInfo.RedirectStandardInput = true;
            partDest.StartInfo.RedirectStandardOutput = true;
            partDest.Start();

            // Partition information
            switch (partitionStyle)
            {
                case Entities.PartitionStyle.Full:
                    switch (firmware)
                    {
                        case Entities.Firmware.BIOS: // Windows + Recovery + Boot on BIOS
                            partDest.StandardInput.WriteLine("select disk " + disk);
                            partDest.StandardInput.WriteLine("clean");
                            partDest.StandardInput.WriteLine("create partition primary size=100");
                            partDest.StandardInput.WriteLine("format quick fs=ntfs label=System");
                            partDest.StandardInput.WriteLine("assign letter=" + bootDrive.Substring(0, 1));
                            partDest.StandardInput.WriteLine("active");
                            partDest.StandardInput.WriteLine("create partition primary");
                            partDest.StandardInput.WriteLine("shrink minimum=650");
                            partDest.StandardInput.WriteLine("format quick fs=ntfs label=Windows");
                            partDest.StandardInput.WriteLine("assign letter=" + windowsDrive.Substring(0, 1));
                            partDest.StandardInput.WriteLine("create partition primary");
                            partDest.StandardInput.WriteLine("format quick fs=ntfs label=Recovery");
                            partDest.StandardInput.WriteLine("assign letter=" + recoveryDrive.Substring(0, 1));
                            partDest.StandardInput.WriteLine("set id=27");
                            partDest.StandardInput.WriteLine("exit");
                            break;
                        case Entities.Firmware.EFI: // Windows + Recovery + Boot on EFI
                            partDest.StandardInput.WriteLine("select disk " + disk);
                            partDest.StandardInput.WriteLine("clean");
                            partDest.StandardInput.WriteLine("convert gpt");
                            partDest.StandardInput.WriteLine("create partition efi size=100");
                            partDest.StandardInput.WriteLine("format quick fs=fat32 label=System");
                            partDest.StandardInput.WriteLine("assign letter=" + bootDrive.Substring(0, 1));
                            partDest.StandardInput.WriteLine("create partition msr size=16");
                            partDest.StandardInput.WriteLine("create partition primary");
                            partDest.StandardInput.WriteLine("shrink minimum=650");
                            partDest.StandardInput.WriteLine("format quick fs=ntfs label=Windows");
                            partDest.StandardInput.WriteLine("assign letter=" + windowsDrive.Substring(0, 1));
                            partDest.StandardInput.WriteLine("create partition primary");
                            partDest.StandardInput.WriteLine("format quick fs=ntfs label=Recovery");
                            partDest.StandardInput.WriteLine("assign letter=" + recoveryDrive.Substring(0, 1));
                            partDest.StandardInput.WriteLine("set id=de94bba4-06d1-4d40-a16a-bfd50179d6ac");
                            partDest.StandardInput.WriteLine("gpt attributes=0x8000000000000001");
                            partDest.StandardInput.WriteLine("exit");
                            break;
                    }
                    break;
                
                case Entities.PartitionStyle.SeparateBoot:
                    switch (firmware)
                    {
                        case Entities.Firmware.BIOS: // Windows + Boot on BIOS
                            partDest.StandardInput.WriteLine("select disk " + disk);
                            partDest.StandardInput.WriteLine("clean");
                            partDest.StandardInput.WriteLine("create partition primary size=100");
                            partDest.StandardInput.WriteLine("format quick fs=ntfs label=System");
                            partDest.StandardInput.WriteLine("assign letter=" + bootDrive.Substring(0, 1));
                            partDest.StandardInput.WriteLine("active");
                            partDest.StandardInput.WriteLine("create partition primary");
                            partDest.StandardInput.WriteLine("format quick fs=ntfs label=Windows");
                            partDest.StandardInput.WriteLine("assign letter=" + windowsDrive.Substring(0, 1));
                            partDest.StandardInput.WriteLine("exit");
                            break;
                        case Entities.Firmware.EFI: // Windows + Boot on EFI
                            partDest.StandardInput.WriteLine("select disk " + disk);
                            partDest.StandardInput.WriteLine("clean");
                            partDest.StandardInput.WriteLine("convert gpt");
                            partDest.StandardInput.WriteLine("create partition efi size=100");
                            partDest.StandardInput.WriteLine("format quick fs=fat32 label=System");
                            partDest.StandardInput.WriteLine("assign letter=" + bootDrive.Substring(0, 1));
                            partDest.StandardInput.WriteLine("create partition msr size=16");
                            partDest.StandardInput.WriteLine("create partition primary");
                            partDest.StandardInput.WriteLine("format quick fs=ntfs label=Windows");
                            partDest.StandardInput.WriteLine("assign letter=" + windowsDrive.Substring(0, 1));
                            partDest.StandardInput.WriteLine("gpt attributes=0x8000000000000001");
                            partDest.StandardInput.WriteLine("exit");
                            break;
                    }
                    break;
                
                case Entities.PartitionStyle.Single:
                    switch (firmware)
                    {
                        case Entities.Firmware.BIOS: // Windows on BIOS
                            partDest.StandardInput.WriteLine("select disk " + disk);
                            partDest.StandardInput.WriteLine("clean");
                            partDest.StandardInput.WriteLine("create partition primary");
                            partDest.StandardInput.WriteLine("format quick fs=ntfs label=Windows");
                            partDest.StandardInput.WriteLine("active");
                            partDest.StandardInput.WriteLine("assign letter=" + windowsDrive.Substring(0, 1));
                            partDest.StandardInput.WriteLine("exit");
                            break;
                        case Entities.Firmware.EFI: // Windows on EFI
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("");
                            Console.WriteLine("   An Error has occurred.");
                            Console.WriteLine("   Error: You cannot use Single Partition layout on a System with EFI Firmware.");
                            if (ui == Entities.UI.Command)
                                Console.WriteLine(); // Only write new line if ui mode is disabled, so that the ui can read the error code above.
                            Console.ResetColor();
                            if (ui == Entities.UI.Graphical) { worker.ReportProgress(301, ""); }
                            return;
                    }
                    break;
            }
            partDest.WaitForExit();

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

        /// <summary>
        /// Installs the specified Windows image.
        /// </summary>
        /// <param name="ui">User Interface type</param>
        /// <param name="path">Target path</param>
        /// <param name="wimfile">Path to image file</param>
        /// <param name="index">Index identifier of the SKU</param>
        /// <param name="worker">Background worker for Graphical user interface</param>
        public static void ApplyWIM(Entities.UI ui, string path, string wimfile, int index, BackgroundWorker worker = null)
        {
            Output.Write("Applying Image ...            ");
            ConsoleUtility.WriteProgressBar(0);
            if (ui == Entities.UI.Graphical) { worker.ReportProgress(101, ""); worker.ReportProgress(0, ""); }
            
            Apply.WriteToDisk(wimfile, index, path, worker);
            Console.WriteLine();
        }

        /// <summary>
        /// Installs the bootloader to the specified disk.
        /// </summary>
        /// <param name="firmware">Firmware type of the device</param>
        /// <param name="bootloader">Windows Bootloader</param>
        /// <param name="ui">User Interface type</param>
        /// <param name="windowsPath">Path to the Windows directory</param>
        /// <param name="bootloaderLetter">Drive letter of the boot partition</param>
        /// <param name="worker">Background worker for Graphical user interface</param>
        public static void InstallBootloader(Entities.Firmware firmware, Entities.Bootloader bootloader, Entities.UI ui, string windowsPath, string bootloaderLetter, BackgroundWorker worker = null)
        {
            Output.Write("Installing Bootloader ...     ");
            ConsoleUtility.WriteProgressBar(0);
            if (ui == Entities.UI.Graphical) { worker.ReportProgress(102, ""); worker.ReportProgress(0, ""); }

            Process bootld = new Process();

            #region Legacy check
            if (bootloader == Entities.Bootloader.NTLDR)
            {
                string StrBl = bootloaderLetter.Substring(0, 2);
                bootld.StartInfo.FileName = "bootsect.exe";
                bootld.StartInfo.Arguments = $"/nt52 {StrBl} /force /mbr";
            }
            #endregion

            #region BIOS / EFI check
            if (bootloader == Entities.Bootloader.BOOTMGR)
            {
                bootld.StartInfo.FileName = "bcdboot.exe";

                if (firmware == Entities.Firmware.BIOS) // BIOS
                    bootld.StartInfo.Arguments = $"{windowsPath} /s {bootloaderLetter} /f BIOS";

                if (firmware == Entities.Firmware.EFI) // EFI
                    bootld.StartInfo.Arguments = $"{windowsPath} /s {bootloaderLetter} /f UEFI";
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

        /// <summary>
        /// Install and register the recovery partition.
        /// </summary>
        /// <param name="ui">User Interface type</param>
        /// <param name="windowsPath">Path to the Windows directory</param>
        /// <param name="recoveryLetter">Drive letter of the recovery partition</param>
        /// <param name="worker">Background worker for Graphical user interface</param>
        public static void InstallRecovery(Entities.UI ui, string windowsPath, string recoveryLetter, BackgroundWorker worker = null)
        {
            Output.Write("Installing Recovery ...       ");
            ConsoleUtility.WriteProgressBar(0);
            if (ui == Entities.UI.Graphical) { worker.ReportProgress(102, ""); worker.ReportProgress(0, ""); }

            // Create Recovery directory
            try
            {
                System.IO.Directory.CreateDirectory(System.IO.Path.Combine(recoveryLetter, "Recovery", "WindowsRE"));
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
                if (System.IO.File.Exists(System.IO.Path.Combine(windowsPath, "System32", "Recovery", "Winre.wim")))
                {
                    System.IO.File.Copy(
                        System.IO.Path.Combine(windowsPath, "System32", "Recovery", "Winre.wim"),
                        System.IO.Path.Combine(recoveryLetter, "Recovery", "WindowsRE", "Winre.wim"),
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
                p.StartInfo.FileName = System.IO.Path.Combine(windowsPath, "System32", "Reagentc.exe");
                p.StartInfo.Arguments = $"/Setreimage /Path {recoveryLetter}\\Recovery\\WindowsRE /Target {windowsPath}";
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

        /// <summary>
        /// Installs the unattended configuration file to the Windows Installation (only Vista and higher).
        /// </summary>
        /// <param name="ui">User Interface type</param>
        /// <param name="windowsPath">Path to the Windows directory</param>
        /// <param name="configuration">Content of the configuration file</param>
        /// <param name="oemLogoPath">Path to the OEM logo</param>
        /// <param name="worker">Background worker for Graphical user interface</param>
        public static void InstallUnattend(Entities.UI ui, string windowsPath, string configuration, string oemLogoPath = null, BackgroundWorker worker = null)
        {
            Output.Write("Installing unattend file ...  ");
            ConsoleUtility.WriteProgressBar(0);
            if (ui == Entities.UI.Graphical) { worker.ReportProgress(102, ""); worker.ReportProgress(0, ""); }

            // Create Recovery directory
            try
            {
                System.IO.Directory.CreateDirectory(System.IO.Path.Combine(windowsPath, "Panther"));
            }
            catch
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("");
                Console.WriteLine("   An Error has occurred.");
                Console.WriteLine("   Error: Cannot create Panther directory.");
                if (ui == Entities.UI.Command)
                    Console.WriteLine(); // Only write new line if ui mode is disabled, so that the ui can read the error code above.
                Console.ResetColor();
                if (ui == Entities.UI.Graphical) { worker.ReportProgress(305, ""); }
                if (ui == Entities.UI.Command) { Environment.Exit(1); }
            }

            // Write config to disk as unattend.xml
            try
            {
                System.IO.File.WriteAllText(System.IO.Path.Combine(windowsPath, "Panther", "unattend.xml"), configuration);
            }
            catch
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("");
                Console.WriteLine("   An Error has occurred.");
                Console.WriteLine("   Error: Cannot write content of unattend.xml!");
                if (ui == Entities.UI.Command)
                    Console.WriteLine(); // Only write new line if ui mode is disabled, so that the ui can read the error code above.
                Console.ResetColor();
                if (ui == Entities.UI.Graphical) { worker.ReportProgress(305, ""); }
                if (ui == Entities.UI.Command) { Environment.Exit(1); }
            }

            // Copy OEM logo to Windows\System32 directory
            if (oemLogoPath != null || oemLogoPath != "")
            {
                try
                {
                    System.IO.File.Copy(oemLogoPath, Path.Combine(windowsPath, "System32", "logo.bmp"), true);
                }
                catch
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("");
                    Console.WriteLine("   An Error has occurred.");
                    Console.WriteLine("   Error: Cannot copy OEM logo to the disk!");
                    if (ui == Entities.UI.Command)
                        Console.WriteLine(); // Only write new line if ui mode is disabled, so that the ui can read the error code above.
                    Console.ResetColor();
                    if (ui == Entities.UI.Graphical) { worker.ReportProgress(306, ""); }
                    if (ui == Entities.UI.Command) { Environment.Exit(1); }
                }
            }

            ConsoleUtility.WriteProgressBar(100, true);
            Console.WriteLine();
            if (ui == Entities.UI.Graphical) { worker.ReportProgress(101, ""); worker.ReportProgress(100, ""); }
        }

        /// <summary>
        /// Get information about an Windows deployment image (WIM).
        /// </summary>
        /// <param name="imagePath">Path to image file</param>
        /// <returns>Information about the image file as XML</returns>
        public static string GetInfo(string imagePath)
        {
            using (WimHandle file = WimgApi.CreateFile(imagePath, WimFileAccess.Read, WimCreationDisposition.OpenExisting, WimCreateFileOptions.None, WimCompressionType.None))
            {
                string a = WimgApi.GetImageInformationAsString(file);
                return a;
            }
        }

        /// <summary>
        /// Get array of unused Drive letters for deployment.
        /// </summary>
        /// <param name="style">Partition style</param>
        /// <returns>Array of unused Drive letters. Length depends on the partition style.</returns>
        public static char[] GetSystemLetters(Entities.PartitionStyle style)
        {
            // Search for non used drive letters
            char[] getDrives = Management.GetAvailableDriveLetters();
            List<char> arr = new List<char>();

            switch (style)
            {
                case Entities.PartitionStyle.Single when getDrives.Length >= 1:
                    arr.Add(getDrives[0]);
                    return arr.ToArray();

                case Entities.PartitionStyle.SeparateBoot when getDrives.Length >= 2:
                    arr.Add(getDrives[0]);
                    arr.Add(getDrives[1]);
                    return arr.ToArray();

                case Entities.PartitionStyle.Full when getDrives.Length >= 3:
                    arr.Add(getDrives[0]);
                    arr.Add(getDrives[1]);
                    arr.Add(getDrives[2]);
                    return arr.ToArray();

                default:
                    return null;
            }
        }

        /// <summary>
        /// Installs the specified Windows image.
        /// </summary>
        /// <param name="ui">User Interface type</param>
        /// <param name="name">Image Name</param>
        /// <param name="description">Description of the image</param>
        /// <param name="pathToCapture">Path of the captured dir</param>
        /// <param name="pathToImage">Path of the output file</param>
        /// <param name="worker">Background worker for Graphical user interface</param>
        public static void CaptureToWIM(Entities.UI ui, string name, string description, string pathToCapture, string pathToImage, BackgroundWorker worker = null)
        {
            Output.Write("Capture Image ...            ");
            ConsoleUtility.WriteProgressBar(0);
            if (ui == Entities.UI.Graphical) { worker.ReportProgress(101, ""); worker.ReportProgress(0, ""); }

            Capture.CreateWim(name, description, pathToCapture, pathToImage, worker);
            Console.WriteLine();
        }
    }

    internal class Apply
    {
        internal static BackgroundWorker BW = null;

        internal static void WriteToDisk(string imagePath, int index, string drive, BackgroundWorker worker = null)
        {
            BW = worker;

            string path = drive;
            using (WimHandle file = WimgApi.CreateFile(imagePath, WimFileAccess.Read, WimCreationDisposition.OpenExisting, WimCreateFileOptions.None, WimCompressionType.None))
            {
                WimgApi.SetTemporaryPath(file, Environment.GetEnvironmentVariable("TEMP"));
                WimgApi.RegisterMessageCallback(file, new WimMessageCallback(ApplyCallbackMethod));
                try
                {
                    using (WimHandle imageHandle = WimgApi.LoadImage(file, index))
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

    internal class Capture
    {
        internal static BackgroundWorker BW = null;
        public static void CreateWim(string name, string description, string pathToCapture, string pathToImage, BackgroundWorker worker)
        {
            BW = worker;

            using (WimHandle wimHandle = WimgApi.CreateFile(pathToImage, 
                       WimFileAccess.Write, 
                       WimCreationDisposition.CreateAlways, 
                       WimCreateFileOptions.None, 
                       WimCompressionType.None))
            {

                WimgApi.SetTemporaryPath(wimHandle, Environment.GetEnvironmentVariable("TEMP"));
                WimgApi.RegisterMessageCallback(wimHandle, CaptureCallbackMethod);
                try
                {
                    using (WimHandle imageHandle = WimgApi.CaptureImage(wimHandle, pathToCapture, WimCaptureImageOptions.None))
                    {
                        var xmlDocument = BuildImageInfo(name, description);
                        WimgApi.SetImageInformation(imageHandle, xmlDocument);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("CaptureImage error: " + ex.Message);
                }
                finally
                {
                    // Be sure to unregister the callback method
                    //
                    WimgApi.UnregisterMessageCallback(wimHandle, CaptureCallbackMethod);
                }
            }
        }

        internal static string BuildImageInfo(string name, string description)
        {
            var wimDescription = new deployaCore.Common.WIMDescription.WIMIMAGE()
            {
                NAME = name,
                DESCRIPTION = description,
                INDEX = 1
            };

            var emptyNamespaces = new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty });
            var serializer = new XmlSerializer(typeof(WIMDescription.WIMIMAGE));
            var settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.OmitXmlDeclaration = true;

            using var stream = new StringWriter();
            using var writer = XmlWriter.Create(stream, settings);
            serializer.Serialize(writer, wimDescription, emptyNamespaces);
            return stream.ToString();
        }

        private static WimMessageResult CaptureCallbackMethod(WimMessageType messageType, object message, object userData)
        {
            switch (messageType)
            {
                case WimMessageType.Progress:
                    WimMessageProgress wimMessageProgress = (WimMessageProgress)message;
                    if (BW != null)
                    {
                        BW.ReportProgress(wimMessageProgress.PercentComplete, ""); // Update progress bar
                        //BW.ReportProgress(201, ""); // Update progress text
                    }
                    ConsoleUtility.WriteProgressBar(wimMessageProgress.PercentComplete, true);
                    break;

                case WimMessageType.Process:
                    WimMessageProcess processMessage = (WimMessageProcess)message;
                    if (BW != null)
                    {
                        BW.ReportProgress(202, ""); // Update progress text
                        Output.WriteLine("Copying: " + processMessage.Path);
                    }
                    break;
              
                case WimMessageType.Scanning:
                    WimMessageScanning wimMessageScanning = (WimMessageScanning)message;
                    if (BW != null)
                    {
                        BW.ReportProgress(203, ""); // Update progress text
                        Output.WriteLine("Scanning: " + wimMessageScanning.Count);
                    }
                    break;

                case WimMessageType.Compress:
                    WimMessageCompress wimMessageCompress = (WimMessageCompress)message;
                    if (BW != null)
                    {
                        BW.ReportProgress(204, ""); // Update progress text
                        Output.WriteLine("Compress: " + wimMessageCompress.Path + " " + wimMessageCompress.Compress.ToString());
                    }
                    break;
                
                case WimMessageType.Error:
                    WimMessageError wimMessageError = (WimMessageError)message;
                    Console.WriteLine($"Error: {0} ({1})", (object)wimMessageError.Path, (object)wimMessageError.Win32ErrorCode);
                    if (BW != null) { BW.ReportProgress(301, ""); }
                    break;

                case WimMessageType.Warning:
                    WimMessageWarning wimMessageWarning = (WimMessageWarning)message;
                    Console.WriteLine($"Warning: {0} ({1})", (object)wimMessageWarning.Path, (object)wimMessageWarning.Win32ErrorCode);
                    break;
            }
            return WimMessageResult.Success;
        }
    }

    internal class Management
    {
        internal static char[] GetAvailableDriveLetters()
        {
            var availableDriveLetters = new List<char>() { 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z' };
            var drives = DriveInfo.GetDrives();
            foreach (var t in drives)
            {
                availableDriveLetters.Remove(t.Name.ToLower()[0]);
            }

            return availableDriveLetters.ToArray();
        }
        public class Utf8StringWriter : StringWriter
        {
            public override Encoding Encoding => Encoding.UTF8;
        }
    }
}
