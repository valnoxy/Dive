using Dive.Core.Common;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Management;
using System.Runtime.InteropServices;

namespace Dive.Core.Action.DiskPreparation
{
    public class PrepareDisk
    {
        #region DiskOperations
        private const string DllPath = "Dive.DiskOperation.dll";

        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool CleanDisk(int diskNumber);

        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool ConvertToGPT(int diskNumber);

        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool CreateMBRPartition(int diskNumber, ulong sizeInMB, byte partitionType, char driveLetter);

        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool CreateGPTPartition(int diskNumber, ulong sizeInMB, Guid partitionType, char driveLetter);

        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool ChangePartitionType(int diskNumber, int partitionIndex, Guid partitionType);

        private static readonly Guid EfiPartitionType = new("c12a7328-f81f-11d2-ba4b-00a0c93ec93b"); // EFI System Partition
        private static readonly Guid MsrPartitionType = new("E3C9E316-0B5C-4DB8-817D-F92DF00215AE"); // MSR
        private static readonly Guid WindowsPartitionType = new("EBD0A0A2-B9E5-4433-87C0-68B6B72699C7"); // Windows
        private static readonly Guid RecoveryPartitionType = new("DE94BBA4-06D1-4D40-A16A-BFD50179D6AC"); // Recovery
        private static readonly byte NtfsType = 0x07; // NTFS (MBR)
        #endregion

        public class EFI
        {
            /// <summary>
            /// Prepares the disk for EFI Full Style (Windows + Recovery + Boot)
            /// </summary>
            /// <param name="diskIndex">Disk Identifier</param>
            /// <param name="windowsDrive">Drive letter of the Windows partition</param>
            /// <param name="bootDrive">Drive letter of the Boot partition</param>
            /// <param name="recoveryDrive">Drive letter of the Recovery partition</param>
            /// <param name="worker">Background worker for Graphical user interface</param>

            /// <returns>Result of disk preparation</returns>
            public static bool PrepareFull(int diskIndex, string bootDrive, string windowsDrive, string recoveryDrive, BackgroundWorker worker = null)
            {
                var sizeInBytes = GetDiskSize(diskIndex.ToString());
                if (sizeInBytes == 0)
                {
                    ReportError("Failed to get disk size.");
                    return false;
                }
                var sizeInMb = sizeInBytes / (1024 * 1024);
                const ulong efiSize = 300;
                const ulong msrSize = 16;
                const ulong recoverySize = 1024;
                var windowsSize = Convert.ToUInt64(sizeInMb - efiSize - msrSize - recoverySize);

                #region Disk Preparation

                ReportDebug("Cleaning disk ...");
                if (!CleanDisk(diskIndex))
                {
                    ReportError("Failed to clean up the disk.");
                    return false;
                }

                // Convert to GPT
                ReportDebug("Converting to GPT ...");
                if (!ConvertToGPT(diskIndex))
                {
                    ReportError("Failed to convert disk to GPT.");
                    return false;
                }

                #endregion

                #region EFI Partition
                // EFI
                ReportDebug("Creating EFI partition ...");
                if (!CreateGPTPartition(diskIndex, efiSize, WindowsPartitionType, Convert.ToChar(bootDrive.Substring(0, 1))))
                {
                    ReportError("Failed to create EFI partition.");
                    return false;
                }

                // Format EFI
                ReportDebug("Formatting EFI partition ...");
                if (!FormatPartition(bootDrive, "FAT32", true, "EFI"))
                {
                    ReportError("Failed to format EFI partition.");
                    return false;
                }

                // Set EFI ID
                ReportDebug("Changing Partition Type on EFI partition ...");
                if (!ChangePartitionType(diskIndex, 0, EfiPartitionType))
                {
                    ReportError("Failed to set Partition Type on EFI partition.");
                    return false;
                }
                #endregion

                #region MSR
                // MSR
                ReportDebug("Creating MSR partition ...");
                if (!CreateGPTPartition(diskIndex, msrSize, MsrPartitionType, '\0'))
                {
                    ReportError("Failed to create EFI partition.");
                    return false;
                }
                #endregion

                #region Windows Partition
                // Windows
                ReportDebug("Creating Windows partition ...");
                if (!CreateGPTPartition(diskIndex, windowsSize, WindowsPartitionType, Convert.ToChar(windowsDrive.Substring(0, 1))))
                {
                    ReportError("Failed to create Windows partition.");
                    return false;
                }

                // Format Windows
                ReportDebug("Formatting Windows partition ...");
                if (!FormatPartition(windowsDrive, "NTFS", true, "Windows"))
                {
                    ReportError("Failed to format Windows partition.");
                    return false;
                }
                #endregion

                #region Recovery
                // Recovery
                ReportDebug("Creating Recovery partition ...");
                if (!CreateGPTPartition(diskIndex, recoverySize, WindowsPartitionType, Convert.ToChar(recoveryDrive.Substring(0, 1))))
                {
                    ReportError("Failed to create Recovery partition.");
                    return false;
                }

                // Format Recovery
                ReportDebug("Formatting Recovery partition ...");
                if (!FormatPartition(recoveryDrive, "NTFS", true, "Recovery"))
                {
                    ReportError("Failed to format Recovery partition.");
                    return false;
                }

                // Set Recovery ID
                ReportDebug("Changing Partition Type on Recovery partition ...");
                if (!ChangePartitionType(diskIndex, 3, RecoveryPartitionType))
                {
                    ReportError("Failed to set Partition Type on Recovery partition.");
                    return false;
                }
                #endregion

                ReportDebug("Partitioning completed.");
                return true;
            }
        }

