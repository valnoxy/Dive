using System;
using System.ComponentModel;
using System.Management;
using System.Runtime.InteropServices;

namespace Dive.Core.Action.DiskPreparation
{
    [Obsolete("NOT TESTED")]
    public class MbrDiskManager
    {
        private const string DllPath = "Dive.DiskMgr.dll";

        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool DiskMgr_CleanDisk(int diskNumber);

        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool DiskMgr_InitializeMBR(int diskNumber);

        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool DiskMgr_CreateMBRPartition(int diskNumber, ulong sizeInMB, byte partitionType, bool makeActive);

        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool DiskMgr_SetMBRPartitionActive(int diskNumber, int partitionIndex);

        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern ulong DiskMgr_GetDiskSize(int diskNumber);

        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern int DiskMgr_GetPartitionCount(int diskNumber);

        private const byte MBR_NTFS = 0x07;
        private const byte MBR_FAT32 = 0x0B;

        private readonly int _diskNumber;

        public MbrDiskManager(int diskNumber)
        {
            _diskNumber = diskNumber;
        }

        /// <summary>
        /// Creates a single NTFS partition on the specified disk and formats it for Windows use.
        /// </summary>
        /// <remarks>This method cleans the target disk, initializes it with an MBR partition table,
        /// creates a single NTFS partition, and formats it for Windows. All existing data on the disk will be lost. Use
        /// caution when specifying the target disk and drive letter.</remarks>
        /// <param name="windowsDrive">The drive letter (e.g., "C:\") to assign to the new Windows partition. Must refer to a valid, available
        /// drive letter.</param>
        /// <param name="worker">An optional BackgroundWorker used to report progress and errors during the operation. If null, progress is
        /// not reported.</param>
        /// <returns>true if the partition was created and formatted successfully; otherwise, false.</returns>
        public bool CreateSinglePartition(string windowsDrive, BackgroundWorker worker = null)
        {
            ReportProgress(worker, "Cleaning disk...");
            if (!DiskMgr_CleanDisk(_diskNumber))
            {
                ReportError(worker, "Failed to clean disk");
                return false;
            }

            ReportProgress(worker, "Initializing MBR...");
            if (!DiskMgr_InitializeMBR(_diskNumber))
            {
                ReportError(worker, "Failed to initialize MBR");
                return false;
            }

            var diskSizeMB = DiskMgr_GetDiskSize(_diskNumber);
            var partitionSize = diskSizeMB - 2; // Reserve 2 MB

            ReportProgress(worker, "Creating Windows partition...");
            if (!DiskMgr_CreateMBRPartition(_diskNumber, partitionSize, MBR_NTFS, true))
            {
                ReportError(worker, "Failed to create Windows partition");
                return false;
            }

            ReportProgress(worker, "Formatting Windows partition...");
            if (!FormatPartition(windowsDrive, "NTFS", true, "Windows"))
            {
                ReportError(worker, "Failed to format Windows partition");
                return false;
            }

            ReportProgress(worker, "MBR Single Partition layout created successfully");
            return true;
        }

        /// <summary>
        /// Creates and formats a boot partition and a Windows partition on the target disk using the specified drive
        /// letters.
        /// </summary>
        /// <remarks>This method cleans the target disk, initializes it with an MBR partition table,
        /// creates a 500 MB NTFS boot partition and a Windows partition using the remaining space, formats both
        /// partitions, and sets the boot partition as active. All existing data on the disk will be lost. The method
        /// reports progress and errors through the specified BackgroundWorker, if provided.</remarks>
        /// <param name="bootDrive">The drive letter to assign to the boot partition. Must be a valid, available drive letter (for example,
        /// "E:").</param>
        /// <param name="windowsDrive">The drive letter to assign to the Windows partition. Must be a valid, available drive letter (for example,
        /// "C:").</param>
        /// <param name="worker">An optional BackgroundWorker instance used to report progress and errors. If null, progress is not reported.</param>
        /// <returns>true if the boot and Windows partitions are created and formatted successfully; otherwise, false.</returns>
        public bool CreateBootAndWindowsPartitions(string bootDrive, string windowsDrive, BackgroundWorker worker = null)
        {
            ReportProgress(worker, "Cleaning disk...");
            if (!DiskMgr_CleanDisk(_diskNumber))
            {
                ReportError(worker, "Failed to clean disk");
                return false;
            }

            ReportProgress(worker, "Initializing MBR...");
            if (!DiskMgr_InitializeMBR(_diskNumber))
            {
                ReportError(worker, "Failed to initialize MBR");
                return false;
            }

            var diskSizeMB = DiskMgr_GetDiskSize(_diskNumber);
            const ulong bootSize = 500; // 500 MB Boot-Partition
            var windowsSize = diskSizeMB - bootSize - 2; // Reserve 2 MB

            // Boot-Partition erstellen
            ReportProgress(worker, "Creating Boot partition...");
            if (!DiskMgr_CreateMBRPartition(_diskNumber, bootSize, MBR_NTFS, true))
            {
                ReportError(worker, "Failed to create Boot partition");
                return false;
            }

            ReportProgress(worker, "Formatting Boot partition...");
            if (!FormatPartition(bootDrive, "NTFS", true, "System"))
            {
                ReportError(worker, "Failed to format Boot partition");
                return false;
            }

            // Windows-Partition erstellen
            ReportProgress(worker, "Creating Windows partition...");
            if (!DiskMgr_CreateMBRPartition(_diskNumber, windowsSize, MBR_NTFS, false))
            {
                ReportError(worker, "Failed to create Windows partition");
                return false;
            }

            ReportProgress(worker, "Formatting Windows partition...");
            if (!FormatPartition(windowsDrive, "NTFS", true, "Windows"))
            {
                ReportError(worker, "Failed to format Windows partition");
                return false;
            }

            // Boot-Partition als aktiv setzen
            ReportProgress(worker, "Setting Boot partition as active...");
            if (!DiskMgr_SetMBRPartitionActive(_diskNumber, 0))
            {
                ReportError(worker, "Failed to set Boot partition as active");
                return false;
            }

            ReportProgress(worker, "MBR Boot + Windows layout created successfully");
            return true;
        }

        [Obsolete("Use DiskMgr_FormatPartition()")]
        private bool FormatPartition(string driveLetter, string fileSystem, bool quickFormat, string label)
        {
            try
            {
                var query = $"select * from Win32_Volume WHERE DriveLetter = \"{driveLetter}:\"";
                using var searcher = new ManagementObjectSearcher(query);

                foreach (var o in searcher.Get())
                {
                    var volume = (ManagementObject)o;
                    using var inParams = volume.GetMethodParameters("Format");
                    inParams["FileSystem"] = fileSystem;
                    inParams["QuickFormat"] = quickFormat;
                    inParams["Label"] = label;

                    using var outParams = volume.InvokeMethod("Format", inParams, null);
                    var returnValue = (uint)outParams["ReturnValue"];

                    return returnValue == 0;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        private void ReportProgress(BackgroundWorker worker, string message)
        {
            worker?.ReportProgress(0, message);
        }

        private void ReportError(BackgroundWorker worker, string message)
        {
            worker?.ReportProgress(-1, message);
        }
    }
}