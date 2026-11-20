/*
 * Dive (formally deploya) - Deployment is very easy.
 * Copyright (c) 2018 - 2025 Exploitox.
 *
 * Dive is licensed under MIT License (https://github.com/valnoxy/dive/blob/main/LICENSE).
 * So you are allowed to use freely and modify the application.
 * I will not be responsible for any outcome.
 * Proceed with any action at your own risk.
 *
 * Source code: https://github.com/valnoxy/dive
 */

using Dive.Core.Action.Capturing;
using Dive.Core.Action.Deployment;
using Dive.Core.Assets;
using Dive.Core.Common;
using Microsoft.Dism;
using Microsoft.Wim;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace Dive.Core
{
    public class Actions
    {
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr LoadLibrary(string lpFileName);

        /// <summary>
        /// Loads all native library into the current application domain.
        /// </summary>
        /// <remarks>This method must be called before invoking any functionality that depends on any native library.
        /// Calling this method multiple times has no additional effect if the library is already loaded.
        /// </remarks>
        /// <exception cref="Exception">Thrown if an error occurs.</exception>
        public static void Initialize()
        {
            var dllPath = Path.Combine(AppContext.BaseDirectory, "DiskMgr.dll");
            var handle = LoadLibrary(dllPath);

            if (handle == IntPtr.Zero)
            {
                throw new Exception($"Failed to load library: {dllPath}");
            }
        }

        /// <summary>
        /// Runs a command line tool and returns its exit code and combined output.
        /// The output is read before waiting for the exit, so a full pipe cannot block the tool.
        /// </summary>
        private static (int ExitCode, string Output) RunProcess(string fileName, string arguments)
        {
            using var process = new Process();
            process.StartInfo = new ProcessStartInfo(fileName, arguments)
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            process.Start();
            var errorTask = process.StandardError.ReadToEndAsync();
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            return (process.ExitCode, (output + Environment.NewLine + errorTask.Result).Trim());
        }

        /// <summary>
        /// Determines which bcdboot.exe to use for the given Windows installation.
        /// </summary>
        /// <remarks>
        /// Newer bcdboot versions (Windows 11 24H2+) switch to the "Windows UEFI CA 2023" boot files in
        /// Windows\Boot\EFI_EX when the firmware trusts that CA. Older images do not contain these files,
        /// so the host bcdboot fails ("Failed to validate boot manager checksum", 0xc1). For such images
        /// the bcdboot of the image itself is preferred - as long as it can run on this machine.
        /// The host bcdboot is always kept as fallback.
        /// </remarks>
        private static List<string> GetBcdbootCandidates(string windowsDir, BackgroundWorker worker)
        {
            const string hostBcdboot = "bcdboot.exe";
            var imageBcdboot = Path.Combine(windowsDir, "System32", "bcdboot.exe");
            var imageHasExBootFiles = Directory.Exists(Path.Combine(windowsDir, "Boot", "EFI_EX"));
            var imageBcdbootUsable = File.Exists(imageBcdboot) && IsRunnableOnHost(imageBcdboot);

            var candidates = new List<string>();
            if (!imageHasExBootFiles && imageBcdbootUsable)
                candidates.Add(imageBcdboot);
            candidates.Add(hostBcdboot);
            if (imageHasExBootFiles && imageBcdbootUsable)
                candidates.Add(imageBcdboot);

            worker?.ReportProgress(0, new ActionWorker
            {
                IsDebug = true,
                Message = $"bcdboot selection: EFI_EX in image: {imageHasExBootFiles}, image bcdboot usable: {imageBcdbootUsable} -> {string.Join(", ", candidates)}"
            });
            return candidates;
        }

        /// <summary>
        /// Checks whether the executable was built for the architecture of the running OS.
        /// </summary>
        private static bool IsRunnableOnHost(string executable)
        {
            try
            {
                using var stream = File.OpenRead(executable);
                using var reader = new BinaryReader(stream);
                stream.Seek(0x3C, SeekOrigin.Begin);
                var peOffset = reader.ReadInt32();
                stream.Seek(peOffset + 4, SeekOrigin.Begin);
                var machine = reader.ReadUInt16();

                return RuntimeInformation.OSArchitecture switch
                {
                    Architecture.X64 => machine is 0x8664 or 0x014C,          // x64, x86 (WOW64)
                    Architecture.X86 => machine is 0x014C,
                    Architecture.Arm64 => machine is 0xAA64 or 0x8664 or 0x014C, // ARM64 runs x64/x86 emulated
                    _ => false
                };
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Returns the last meaningful line of a tool output for use in an error message.
        /// </summary>
        private static string FormatToolOutput(string output)
        {
            if (string.IsNullOrWhiteSpace(output)) return string.Empty;
            var lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            return lines.Length == 0 ? string.Empty : $" {lines[^1]}";
        }

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
            worker?.ReportProgress(0, new ActionWorker
            {
                Action = Progress.PrepareDisk,
                IsError = false,
                IsIndeterminate = false,
                Message = "Partitioning disk ..."
            });

            // Validate the drive letters required by the selected layout
            static bool IsMissing(string letter) => string.IsNullOrEmpty(letter) || letter == "\0";

            if (IsMissing(windowsDrive))
                throw new DeploymentException(Progress.PrepareDisk, "No drive letter for the Windows partition was assigned.");
            if (partitionStyle is not Entities.PartitionStyle.Single && IsMissing(bootDrive))
                throw new DeploymentException(Progress.PrepareDisk, "No drive letter for the boot partition was assigned.");
            if (partitionStyle is Entities.PartitionStyle.Full or Entities.PartitionStyle.LegacyEfiWithRecovery && IsMissing(recoveryDrive))
                throw new DeploymentException(Progress.PrepareDisk, "No drive letter for the recovery partition was assigned.");

            // The legacy EFI layouts are only implemented with diskpart.
            var useNewDiskOperation = Entities.CanarySettings.UseNewDiskOperation
                                      && partitionStyle is not (Entities.PartitionStyle.LegacyEfi or Entities.PartitionStyle.LegacyEfiWithRecovery);

            if (useNewDiskOperation)
            {
                worker?.ReportProgress(0, new ActionWorker
                {
                    IsDebug = true,
                    Message = "Using new disk operation!"
                });
                switch (partitionStyle)
                {
                    case Entities.PartitionStyle.Full:
                        switch (firmware)
                        {
                            case Entities.Firmware.EFI:
                                var diskManagerGpt = new Action.DiskPreparation.GptDiskManager(disk);
                                diskManagerGpt.CreateFullLayout(bootDrive, windowsDrive, recoveryDrive, worker);
                                break;
                            case Entities.Firmware.BIOS:
                                var diskManagerMbr = new Action.DiskPreparation.MbrDiskManager(disk);
                                diskManagerMbr.CreateFullLayout(bootDrive, windowsDrive, recoveryDrive, worker);
                                break;
                            default:
                                throw new ArgumentOutOfRangeException(nameof(firmware), firmware, null);
                        }

                        break;
                    case Entities.PartitionStyle.Single:
                        switch (firmware)
                        {
                            case Entities.Firmware.EFI:
                                throw new DeploymentException(Progress.PrepareDisk, "You cannot use Single Partition layout on a system with EFI Firmware.");
                            case Entities.Firmware.BIOS:
                                var diskManagerMbr = new Action.DiskPreparation.MbrDiskManager(disk);
                                diskManagerMbr.CreateSinglePartition(windowsDrive, worker);
                                break;
                            default:
                                throw new ArgumentOutOfRangeException(nameof(firmware), firmware, null);
                        }
                        break;
                    case Entities.PartitionStyle.SeparateBoot:
                        switch (firmware)
                        {
                            case Entities.Firmware.EFI:
                                var diskManagerGpt = new Action.DiskPreparation.GptDiskManager(disk);
                                diskManagerGpt.CreateStandardLayout(bootDrive, windowsDrive, worker);
                                break;
                            case Entities.Firmware.BIOS:
                                var diskManagerMbr = new Action.DiskPreparation.MbrDiskManager(disk);
                                diskManagerMbr.CreateBootAndWindowsPartitions(bootDrive, windowsDrive, worker);
                                break;
                            default:
                                throw new ArgumentOutOfRangeException(nameof(firmware), firmware, null);
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(partitionStyle), partitionStyle, null);
                }

                worker?.ReportProgress(100, new ActionWorker
                {
                    Action = Progress.PrepareDisk,
                    IsError = false,
                    IsIndeterminate = false,
                    Message = "Done."
                });
                return;
            }

            // Reject unsupported combinations before diskpart is started
            if (partitionStyle == Entities.PartitionStyle.Single && firmware == Entities.Firmware.EFI)
                throw new DeploymentException(Progress.PrepareDisk, "You cannot use Single Partition layout on a system with EFI Firmware.");
            if (partitionStyle is Entities.PartitionStyle.LegacyEfi or Entities.PartitionStyle.LegacyEfiWithRecovery && firmware != Entities.Firmware.EFI)
                throw new DeploymentException(Progress.PrepareDisk, "You cannot use Legacy EFI layout on a system without EFI Firmware.");

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
                            throw new DeploymentException(Progress.PrepareDisk, "You cannot use Single Partition layout on a system with EFI Firmware.");
                    }
                    break;

                case Entities.PartitionStyle.LegacyEfi:
                case Entities.PartitionStyle.LegacyEfiWithRecovery:
                    switch (firmware)
                    {
                        case Entities.Firmware.BIOS: // Windows on BIOS
                            throw new DeploymentException(Progress.PrepareDisk, "You cannot use Legacy EFI layout on a system without EFI Firmware.");
                        case Entities.Firmware.EFI: // Windows on EFI
                            partDest.StandardInput.WriteLine("select disk " + disk);
                            partDest.StandardInput.WriteLine("clean");
                            partDest.StandardInput.WriteLine("create partition primary size=100");
                            partDest.StandardInput.WriteLine("format quick fs=fat32 label=System");
                            partDest.StandardInput.WriteLine("assign letter=" + bootDrive.Substring(0, 1));
                            partDest.StandardInput.WriteLine("create partition primary");
                            if (useRecovery) partDest.StandardInput.WriteLine("shrink minimum=800");
                            partDest.StandardInput.WriteLine("format quick fs=ntfs label=Windows");
                            partDest.StandardInput.WriteLine("active");
                            partDest.StandardInput.WriteLine("assign letter=" + windowsDrive.Substring(0, 1));
                            if (useRecovery)
                            {
                                partDest.StandardInput.WriteLine("create partition primary");
                                partDest.StandardInput.WriteLine("format quick fs=ntfs label=Recovery");
                                partDest.StandardInput.WriteLine("assign letter=" + recoveryDrive.Substring(0, 1));
                            }
                            partDest.StandardInput.WriteLine("exit");
                            break;
                    }
                    break;
            }
            partDest.StandardInput.Close();
            // Read the output before waiting, otherwise a full output buffer blocks diskpart.
            var diskpartOutput = partDest.StandardOutput.ReadToEnd();
            partDest.WaitForExit();

            if (partDest.ExitCode != 0)
            {
                worker?.ReportProgress(0, new ActionWorker
                {
                    IsDebug = true,
                    Message = "Diskpart output:\n" + diskpartOutput
                });
                throw new DeploymentException(Progress.PrepareDisk, "Diskpart terminated with exit code " + partDest.ExitCode + ".");
            }

            worker?.ReportProgress(100, new ActionWorker
            {
                Action = Progress.PrepareDisk,
                IsError = false,
                IsIndeterminate = false,
                Message = "Done."
            });
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
            worker?.ReportProgress(0, new ActionWorker
            {
                Action = Progress.ApplyImage,
                IsError = false,
                IsIndeterminate = false,
                Message = "Applying image ..."
            });

            // Checking if WIM File is an HTTP/S URL -> Download file to WINDOWS-DISK:\Dive\image.wim
            var isUrl = Uri.TryCreate(wimFile, UriKind.Absolute, out var uriResult)
                   && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
            if (isUrl)
            {
                // Download async
                var downloader = new FileDownloaderSync();
                var outputPath = Path.Combine(path, "Dive", uriResult.Segments[^1]); // Should be something like: C:\Dive\pro.wim
                Directory.CreateDirectory(Path.Combine(path, "Dive"));

                var progress = new Progress<int>(percentage =>
                {
                    worker?.ReportProgress(percentage, new ActionWorker
                    {
                        Action = Progress.ApplyImage,
                        IsError = false,
                        IsIndeterminate = false,
                        Message = $"Downloading Windows Image file ({percentage}%) ..."
                    });
                });

                try
                {
                    downloader.DownloadFile(wimFile, outputPath, progress);
                }
                catch (Exception ex) when (ex is not DeploymentException)
                {
                    throw new DeploymentException(Progress.ApplyImage, $"Failed to download Windows Image: {ex.Message}");
                }

                wimFile = outputPath;
            }

            try
            {
                Apply.WriteToDisk(wimFile, index, path, worker);
            }
            catch (Exception ex) when (ex is not DeploymentException)
            {
                throw new DeploymentException(Progress.ApplyImage, $"Failed to apply the Windows image: {ex.Message}", ex);
            }
            worker?.ReportProgress(100, new ActionWorker
            {
                Action = Progress.ApplyImage,
                IsError = false,
                IsIndeterminate = false,
                Message = "Done."
            });
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
            worker?.ReportProgress(0, new ActionWorker
            {
                Action = Progress.InstallBootloader,
                IsError = false,
                IsIndeterminate = true,
                Message = "Installing bootloader ..."
            });
            // Candidates in order of preference; the next one is only tried if the previous one fails.
            var candidates = new List<(string FileName, string Arguments)>();
            switch (bootloader)
            {
                case Entities.Bootloader.NTLDR:
                    candidates.Add(("bootsect.exe", $"/nt52 {bootloaderLetter[..2]} /force /mbr"));
                    break;

                case Entities.Bootloader.BOOTMGR:
                    {
                        var windowsDir = $"{windowsPath}Windows";
                        // bcdboot expects the system partition as "S:" (without trailing backslash)
                        var systemPartition = bootloaderLetter.TrimEnd('\\');
                        var firmwareType = firmware switch
                        {
                            Entities.Firmware.BIOS => "BIOS",
                            Entities.Firmware.EFI => "UEFI",
                            _ => throw new DeploymentException(Progress.InstallBootloader, $"Unsupported firmware type '{firmware}'.")
                        };
                        // /v: verbose output, logged if bcdboot fails
                        var arguments = $"{windowsDir} /s {systemPartition} /f {firmwareType} /v";

                        foreach (var bcdboot in GetBcdbootCandidates(windowsDir, worker))
                            candidates.Add((bcdboot, arguments));
                        break;
                    }

                default:
                    throw new DeploymentException(Progress.InstallBootloader, $"Unsupported bootloader '{bootloader}'.");
            }

            string lastError = null;
            Exception lastException = null;
            var succeeded = false;
            foreach (var (fileName, arguments) in candidates)
            {
                int exitCode;
                string output;
                try
                {
                    (exitCode, output) = RunProcess(fileName, arguments);
                }
                catch (Exception ex)
                {
                    lastError = $"Failed to start {fileName}: {ex.Message}";
                    lastException = ex;
                    worker?.ReportProgress(0, new ActionWorker { Action = Progress.InstallBootloader, IsWarning = true, Message = lastError });
                    continue;
                }

                worker?.ReportProgress(0, new ActionWorker
                {
                    IsDebug = true,
                    Message = $"{fileName} {arguments} -> exit code {exitCode}"
                });

                if (exitCode == 0)
                {
                    succeeded = true;
                    break;
                }

                lastError = $"{fileName} terminated with exit code {exitCode}.{FormatToolOutput(output)}";
                lastException = null;
                worker?.ReportProgress(0, new ActionWorker { IsDebug = true, Message = $"{fileName} output:\n{output}" });
                worker?.ReportProgress(0, new ActionWorker { Action = Progress.InstallBootloader, IsWarning = true, Message = lastError });
                Common.Logging.Log($"{fileName} {arguments} failed with exit code {exitCode}:\n{output}", Common.Logging.LogLevel.ERROR);
            }

            if (!succeeded)
                throw new DeploymentException(Progress.InstallBootloader, lastError ?? "No bootloader tool available.", lastException);

            worker?.ReportProgress(100, new ActionWorker
            {
                Action = Progress.InstallBootloader,
                IsError = false,
                IsIndeterminate = false,
                Message = "Done."
            });
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
            worker?.ReportProgress(0, new ActionWorker
            {
                Action = Progress.InstallRecovery,
                IsError = false,
                IsIndeterminate = true,
                Message = "Copying recovery image to partition ..."
            });

            // Create Recovery directory
            try
            {
                System.IO.Directory.CreateDirectory(System.IO.Path.Combine(recoveryLetter, "Recovery", "WindowsRE"));
            }
            catch (Exception ex) when (ex is not DeploymentException)
            {
                throw new DeploymentException(Progress.InstallRecovery, "Failed to create recovery directory.", ex);
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
                    throw new DeploymentException(Progress.InstallRecovery, "Recovery image does not exist in this image.");
                }
            }
            catch (Exception ex) when (ex is not DeploymentException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                throw new DeploymentException(Progress.InstallRecovery, "Failed to copy recovery image: " + ex.Message);
            }

            // Implement Dive
            if (implementDive)
            {
                try
                {
                    worker?.ReportProgress(0, new ActionWorker
                    {
                        Action = Progress.InstallRecovery,
                        IsError = false,
                        IsIndeterminate = true,
                        Message = "Implementing Dive into Windows Recovery image ..."
                    });

                    // Initialize Dism API
                    worker?.ReportProgress(0, new ActionWorker
                    {
                        IsDebug = true,
                        Message = "Initializing DISM API ..."
                    });
                    DismApi.Initialize(DismLogLevel.LogErrors);

                    // Prepare session
                    var imageFile = System.IO.Path.Combine(recoveryLetter, "Recovery", "WindowsRE", "Winre.wim");
                    var mountPath = System.IO.Path.Combine(windowsPath, "..", "dive-tmp", "mount");
                    var targetPath = System.IO.Path.Combine(mountPath, "sources", "recovery", "tools");
                    const int imageIndex = 1;
                    Directory.CreateDirectory(mountPath);

                    // Mount image
                    worker?.ReportProgress(0, new ActionWorker
                    {
                        IsDebug = true,
                        Message = "Mounting Recovery Image ..."
                    });
                    DismApi.MountImage(imageFile, mountPath, imageIndex);

                    // Copy Dive
                    worker?.ReportProgress(0, new ActionWorker
                    {
                        IsDebug = true,
                        Message = "Copying Dive into Recovery Image ..."
                    });
                    var strExeFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                    var strWorkPath = System.IO.Path.GetDirectoryName(strExeFilePath);
                    Common.FileIO.CopyFilesRecursively(new DirectoryInfo(strWorkPath), new DirectoryInfo(Path.Combine(targetPath, "Dive")));

                    // Create WinREConfig.xml
                    worker?.ReportProgress(0, new ActionWorker
                    {
                        IsDebug = true,
                        Message = "Creating and injecting WinREConfig.xml ..."
                    });
                    File.WriteAllText(Path.Combine(targetPath, "WinREConfig.xml"), WinRE.WinREConfig);

                    // Unmount image
                    worker?.ReportProgress(0, new ActionWorker
                    {
                        IsDebug = true,
                        Message = "Commiting changes to recovery image ..."
                    });
                    DismApi.UnmountImage(mountPath, true);
                }
                catch (Exception ex) when (ex is not DeploymentException)
                {
                    throw new DeploymentException(Progress.InstallRecovery, "Failed to inject Dive into Recovery image.", ex);
                }
                finally
                {
                    // Shutdown Dism API
                    worker?.ReportProgress(0, new ActionWorker
                    {
                        IsDebug = true,
                        Message = "Shutting down DISM API ..."
                    });
                    DismApi.Shutdown();
                }
            }

            // Register recovery partition
            try
            {
                worker?.ReportProgress(0, new ActionWorker
                {
                    Action = Progress.InstallRecovery,
                    IsError = false,
                    IsIndeterminate = true,
                    Message = "Registering Recovery image to Windows ..."
                });
                var reagentc = System.IO.Path.Combine(windowsPath, "System32", "Reagentc.exe");
                var (reExitCode, reOutput) = RunProcess(reagentc, $"/Setreimage /Path {recoveryLetter}\\Recovery\\WindowsRE /Target {windowsPath}");
                if (reExitCode != 0)
                {
                    worker?.ReportProgress(0, new ActionWorker
                    {
                        Action = Progress.InstallRecovery,
                        IsWarning = true,
                        Message = $"Reagentc /Setreimage terminated with exit code {reExitCode}.{FormatToolOutput(reOutput)}"
                    });
                }

                if (implementDive)
                {
                    worker?.ReportProgress(0, new ActionWorker
                    {
                        IsDebug = true,
                        Message = "Apply Diagnostics Tool configuration to recovery image ..."
                    });
                    var recoveryBootMenuPath = System.IO.Path.Combine(recoveryLetter, "Recovery", "BootMenu");
                    Directory.CreateDirectory(recoveryBootMenuPath);

                    File.WriteAllText(Path.Combine(recoveryBootMenuPath, "AddDiagnosticsToolToBootMenu.xml"), WinRE.AddDiagnosticsToolToBootMenu);
                    var (linkExitCode, linkOutput) = RunProcess(reagentc, $"/setbootshelllink /configfile {Path.Combine(recoveryBootMenuPath, "AddDiagnosticsToolToBootMenu.xml")}");
                    if (linkExitCode != 0)
                    {
                        worker?.ReportProgress(0, new ActionWorker
                        {
                            Action = Progress.InstallRecovery,
                            IsWarning = true,
                            Message = $"Reagentc /setbootshelllink terminated with exit code {linkExitCode}.{FormatToolOutput(linkOutput)}"
                        });
                    }
                }
            }
            catch (Exception ex) when (ex is not DeploymentException)
            {
                throw new DeploymentException(Progress.InstallRecovery, "Failed to register recovery image.", ex);
            }

            worker?.ReportProgress(100, new ActionWorker
            {
                Action = Progress.InstallRecovery,
                IsError = false,
                IsIndeterminate = false,
                Message = "Done."
            });
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
            worker?.ReportProgress(0, new ActionWorker
            {
                Action = Progress.InstallUnattend,
                IsError = false,
                IsIndeterminate = true,
                Message = "Installing Unattend configuration ..."
            });

            // Create Recovery directory
            try
            {
                System.IO.Directory.CreateDirectory(System.IO.Path.Combine(windowsPath, "Panther"));
            }
            catch (Exception ex) when (ex is not DeploymentException)
            {
                throw new DeploymentException(Progress.InstallUnattend, "Failed to create Panther directory.", ex);
            }

            // Write config to disk as unattend.xml
            try
            {
                System.IO.File.WriteAllText(System.IO.Path.Combine(windowsPath, "Panther", "unattend.xml"), configuration);
            }
            catch (Exception ex) when (ex is not DeploymentException)
            {
                throw new DeploymentException(Progress.InstallUnattend, "Failed to write Unattend configuration to disk.", ex);
            }

            // Copy OEM logo to Windows\System32 directory
            if (!string.IsNullOrEmpty(oemLogoPath))
            {
                try
                {
                    System.IO.File.Copy(oemLogoPath, Path.Combine(windowsPath, "System32", "logo.bmp"), true);
                }
                catch (Exception ex) when (ex is not DeploymentException)
                {
                    throw new DeploymentException(Progress.InstallUnattend, "Failed to copy OEM logo to disk.", ex);
                }
            }

            // Perform DISM Apply Unattend Task
            if (performDismApply)
            {
                try
                {
                    worker?.ReportProgress(0, new ActionWorker
                    {
                        Action = Progress.InstallUnattend,
                        IsError = false,
                        IsIndeterminate = true,
                        Message = "Registering Unattend configuration with DISM ..."
                    });
                    var f = new FileInfo(windowsPath);
                    var drive = Path.GetPathRoot(f.FullName);

                    var (dismExitCode, dismOutput) = RunProcess(
                        System.IO.Path.Combine(windowsPath, "System32", "Dism.exe"),
                        $"/Image:{drive} /Apply-Unattend:{System.IO.Path.Combine(windowsPath, "Panther", "unattend.xml")}");
                    if (dismExitCode != 0)
                    {
                        worker?.ReportProgress(0, new ActionWorker
                        {
                            Action = Progress.InstallUnattend,
                            IsWarning = true,
                            Message = $"DISM /Apply-Unattend terminated with exit code {dismExitCode}.{FormatToolOutput(dismOutput)}"
                        });
                    }
                }
                catch (Exception ex) when (ex is not DeploymentException)
                {
                    throw new DeploymentException(Progress.InstallUnattend, "Failed to apply Unattend configuration with DISM.", ex);
                }
            }

            worker?.ReportProgress(100, new ActionWorker
            {
                Action = Progress.InstallUnattend,
                IsError = false,
                IsIndeterminate = false,
                Message = "Done."
            });
        }

        /// <summary>
        /// Installs the specific driver to the Windows Installation (only Vista and higher).
        /// </summary>
        /// <param name="windowsDrive">Drive letter to the Windows disk</param>
        /// <param name="driverPath">List of driver paths</param>
        /// <param name="worker">Background worker for Graphical user interface</param>
        public static void InstallDriver(string windowsDrive, List<string> driverPath, BackgroundWorker worker = null)
        {
            worker?.ReportProgress(0, new ActionWorker
            {
                Action = Progress.InstallDrivers,
                IsError = false,
                IsIndeterminate = true,
                Message = "Installing drivers ..."
            });

            Apply.AddDriverToDisk(windowsDrive, driverPath, worker);

            worker?.ReportProgress(100, new ActionWorker
            {
                Action = Progress.InstallDrivers,
                IsError = false,
                IsIndeterminate = false,
                Message = "Done."
            });
        }

        /// <summary>
        /// Get information about a Windows deployment image (WIM).
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
            // Search for non-used drive letters
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

                case Entities.PartitionStyle.LegacyEfi when getDrives.Length >= 2:
                    arr.Add(getDrives[0]);
                    arr.Add(getDrives[1]);
                    return arr.ToArray();

                case Entities.PartitionStyle.Full when getDrives.Length >= 3:
                    arr.Add(getDrives[0]);
                    arr.Add(getDrives[1]);
                    arr.Add(getDrives[2]);
                    return arr.ToArray();

                case Entities.PartitionStyle.LegacyEfiWithRecovery when getDrives.Length >= 3:
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
            worker?.ReportProgress(0, new ActionWorker
            {
                Action = Progress.CaptureDisk,
                IsError = false,
                IsIndeterminate = false,
                Message = "Capturing disk ..."
            });

            Capture.CreateWim(name, description, pathToCapture, pathToImage, worker);
            worker?.ReportProgress(0, new ActionWorker
            {
                Action = Progress.CaptureDisk,
                IsError = false,
                IsIndeterminate = false,
                Message = "Done."
            });
        }
    }
}