        private static void ReportDebug(string message, BackgroundWorker worker = null)
        {
            worker?.ReportProgress(0, JsonConvert.SerializeObject(new ActionWorker
            {
                Action = Progress.PrepareDisk,
                IsError = true,
                IsIndeterminate = false,
                Message = message
            }));
        }

        private static void ReportError(string message, BackgroundWorker worker = null)
        {
            worker?.ReportProgress(0, JsonConvert.SerializeObject(new ActionWorker
            {
                Action = Progress.PrepareDisk,
                IsError = true,
                IsIndeterminate = false,
                Message = message
            }));
        }

        private static double GetDiskSize(string diskId)
        {
            try
            {
                var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive");
                foreach (var info in searcher.Get())
                {
                    var deviceId = info["DeviceID"].ToString();
                    var size = Convert.ToDouble(info["Size"]);
                    if (deviceId != "\\\\.\\PHYSICALDRIVE" + diskId) continue;

                    return size;
                }
            }
            catch
            {
                // throw;
            }

            return 0;
        }

        private static bool FormatPartition(string driveLetter, string fileSystem, bool quickFormat, string label, BackgroundWorker worker = null)
        {
            try
            {
                var query = $"select * from Win32_Volume WHERE DriveLetter = \"{driveLetter}:\"";
                using var searcher = new ManagementObjectSearcher(query);
                var found = false;
                
                foreach (var o in searcher.Get())
                {
                    var volume = (ManagementObject)o;
                    found = true;
                    using var inParams = volume.GetMethodParameters("Format");
                    inParams["FileSystem"] = fileSystem;
                    inParams["QuickFormat"] = quickFormat;
                    inParams["Label"] = label;

                    using var outParams = volume.InvokeMethod("Format", inParams, null);

                    var returnValue = (uint)outParams["ReturnValue"];
                    if (returnValue != 0)
                    {
                        throw new InvalidOperationException($"Formatierung fehlgeschlagen, Fehlercode: {returnValue}");
                    }
                }

                if (!found)
                {
                    throw new InvalidOperationException($"Das Laufwerk {driveLetter} wurde nicht gefunden.");
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
