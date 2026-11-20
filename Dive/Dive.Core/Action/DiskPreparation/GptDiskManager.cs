using Dive.Core.Common;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Dive.Core.Action.DiskPreparation
{
    public class GptDiskManager(int diskNumber)
    {
        private const string DllPath = "DiskMgr.dll";

        // Space not assigned to any partition: 1 MB in front of the first partition (alignment)
        // and 1 MB at the end of the disk (backup GPT header / rounding of the disk size).
        private const ulong ReservedSpaceMB = 2;

        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool DiskMgr_CleanDisk(int diskNumber);

        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool DiskMgr_InitializeGPT(int diskNumber);

        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool DiskMgr_CreateGPTPartition(int diskNumber, ulong sizeInMB, Guid partitionType);

        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool DiskMgr_SetGPTPartitionAttributes(int diskNumber, int partitionIndex, ulong attributes);

        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern ulong DiskMgr_GetDiskSize(int diskNumber);

        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool DiskMgr_ChangeGPTPartitionType(int diskNumber, int partitionIndex, Guid newPartitionType);

        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool DiskMgr_AssignDriveLetter(int diskNumber, int partitionIndex, char driveLetter);

        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private static extern int DiskMgr_FormatPartition(char driveLetter,
            [MarshalAs(UnmanagedType.LPWStr)] string fileSystem,
            [MarshalAs(UnmanagedType.LPWStr)] string label,
            bool quickFormat);

        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr DiskMgr_GetLastError();

        // GPT Partition Type GUIDs
        private static readonly Guid GUID_EFI_SYSTEM = new("c12a7328-f81f-11d2-ba4b-00a0c93ec93b");
        private static readonly Guid GUID_MSR = new("e3c9e316-0b5c-4db8-817d-f92df00215ae");
        private static readonly Guid GUID_BASIC_DATA = new("ebd0a0a2-b9e5-4433-87c0-68b6b72699c7");
        private static readonly Guid GUID_RECOVERY = new("de94bba4-06d1-4d40-a16a-bfd50179d6ac");

        private const ulong GPT_ATTR_NO_AUTO_MOUNT = 0x8000000000000000;
        private const ulong GPT_ATTR_REQUIRED_PARTITION = 0x0000000000000001;

        /// <summary>
        /// Creates a full GPT disk layout with EFI, MSR, Windows, and Recovery partitions, assigning the specified
        /// drive letters and formatting each partition as required.
        /// </summary>
        /// <remarks>This method cleans the target disk, initializes a GPT partition table, and creates
        /// the required partitions for a standard Windows installation. Each partition is formatted and assigned the
        /// specified drive letter. If any step fails, the method returns false and no further actions are performed.
        /// The method is not thread-safe and should be called from a context that ensures exclusive access to the
        /// target disk.</remarks>
        /// <param name="bootDrive">The drive letter to assign to the EFI partition. Must be a single uppercase letter (e.g., "S").</param>
        /// <param name="windowsDrive">The drive letter to assign to the Windows partition. Must be a single uppercase letter (e.g., "C").</param>
        /// <param name="recoveryDrive">The drive letter to assign to the Recovery partition. Must be a single uppercase letter (e.g., "R").</param>
        /// <param name="worker">An optional BackgroundWorker used to report progress and errors during the operation. Can be null if
        /// progress reporting is not required.</param>
        public void CreateFullLayout(string bootDrive, string windowsDrive, string recoveryDrive, BackgroundWorker worker = null)
        {
            ReportProgress(worker, "Cleaning disk...");
            if (!DiskMgr_CleanDisk(diskNumber))
                throw Fail("Failed to clean disk");

            ReportProgress(worker, "Initializing GPT...");
            if (!DiskMgr_InitializeGPT(diskNumber))
                throw Fail("Failed to initialize GPT");

            var diskSizeMB = DiskMgr_GetDiskSize(diskNumber);
            const ulong efiSize = 300;
            const ulong msrSize = 16;
            const ulong recoverySize = 1024;
            RequireDiskSpace(diskSizeMB, efiSize + msrSize + recoverySize);
            var windowsSize = diskSizeMB - efiSize - msrSize - recoverySize - ReservedSpaceMB;

            // EFI Partition
            ReportProgress(worker, "Creating EFI partition...");
            if (!DiskMgr_CreateGPTPartition(diskNumber, efiSize, GUID_BASIC_DATA))
                throw Fail("Failed to create EFI partition");

            ReportProgress(worker, "Assigning drive letter to EFI partition...");
            if (!DiskMgr_AssignDriveLetter(diskNumber, 0, bootDrive[0]))
                throw Fail("Failed to assign drive letter to EFI partition");

            ReportProgress(worker, "Formatting EFI partition...");
            var efiFormatSummary = DiskMgr_FormatPartition(bootDrive[0], "FAT32", "EFI", true);
            if (efiFormatSummary != 0)
                throw Fail("Failed to format EFI partition", efiFormatSummary);

            ReportProgress(worker, "Changing EFI partition type...");
            if (!DiskMgr_ChangeGPTPartitionType(diskNumber, 0, GUID_EFI_SYSTEM))
                throw Fail("Failed to change EFI partition type");

            // MSR Partition
            ReportProgress(worker, "Creating MSR partition...");
            if (!DiskMgr_CreateGPTPartition(diskNumber, msrSize, GUID_MSR))
                throw Fail("Failed to create MSR partition");

            // Windows Partition
            ReportProgress(worker, "Creating Windows partition...");
            if (!DiskMgr_CreateGPTPartition(diskNumber, windowsSize, GUID_BASIC_DATA))
                throw Fail("Failed to create Windows partition");

            ReportProgress(worker, "Assigning drive letter to Windows partition...");
            if (!DiskMgr_AssignDriveLetter(diskNumber, 2, windowsDrive[0]))
                throw Fail("Failed to assign drive letter to Windows partition");

            ReportProgress(worker, "Formatting Windows partition...");
            var windowsFormatSummary = DiskMgr_FormatPartition(windowsDrive[0], "NTFS", "Windows", true);
            if (windowsFormatSummary != 0)
                throw Fail("Failed to format Windows partition", windowsFormatSummary);

            // Recovery Partition
            ReportProgress(worker, "Creating Recovery partition...");
            if (!DiskMgr_CreateGPTPartition(diskNumber, recoverySize, GUID_BASIC_DATA))
                throw Fail("Failed to create Recovery partition");

            ReportProgress(worker, "Assigning drive letter to Recovery partition...");
            if (!DiskMgr_AssignDriveLetter(diskNumber, 3, recoveryDrive[0]))
                throw Fail("Failed to assign drive letter to Recovery partition");

            ReportProgress(worker, "Formatting Recovery partition...");
            var recoveryFormatSummary = DiskMgr_FormatPartition(recoveryDrive[0], "NTFS", "Recovery", true);
            if (recoveryFormatSummary != 0)
                throw Fail("Failed to format Recovery partition", recoveryFormatSummary);

            ReportProgress(worker, "Changing Recovery partition type...");
            if (!DiskMgr_ChangeGPTPartitionType(diskNumber, 3, GUID_RECOVERY))
                throw Fail("Failed to change Recovery partition type");

            // Some partition attributes
            ReportProgress(worker, "Setting partition attributes...");
            if (!DiskMgr_SetGPTPartitionAttributes(diskNumber, 3, GPT_ATTR_REQUIRED_PARTITION | GPT_ATTR_NO_AUTO_MOUNT)) // Recovery
                throw Fail("Failed to set Recovery partition attributes");

            if (!DiskMgr_SetGPTPartitionAttributes(diskNumber, 0, GPT_ATTR_REQUIRED_PARTITION | GPT_ATTR_NO_AUTO_MOUNT)) // EFI
                throw Fail("Failed to set EFI partition attributes");

            ReportProgress(worker, "Layout created successfully");
        }

        /// <summary>
        /// Creates a standard GPT disk layout with EFI, MSR, and Windows partitions, formatting and assigning drive
        /// letters as specified.
        /// </summary>
        /// <remarks>This method cleans the target disk, initializes it with a GPT partition table, and
        /// creates three partitions: EFI (FAT32), MSR, and Windows (NTFS). The specified drive letters are assigned to
        /// the EFI and Windows partitions. If any step fails, the method returns false and no further actions are
        /// performed. The method is not thread-safe.</remarks>
        /// <param name="bootDrive">The drive letter to assign to the EFI system partition. Must be a single uppercase letter (e.g., "S").</param>
        /// <param name="windowsDrive">The drive letter to assign to the Windows partition. Must be a single uppercase letter (e.g., "W").</param>
        /// <param name="worker">An optional BackgroundWorker used to report progress and errors during the operation. If null, progress is
        /// not reported.</param>
        public void CreateStandardLayout(string bootDrive, string windowsDrive, BackgroundWorker worker = null)
        {
            ReportProgress(worker, "Cleaning disk...");
            if (!DiskMgr_CleanDisk(diskNumber))
                throw Fail("Failed to clean disk");

            ReportProgress(worker, "Initializing GPT...");
            if (!DiskMgr_InitializeGPT(diskNumber))
                throw Fail("Failed to initialize GPT");

            var diskSizeMB = DiskMgr_GetDiskSize(diskNumber);
            const ulong efiSize = 300;
            const ulong msrSize = 16;
            RequireDiskSpace(diskSizeMB, efiSize + msrSize);
            var windowsSize = diskSizeMB - efiSize - msrSize - ReservedSpaceMB;

            // EFI Partition
            ReportProgress(worker, "Creating EFI partition...");
            if (!DiskMgr_CreateGPTPartition(diskNumber, efiSize, GUID_EFI_SYSTEM))
                throw Fail("Failed to create EFI partition");

            ReportProgress(worker, "Assigning drive letter to EFI partition...");
            if (!DiskMgr_AssignDriveLetter(diskNumber, 0, bootDrive[0]))
                throw Fail("Failed to assign drive letter to EFI partition");

            ReportProgress(worker, "Formatting EFI partition...");
            var efiFormatSummary = DiskMgr_FormatPartition(bootDrive[0], "FAT32", "EFI", true);
            if (efiFormatSummary != 0)
                throw Fail("Failed to format EFI partition", efiFormatSummary);

            // MSR Partition
            ReportProgress(worker, "Creating MSR partition...");
            if (!DiskMgr_CreateGPTPartition(diskNumber, msrSize, GUID_MSR))
                throw Fail("Failed to create MSR partition");

            // Windows Partition
            ReportProgress(worker, "Creating Windows partition...");
            if (!DiskMgr_CreateGPTPartition(diskNumber, windowsSize, GUID_BASIC_DATA))
                throw Fail("Failed to create Windows partition");

            ReportProgress(worker, "Assigning drive letter to Windows partition...");
            if (!DiskMgr_AssignDriveLetter(diskNumber, 2, windowsDrive[0]))
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