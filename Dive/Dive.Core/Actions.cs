/*
 * Dive (formally deploya) - Deployment is very easy.
 * Copyright (c) 2018 - 2024 Exploitox.
 *
 * Dive is licensed under MIT License (https://github.com/valnoxy/dive/blob/main/LICENSE).
 * So you are allowed to use freely and modify the application.
 * I will not be responsible for any outcome.
 * Proceed with any action at your own risk.
 *
 * Source code: https://github.com/valnoxy/dive
 */

using Dive.Core.Common;
using Microsoft.Wim;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using Dive.Core.Assets;
using Microsoft.Dism;
using Newtonsoft.Json;
using Dive.Core.Action.Deployment;
using Dive.Core.Action.Capturing;

namespace Dive.Core
{
    public class Actions
    {
        /// <summary>
        /// Prepare and format the specified disk for Windows deployment.
        /// </summary>
        /// <param name="firmware">Firmware type of the device</param>
        /// <param name="bootloader">Windows Bootloader</param>
        /// <param name="disk">Disk Identifier</param>
        /// <param name="isRemovable">Is Disk removable or not</param>
        /// <param name="useRecovery">Install native recovery partition</param>
        /// <param name="windowsDrive">Drive letter of the Windows partition</param>
        /// <param name="bootDrive">Drive letter of the Boot partition</param>
        /// <param name="recoveryDrive">Drive letter of the Recovery partition</param>
        /// <param name="worker">Background worker for Graphical user interface</param>
        public static void PrepareDisk(Entities.Firmware firmware, Entities.Bootloader bootloader, int disk, bool isRemovable, Entities.PartitionStyle partitionStyle, bool useRecovery, string windowsDrive, string bootDrive = "\0", string recoveryDrive = "\0", BackgroundWorker worker = null)
        {
            // General message
            worker?.ReportProgress(0, JsonConvert.SerializeObject(new ActionWorker
            {
                Action = Progress.PrepareDisk,
                IsError = false,
                IsIndeterminate = false,
                Message = "Partitioning disk ..."
            }));

            // Validate parsed arguments
            if (useRecovery && (windowsDrive == "\0" || bootDrive == "\0" || recoveryDrive == "\0"))
                throw new ArgumentException("Invalid arguments");
            if (bootloader == Entities.Bootloader.BOOTMGR && firmware == Entities.Firmware.EFI && (windowsDrive == "\0" || bootDrive == "\0"))
                throw new ArgumentException("Invalid arguments");

            // Start Diskpart tool
            var partDest = new Process();
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
                            partDest.StandardInput.WriteLine("shrink minimum=800");
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
                            partDest.StandardInput.WriteLine(isRemovable
                                ? "create partition primary size=100"
                                : "create partition efi size=100");
                            partDest.StandardInput.WriteLine("format quick fs=fat32 label=System");
                            partDest.StandardInput.WriteLine("assign letter=" + bootDrive.Substring(0, 1));
                            partDest.StandardInput.WriteLine("create partition msr size=16");
                            partDest.StandardInput.WriteLine("create partition primary");
                            partDest.StandardInput.WriteLine("shrink minimum=800");
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
                            partDest.StandardInput.WriteLine(isRemovable
                                ? "create partition primary size=100"
                                : "create partition efi size=100"); partDest.StandardInput.WriteLine("format quick fs=fat32 label=System");
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
                            worker?.ReportProgress(0, JsonConvert.SerializeObject(new ActionWorker
                            {
                                Action = Progress.PrepareDisk,
                                IsError = true,
                                IsIndeterminate = false,
                                Message = "You cannot use Single Partition layout on a system with EFI Firmware."
                            }));
                            return;
                    }
                    break;
            }
            partDest.WaitForExit();

            if (partDest.ExitCode != 0)
            {
                worker?.ReportProgress(0, JsonConvert.SerializeObject(new ActionWorker
                {
                    Action = Progress.PrepareDisk,
                    IsError = true,
                    IsIndeterminate = false,
                    Message = "Diskpart terminated with exit code " + partDest.ExitCode + "."
                }));
                return;
            }

