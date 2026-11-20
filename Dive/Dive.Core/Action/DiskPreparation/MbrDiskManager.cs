using Dive.Core.Common;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Dive.Core.Action.DiskPreparation
{
    public class MbrDiskManager(int diskNumber)
    {
        private const string DllPath = "DiskMgr.dll";

        // Space not assigned to any partition: 1 MB in front of the first partition (alignment)
        // and 1 MB at the end of the disk (rounding of the disk size / safety margin).
        private const ulong ReservedSpaceMB = 2;

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

        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr DiskMgr_GetLastError();

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
        public void CreateSinglePartition(string windowsDrive, BackgroundWorker worker = null)
        {
            ReportProgress(worker, "Cleaning disk...");
            if (!DiskMgr_CleanDisk(diskNumber))
                throw Fail("Failed to clean disk");

            ReportProgress(worker, "Initializing MBR...");
            if (!DiskMgr_InitializeMBR(diskNumber))
                throw Fail("Failed to initialize MBR");

            var diskSizeMB = DiskMgr_GetDiskSize(diskNumber);
            RequireDiskSpace(diskSizeMB, 0);
            var partitionSize = diskSizeMB - ReservedSpaceMB;

            ReportProgress(worker, "Creating Windows partition...");
            if (!DiskMgr_CreateMBRPartition(diskNumber, partitionSize, MBR_NTFS, true))
                throw Fail("Failed to create Windows partition");

            ReportProgress(worker, "Formatting Windows partition...");
            var windowsFormatSummary = DiskMgr_FormatPartition(windowsDrive[0], "NTFS", "Windows", true);
            if (windowsFormatSummary != 0)
                throw Fail("Failed to format Windows partition", windowsFormatSummary);

            ReportProgress(worker, "Layout created successfully");
        }

        public void CreateFullLayout(string bootDrive, string windowsDrive, string recoveryDrive, BackgroundWorker worker)
        {
            ReportProgress(worker, "Cleaning disk...");
            if (!DiskMgr_CleanDisk(diskNumber))
                throw Fail("Failed to clean disk");

            ReportProgress(worker, "Initializing MBR...");
            if (!DiskMgr_InitializeMBR(diskNumber))
                throw Fail("Failed to initialize MBR");

            var diskSizeMB = DiskMgr_GetDiskSize(diskNumber);
            const ulong bootSize = 500; // Boot Partition
            const ulong recoverySize = 1024; // Recovery Partition
            RequireDiskSpace(diskSizeMB, bootSize + recoverySize);
            var windowsSize = diskSizeMB - bootSize - recoverySize - ReservedSpaceMB;

            // Boot Partition 
            ReportProgress(worker, "Creating Boot partition...");
            if (!DiskMgr_CreateMBRPartition(diskNumber, bootSize, MBR_NTFS, true))
                throw Fail("Failed to create Boot partition");

            ReportProgress(worker, "Assigning drive letter to Boot partition...");
            if (!DiskMgr_AssignDriveLetter(diskNumber, 0, bootDrive[0]))
                throw Fail("Failed to assign drive letter to Boot partition");

            ReportProgress(worker, "Formatting Boot partition...");
            var bootFormatSummary = DiskMgr_FormatPartition(bootDrive[0], "NTFS", "System", true);
            if (bootFormatSummary != 0)
                throw Fail("Failed to format Boot partition", bootFormatSummary);

            // Windows Partition
            ReportProgress(worker, "Creating Windows partition...");
            if (!DiskMgr_CreateMBRPartition(diskNumber, windowsSize, MBR_NTFS, false))
                throw Fail("Failed to create Windows partition");

            ReportProgress(worker, "Assigning drive letter to Windows partition...");
            if (!DiskMgr_AssignDriveLetter(diskNumber, 1, windowsDrive[0]))
                throw Fail("Failed to assign drive letter to Windows partition");

            ReportProgress(worker, "Formatting Windows partition...");
            var windowsFormatSummary = DiskMgr_FormatPartition(windowsDrive[0], "NTFS", "Windows", true);
            if (windowsFormatSummary != 0)
                throw Fail("Failed to format Windows partition", windowsFormatSummary);

            // Recovery Partition
            ReportProgress(worker, "Creating Recovery partition...");
            if (!DiskMgr_CreateMBRPartition(diskNumber, recoverySize, MBR_NTFS, false))
                throw Fail("Failed to create Recovery partition");

            ReportProgress(worker, "Assigning drive letter to Recovery partition...");
            if (!DiskMgr_AssignDriveLetter(diskNumber, 2, recoveryDrive[0]))
                throw Fail("Failed to assign drive letter to Recovery partition");

            ReportProgress(worker, "Formatting Recovery partition...");
            var recoveryFormatSummary = DiskMgr_FormatPartition(recoveryDrive[0], "NTFS", "Recovery", true);
            if (recoveryFormatSummary != 0)
                throw Fail("Failed to format Recovery partition", recoveryFormatSummary);

            ReportProgress(worker, "Layout created successfully");
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
        public void CreateBootAndWindowsPartitions(string bootDrive, string windowsDrive, BackgroundWorker worker = null)
        {
            ReportProgress(worker, "Cleaning disk...");
            if (!DiskMgr_CleanDisk(diskNumber))
                throw Fail("Failed to clean disk");

            ReportProgress(worker, "Initializing MBR...");
            if (!DiskMgr_InitializeMBR(diskNumber))
                throw Fail("Failed to initialize MBR");

            var diskSizeMB = DiskMgr_GetDiskSize(diskNumber);
            const ulong bootSize = 500; // 500 MB Boot Partition
            RequireDiskSpace(diskSizeMB, bootSize);
            var windowsSize = diskSizeMB - bootSize - ReservedSpaceMB;

            // Boot Partition 
            ReportProgress(worker, "Creating Boot partition...");
            if (!DiskMgr_CreateMBRPartition(diskNumber, bootSize, MBR_NTFS, true))
                throw Fail("Failed to create Boot partition");

            ReportProgress(worker, "Assigning drive letter to Boot partition...");
            if (!DiskMgr_AssignDriveLetter(diskNumber, 0, bootDrive[0]))
                throw Fail("Failed to assign drive letter to Boot partition");

            ReportProgress(worker, "Formatting Boot partition...");
            var bootFormatSummary = DiskMgr_FormatPartition(bootDrive[0], "NTFS", "System", true);
            if (bootFormatSummary != 0)
                throw Fail("Failed to format Boot partition", bootFormatSummary);

            // Windows Partition
            ReportProgress(worker, "Creating Windows partition...");
            if (!DiskMgr_CreateMBRPartition(diskNumber, windowsSize, MBR_NTFS, false))
                throw Fail("Failed to create Windows partition");

            ReportProgress(worker, "Assigning drive letter to Windows partition...");
            if (!DiskMgr_AssignDriveLetter(diskNumber, 1, windowsDrive[0]))
                throw Fail("Failed to assign drive letter to Windows partition");

            ReportProgress(worker, "Formatting Windows partition...");
            var windowsFormatSummary = DiskMgr_FormatPartition(windowsDrive[0], "NTFS", "Windows", true);
            if (windowsFormatSummary != 0)
                throw Fail("Failed to format Windows partition", windowsFormatSummary);

            ReportProgress(worker, "Layout created successfully");
        }

        /// <summary>
        /// Ensures the disk can hold the fixed-size partitions plus a minimal Windows partition.
        /// </summary>
        private static void RequireDiskSpace(ulong diskSizeMB, ulong fixedPartitionsMB)
        {
            if (diskSizeMB == 0)
                throw Fail("Failed to read disk size");

            const ulong minimumWindowsSizeMB = 1024;
            var requiredMB = fixedPartitionsMB + ReservedSpaceMB + minimumWindowsSizeMB;
            if (diskSizeMB < requiredMB)
                throw new DeploymentException(Progress.PrepareDisk,
                    $"The disk is too small ({diskSizeMB} MB). At least {requiredMB} MB are required.");
        }

        private static void ReportProgress(BackgroundWorker worker, string message)
        {
            worker?.ReportProgress(0, new ActionWorker
            {
                Action = Progress.PrepareDisk,
                IsIndeterminate = true,
                Message = message
            });
        }

        /// <summary>
        /// Builds the exception for a failed step, including the last error reported by DiskMgr.
        /// </summary>
        private static DeploymentException Fail(string message, int? hResult = null)
        {
            string nativeError = null;
            try
            {
                nativeError = Marshal.PtrToStringUni(DiskMgr_GetLastError());
            }
            catch
            {
                // ignored
            }

            if (hResult is not null)
                message += $" (HRESULT 0x{hResult.Value:X8})";
            else if (!string.IsNullOrWhiteSpace(nativeError))
                message += $": {nativeError}";

            return new DeploymentException(Progress.PrepareDisk, message);
        }
    }
}