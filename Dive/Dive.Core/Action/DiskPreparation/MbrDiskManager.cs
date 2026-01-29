using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Dive.Core.Action.DiskPreparation
{
    public class MbrDiskManager(int diskNumber)
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

        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool DiskMgr_AssignDriveLetter(int diskNumber, int partitionIndex, char driveLetter);

        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private static extern int DiskMgr_FormatPartition(char driveLetter,
            [MarshalAs(UnmanagedType.LPWStr)] string fileSystem,
            [MarshalAs(UnmanagedType.LPWStr)] string label,
            bool quickFormat);

        private const byte MBR_NTFS = 0x07;

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
            if (!DiskMgr_CleanDisk(diskNumber))
            {
                ReportError(worker, "Failed to clean disk");
                return false;
            }

            ReportProgress(worker, "Initializing MBR...");
            if (!DiskMgr_InitializeMBR(diskNumber))
            {
                ReportError(worker, "Failed to initialize MBR");
                return false;
            }

            var diskSizeMB = DiskMgr_GetDiskSize(diskNumber);
            var partitionSize = diskSizeMB - 4; // Reserve 4 MB

            ReportProgress(worker, "Creating Windows partition...");
            if (!DiskMgr_CreateMBRPartition(diskNumber, partitionSize, MBR_NTFS, true))
            {
                ReportError(worker, "Failed to create Windows partition");
                return false;
            }

            ReportProgress(worker, "Formatting Windows partition...");
            var windowsFormatSummary = DiskMgr_FormatPartition(windowsDrive[0], "NTFS", "Windows", true);
            if (windowsFormatSummary != 0)
            {
                ReportError(worker, "Failed to format Windows partition");
                return false;
            }

            ReportProgress(worker, "Layout created successfully");
            return true;
        }

        public bool CreateFullLayout(string bootDrive, string windowsDrive, string recoveryDrive, BackgroundWorker worker)
        {
            ReportProgress(worker, "Cleaning disk...");
            if (!DiskMgr_CleanDisk(diskNumber))
            {
                ReportError(worker, "Failed to clean disk");
                return false;
            }

            ReportProgress(worker, "Initializing MBR...");
            if (!DiskMgr_InitializeMBR(diskNumber))
            {
                ReportError(worker, "Failed to initialize MBR");
                return false;
            }

            var diskSizeMB = DiskMgr_GetDiskSize(diskNumber);
            const ulong bootSize = 500; // Boot Partition
            const ulong recoverySize = 1024; // Recovery Partition
            var windowsSize = diskSizeMB - bootSize - recoverySize - 4; // Reserve 4 MB

            // Boot Partition 
            ReportProgress(worker, "Creating Boot partition...");
            if (!DiskMgr_CreateMBRPartition(diskNumber, bootSize, MBR_NTFS, true))
            {
                ReportError(worker, "Failed to create Boot partition");
                return false;
            }

            ReportProgress(worker, "Assigning drive letter to Boot partition...");
            if (!DiskMgr_AssignDriveLetter(diskNumber, 0, bootDrive[0]))
            {
                ReportError(worker, "Failed to assign drive letter to Boot partition");
                return false;
            }

            ReportProgress(worker, "Formatting Boot partition...");
            var bootFormatSummary = DiskMgr_FormatPartition(bootDrive[0], "NTFS", "System", true);
            if (bootFormatSummary != 0)
            {
                ReportError(worker, "Failed to format Boot partition");
                return false;
            }

            // Windows Partition
            ReportProgress(worker, "Creating Windows partition...");
            if (!DiskMgr_CreateMBRPartition(diskNumber, windowsSize, MBR_NTFS, false))
            {
                ReportError(worker, "Failed to create Windows partition");
                return false;
            }

            ReportProgress(worker, "Assigning drive letter to Windows partition...");
            if (!DiskMgr_AssignDriveLetter(diskNumber, 1, windowsDrive[0]))
            {
                ReportError(worker, "Failed to assign drive letter to Windows partition");
                return false;
            }

            ReportProgress(worker, "Formatting Windows partition...");
            var windowsFormatSummary = DiskMgr_FormatPartition(windowsDrive[0], "NTFS", "Windows", true);
            if (windowsFormatSummary != 0)
            {
                ReportError(worker, "Failed to format Windows partition");
                return false;
            }

            // Recovery Partition
            ReportProgress(worker, "Creating Recovery partition...");
            if (!DiskMgr_CreateMBRPartition(diskNumber, recoverySize, MBR_NTFS, false))
            {
                ReportError(worker, "Failed to create Recovery partition");
                return false;
            }

            ReportProgress(worker, "Assigning drive letter to Recovery partition...");
            if (!DiskMgr_AssignDriveLetter(diskNumber, 2, recoveryDrive[0]))
            {
                ReportError(worker, "Failed to assign drive letter to Recovery partition");
                return false;
            }

            ReportProgress(worker, "Formatting Recovery partition...");
            var recoveryFormatSummary = DiskMgr_FormatPartition(recoveryDrive[0], "NTFS", "Recovery", true);
            if (recoveryFormatSummary != 0)
            {
                ReportError(worker, "Failed to format Recovery partition");
                return false;
            }

            ReportProgress(worker, "Layout created successfully");
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
            if (!DiskMgr_CleanDisk(diskNumber))
            {
                ReportError(worker, "Failed to clean disk");
                return false;
            }

            ReportProgress(worker, "Initializing MBR...");
            if (!DiskMgr_InitializeMBR(diskNumber))
            {
                ReportError(worker, "Failed to initialize MBR");
                return false;
            }

            var diskSizeMB = DiskMgr_GetDiskSize(diskNumber);
            const ulong bootSize = 500; // 500 MB Boot Partition
            var windowsSize = diskSizeMB - bootSize - 4; // Reserve 4 MB

            // Boot Partition 
            ReportProgress(worker, "Creating Boot partition...");
            if (!DiskMgr_CreateMBRPartition(diskNumber, bootSize, MBR_NTFS, true))
            {
                ReportError(worker, "Failed to create Boot partition");
                return false;
            }

            ReportProgress(worker, "Assigning drive letter to Boot partition...");
            if (!DiskMgr_AssignDriveLetter(diskNumber, 0, bootDrive[0]))
            {
                ReportError(worker, "Failed to assign drive letter to Boot partition");
                return false;
            }

            ReportProgress(worker, "Formatting Boot partition...");
            var bootFormatSummary = DiskMgr_FormatPartition(bootDrive[0], "NTFS", "System", true);
            if (bootFormatSummary != 0)
            {
                ReportError(worker, "Failed to format Boot partition");
                return false;
            }

            // Windows Partition
            ReportProgress(worker, "Creating Windows partition...");
            if (!DiskMgr_CreateMBRPartition(diskNumber, windowsSize, MBR_NTFS, false))
            {
                ReportError(worker, "Failed to create Windows partition");
                return false;
            }

            ReportProgress(worker, "Assigning drive letter to Windows partition...");
            if (!DiskMgr_AssignDriveLetter(diskNumber, 1, windowsDrive[0]))
            {
                ReportError(worker, "Failed to assign drive letter to Windows partition");
                return false;
            }

            ReportProgress(worker, "Formatting Windows partition...");
            var windowsFormatSummary = DiskMgr_FormatPartition(windowsDrive[0], "NTFS", "Windows", true);
            if (windowsFormatSummary != 0)
            {
                ReportError(worker, "Failed to format Windows partition");
                return false;
            }

            ReportProgress(worker, "Layout created successfully");
            return true;
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