            worker?.ReportProgress(100, JsonConvert.SerializeObject(new ActionWorker
            {
                Action = Progress.PrepareDisk,
                IsError = false,
                IsIndeterminate = false,
                Message = "Done."
            }));
        }

        /// <summary>
        /// Installs the specified Windows image.
        /// </summary>
        /// <param name="path">Target path</param>
        /// <param name="wimFile">Path to image file</param>
        /// <param name="index">Index identifier of the SKU</param>
        /// <param name="worker">Background worker for Graphical user interface</param>
        public static void ApplyWim(string path, string wimFile, int index, BackgroundWorker worker = null)
        {
            worker?.ReportProgress(0, JsonConvert.SerializeObject(new ActionWorker
            {
                Action = Progress.ApplyImage,
                IsError = false,
                IsIndeterminate = false,
                Message = "Applying image ..."
            }));
            Apply.WriteToDisk(wimFile, index, path, worker);
            worker?.ReportProgress(100, JsonConvert.SerializeObject(new ActionWorker
            {
                Action = Progress.ApplyImage,
                IsError = false,
                IsIndeterminate = false,
                Message = "Done."
            }));
        }

        /// <summary>
        /// Installs the bootloader to the specified disk.
        /// </summary>
        /// <param name="firmware">Firmware type of the device</param>
        /// <param name="bootloader">Windows Bootloader</param>
        /// <param name="windowsPath">Path to the Windows directory</param>
        /// <param name="bootloaderLetter">Drive letter of the boot partition</param>
        /// <param name="worker">Background worker for Graphical user interface</param>
        public static void InstallBootloader(Entities.Firmware firmware, Entities.Bootloader bootloader, string windowsPath, string bootloaderLetter, BackgroundWorker worker = null)
        {
            worker?.ReportProgress(0, JsonConvert.SerializeObject(new ActionWorker
            {
                Action = Progress.InstallBootloader,
                IsError = false,
                IsIndeterminate = true,
                Message = "Installing bootloader ..."
            }));
            var bootLd = new Process();

            #region Bootloader check
            switch (bootloader)
            {
                case Entities.Bootloader.NTLDR:
                {
                    var strBl = bootloaderLetter[..2];
                    bootLd.StartInfo.FileName = "bootsect.exe";
                    bootLd.StartInfo.Arguments = $"/nt52 {strBl} /force /mbr";
                    break;
                }
                case Entities.Bootloader.BOOTMGR:
                {
                    windowsPath = $"{windowsPath}Windows";
                    bootLd.StartInfo.FileName = "bcdboot.exe";

                    bootLd.StartInfo.Arguments = firmware switch
                    {
                        // BIOS
                        Entities.Firmware.BIOS => $"{windowsPath} /s {bootloaderLetter} /f BIOS",
                        // EFI
                        Entities.Firmware.EFI => $"{windowsPath} /s {bootloaderLetter} /f UEFI",
                        _ => bootLd.StartInfo.Arguments
                    };
                    break;
                }
            }
            #endregion

            bootLd.StartInfo.UseShellExecute = false;
            bootLd.StartInfo.RedirectStandardOutput = true;
            bootLd.StartInfo.CreateNoWindow = true;
            bootLd.Start();
            bootLd.WaitForExit();

            if (bootLd.ExitCode != 0)
            {
                worker?.ReportProgress(0, JsonConvert.SerializeObject(new ActionWorker
                {
                    Action = Progress.InstallBootloader,
                    IsError = true,
                    IsIndeterminate = false,
                    Message = "Bootsect / bcdboot terminated with exit code " + bootLd.ExitCode + "."
                }));
                return;
            }

            worker?.ReportProgress(100, JsonConvert.SerializeObject(new ActionWorker
            {
                Action = Progress.InstallBootloader,
                IsError = false,
                IsIndeterminate = false,
                Message = "Done."
            }));
        }

        /// <summary>
        /// Install and register the recovery partition.
        /// </summary>
        /// <param name="windowsPath">Path to the Windows directory</param>
        /// <param name="recoveryLetter">Drive letter of the recovery partition</param>
        /// <param name="implementDive">Implement Dive as a custom recovery tool into Windows RE</param>
        /// <param name="worker">Background worker for Graphical user interface</param>
        public static void InstallRecovery(string windowsPath, string recoveryLetter, bool implementDive = false, BackgroundWorker worker = null)
        {
            worker?.ReportProgress(0, JsonConvert.SerializeObject(new ActionWorker
            {
                Action = Progress.InstallRecovery,
                IsError = false,
                IsIndeterminate = true,
                Message = "Copying recovery image to partition ..."
            }));

            // Create Recovery directory
            try
            {
                System.IO.Directory.CreateDirectory(System.IO.Path.Combine(recoveryLetter, "Recovery", "WindowsRE"));
            }
            catch 
            {
                worker?.ReportProgress(0, JsonConvert.SerializeObject(new ActionWorker
                {
                    Action = Progress.InstallRecovery,
                    IsError = true,
                    IsIndeterminate = false,
                    Message = "Failed to create recovery directory."
                }));
                return;
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
                    worker?.ReportProgress(0, JsonConvert.SerializeObject(new ActionWorker
                    {
                        Action = Progress.InstallRecovery,
                        IsError = true,
                        IsIndeterminate = false,
                        Message = "Recovery image does not exist in this image."
                    }));
                    return;
                }
            }
            catch (Exception ex )
            {
                Console.ForegroundColor = ConsoleColor.Red;
                worker?.ReportProgress(0, JsonConvert.SerializeObject(new ActionWorker
                {
                    Action = Progress.InstallRecovery,
                    IsError = true,
                    IsIndeterminate = false,
                    Message = "Failed to copy recovery image: " + ex.Message
                }));
            }

            // Implement Dive
            if (implementDive)
            {
                try
                {
                    worker?.ReportProgress(0, JsonConvert.SerializeObject(new ActionWorker
                    {
                        Action = Progress.InstallRecovery,
                        IsError = false,
                        IsIndeterminate = true,
                        Message = "Implementing Dive into Windows Recovery image ..."
                    }));

                    // Initialize Dism API
                    worker?.ReportProgress(0, JsonConvert.SerializeObject(new ActionWorker
                    {
                        IsDebug = true,
                        Message = "Initializing DISM API ..."
                    }));
                    DismApi.Initialize(DismLogLevel.LogErrors);

                    // Prepare session
                    var imageFile = System.IO.Path.Combine(recoveryLetter, "Recovery", "WindowsRE", "Winre.wim");
                    var mountPath = System.IO.Path.Combine(windowsPath, "..", "dive-tmp", "mount");
                    var targetPath = System.IO.Path.Combine(mountPath, "sources", "recovery", "tools");
                    const int imageIndex = 1;
                    Directory.CreateDirectory(mountPath);

                    // Mount image
                    worker?.ReportProgress(0, JsonConvert.SerializeObject(new ActionWorker
                    {
                        IsDebug = true,
                        Message = "Mounting Recovery Image ..."
                    }));
                    DismApi.MountImage(imageFile, mountPath, imageIndex);

                    // Copy Dive
                    worker?.ReportProgress(0, JsonConvert.SerializeObject(new ActionWorker
                    {
                        IsDebug = true,
                        Message = "Copying Dive into Recovery Image ..."
                    }));
                    var strExeFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                    var strWorkPath = System.IO.Path.GetDirectoryName(strExeFilePath);
                    Common.FileIO.CopyFilesRecursively(new DirectoryInfo(strWorkPath), new DirectoryInfo(Path.Combine(targetPath, "Dive")));

                    // Create WinREConfig.xml
                    worker?.ReportProgress(0, JsonConvert.SerializeObject(new ActionWorker
                    {
                        IsDebug = true,
                        Message = "Creating and injecting WinREConfig.xml ..."
                    }));
                    File.WriteAllText(Path.Combine(targetPath, "WinREConfig.xml"), WinRE.WinREConfig);

                    // Unmount image
                    worker?.ReportProgress(0, JsonConvert.SerializeObject(new ActionWorker
                    {
                        IsDebug = true,
                        Message = "Commiting changes to recovery image ..."
                    }));
                    DismApi.UnmountImage(mountPath, true);
                }
                catch
                {
                    worker?.ReportProgress(0, JsonConvert.SerializeObject(new ActionWorker
                    {
                        Action = Progress.InstallRecovery,
                        IsError = true,
                        IsIndeterminate = true,
                        Message = "Failed to inject Dive into Recovery image."
                    }));
                    return;
                }
                finally
                {
                    // Shutdown Dism API
                    worker?.ReportProgress(0, JsonConvert.SerializeObject(new ActionWorker
                    {
                        IsDebug = true,
                        Message = "Shutting down DISM API ..."
                    })); 
                    DismApi.Shutdown();
                }
            }

            // Register recovery partition
            try
            {
                worker?.ReportProgress(0, JsonConvert.SerializeObject(new ActionWorker
                {
                    Action = Progress.InstallRecovery,
                    IsError = false,
                    IsIndeterminate = true,
                    Message = "Registering Recovery image to Windows ..."
                }));
                var p = new Process();
                p.StartInfo.FileName = System.IO.Path.Combine(windowsPath, "System32", "Reagentc.exe");
                p.StartInfo.Arguments = $"/Setreimage /Path {recoveryLetter}\\Recovery\\WindowsRE /Target {windowsPath}";
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.CreateNoWindow = true;
                p.Start();
                p.WaitForExit();

                if (implementDive)
                {
                    worker?.ReportProgress(0, JsonConvert.SerializeObject(new ActionWorker
                    {
                        IsDebug = true,
                        Message = "Apply Diagnostics Tool configuration to recovery image ..."
                    }));
                    var recoveryBootMenuPath = System.IO.Path.Combine(recoveryLetter, "Recovery", "BootMenu");
                    Directory.CreateDirectory(recoveryBootMenuPath);

                    File.WriteAllText(Path.Combine(recoveryBootMenuPath, "AddDiagnosticsToolToBootMenu.xml"), WinRE.AddDiagnosticsToolToBootMenu);
                    p.StartInfo.Arguments = $"/setbootshelllink /configfile {Path.Combine(recoveryBootMenuPath, "AddDiagnosticsToolToBootMenu.xml")}";
                    p.Start();
                    p.WaitForExit();
                }
            }
            catch
            {
                worker?.ReportProgress(0, JsonConvert.SerializeObject(new ActionWorker
                {
                    Action = Progress.InstallRecovery,
                    IsError = true,
                    IsIndeterminate = false,
                    Message = "Failed to register recovery image."
                }));
                return;
            }

            worker?.ReportProgress(100, JsonConvert.SerializeObject(new ActionWorker
            {
                Action = Progress.InstallRecovery,
                IsError = false,
                IsIndeterminate = false,
                Message = "Done."
            }));
        }

        /// <summary>
        /// Installs the unattended configuration file to the Windows Installation (only Vista and higher).
        /// </summary>
        /// <param name="windowsPath">Path to the Windows directory</param>
        /// <param name="configuration">Content of the configuration file</param>
        /// <param name="oemLogoPath">Path to the OEM logo</param>
        /// <param name="performDismApply">Whether to perform a DISM Apply-Unattend</param>
        /// <param name="worker">Background worker for Graphical user interface</param>
        public static void InstallUnattend(string windowsPath, string configuration, string oemLogoPath = null, bool performDismApply = false, BackgroundWorker worker = null)
        {
            worker?.ReportProgress(0, JsonConvert.SerializeObject(new ActionWorker
            {
                Action = Progress.InstallUnattend,
                IsError = false,
                IsIndeterminate = true,
                Message = "Installing Unattend configuration ..."
            }));

            // Create Recovery directory
            try
            {
                System.IO.Directory.CreateDirectory(System.IO.Path.Combine(windowsPath, "Panther"));
            }
            catch
            {
                worker?.ReportProgress(0, JsonConvert.SerializeObject(new ActionWorker
                {
                    Action = Progress.InstallUnattend,
                    IsError = true,
                    IsIndeterminate = false,
                    Message = "Failed to create Panther directory."
                }));
                return;
            }

            // Write config to disk as unattend.xml
            try
            {
                System.IO.File.WriteAllText(System.IO.Path.Combine(windowsPath, "Panther", "unattend.xml"), configuration);
            }
            catch
            {
                worker?.ReportProgress(0, JsonConvert.SerializeObject(new ActionWorker
                {
                    Action = Progress.InstallUnattend,
                    IsError = true,
                    IsIndeterminate = false,
                    Message = "Failed to write Unattend configuration to disk."
                }));
                return;
            }

            // Copy OEM logo to Windows\System32 directory
            if (!string.IsNullOrEmpty(oemLogoPath))
            {
                try
                {
                    System.IO.File.Copy(oemLogoPath, Path.Combine(windowsPath, "System32", "logo.bmp"), true);
                }
                catch
                {
                    worker?.ReportProgress(0, JsonConvert.SerializeObject(new ActionWorker
                    {
                        Action = Progress.InstallUnattend,
                        IsError = true,
                        IsIndeterminate = false,
                        Message = "Failed to copy OEM logo to disk."
                    }));
                    return;
                }
            }

            // Perform DISM Apply Unattend Task
            if (performDismApply)
            {
                try
                {
                    worker?.ReportProgress(0, JsonConvert.SerializeObject(new ActionWorker
                    {
                        Action = Progress.InstallUnattend,
                        IsError = false,
                        IsIndeterminate = true,
                        Message = "Registering Unattend configuration with DISM ..."
                    }));
                    var f = new FileInfo(windowsPath);
                    var drive = Path.GetPathRoot(f.FullName);
                    
                    var p = new Process();
                    p.StartInfo.FileName = System.IO.Path.Combine(windowsPath, "System32", "Dism.exe");
                    p.StartInfo.Arguments = $"/Image:{drive} /Apply-Unattend:{System.IO.Path.Combine(windowsPath, "Panther", "unattend.xml")}";
                    p.StartInfo.RedirectStandardOutput = true;
                    p.StartInfo.CreateNoWindow = true;
                    p.Start();
                    p.WaitForExit();
                }
                catch
                {
                    worker?.ReportProgress(0, JsonConvert.SerializeObject(new ActionWorker
                    {
                        Action = Progress.InstallUnattend,
                        IsError = true,
                        IsIndeterminate = false,
                        Message = "Failed to apply Unattend configuration with DISM."
                    }));
                    return;
                }
            }

            worker?.ReportProgress(100, JsonConvert.SerializeObject(new ActionWorker
            {
                Action = Progress.InstallUnattend,
                IsError = false,
                IsIndeterminate = false,
                Message = "Done."
            }));
        }

        /// <summary>
        /// Installs the specific driver to the Windows Installation (only Vista and higher).
        /// </summary>
        /// <param name="windowsDrive">Drive letter to the Windows disk</param>
        /// <param name="driverPath">List of driver paths</param>
        /// <param name="worker">Background worker for Graphical user interface</param>
        public static void InstallDriver(string windowsDrive, List<string> driverPath, BackgroundWorker worker = null)
        {
            worker?.ReportProgress(0, JsonConvert.SerializeObject(new ActionWorker
            {
                Action = Progress.InstallDrivers,
                IsError = false,
                IsIndeterminate = true,
                Message = "Installing drivers ..."
            }));

            Apply.AddDriverToDisk(windowsDrive, driverPath, worker);

            worker?.ReportProgress(100, JsonConvert.SerializeObject(new ActionWorker
            {
                Action = Progress.InstallDrivers,
                IsError = false,
                IsIndeterminate = false,
                Message = "Done."
            }));
        }

        /// <summary>
        /// Get information about an Windows deployment image (WIM).
        /// </summary>
        /// <param name="imagePath">Path to image file</param>
        /// <returns>Information about the image file as XML</returns>
        public static string GetInfo(string imagePath)
        {
            var imageExtension = Path.GetExtension(imagePath);
            var option = imageExtension switch
            {
                ".wim" => WimCreateFileOptions.None,
                ".esd" => WimCreateFileOptions.Chunked, // 0x20000000
                _ => WimCreateFileOptions.None
            };

            using var file = WimgApi.CreateFile(imagePath, 
                WimFileAccess.Read, WimCreationDisposition.OpenExisting, option, WimCompressionType.None);
            var a = WimgApi.GetImageInformationAsString(file);
            return a;
        }

        /// <summary>
        /// Get array of unused Drive letters for deployment.
        /// </summary>
        /// <param name="style">Partition style</param>
        /// <returns>Array of unused Drive letters. Length depends on the partition style.</returns>
        public static char[] GetSystemLetters(Entities.PartitionStyle style)
        {
            // Search for non used drive letters
            var getDrives = Management.GetAvailableDriveLetters();
            var arr = new List<char>();

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
        /// <param name="name">Image Name</param>
        /// <param name="description">Description of the image</param>
        /// <param name="pathToCapture">Path of the captured dir</param>
        /// <param name="pathToImage">Path of the output file</param>
        /// <param name="worker">Background worker for Graphical user interface</param>
        public static void CaptureToWim(string name, string description, string pathToCapture, string pathToImage, BackgroundWorker worker = null)
        {
            worker?.ReportProgress(0, JsonConvert.SerializeObject(new ActionWorker
            {
                Action = Progress.CaptureDisk,
                IsError = false,
                IsIndeterminate = false,
                Message = "Capturing disk ..."
            }));

            Capture.CreateWim(name, description, pathToCapture, pathToImage, worker);
            worker?.ReportProgress(0, JsonConvert.SerializeObject(new ActionWorker
            {
                Action = Progress.CaptureDisk,
                IsError = false,
                IsIndeterminate = false,
                Message = "Done."
            }));
        }
    }
}
