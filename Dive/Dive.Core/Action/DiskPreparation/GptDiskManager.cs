using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Dive.Core.Action.DiskPreparation
{
    public class GptDiskManager(int diskNumber)
    {
        private const string DllPath = "Dive.DiskMgr.dll";

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
        /// <returns>true if the disk layout was created successfully; otherwise, false.</returns>
        public bool CreateFullLayout(string bootDrive, string windowsDrive, string recoveryDrive, BackgroundWorker worker = null)
        {
            ReportProgress(worker, "Cleaning disk...");
            if (!DiskMgr_CleanDisk(diskNumber))
            {
                ReportError(worker, "Failed to clean disk");
                return false;
            }

            ReportProgress(worker, "Initializing GPT...");
            if (!DiskMgr_InitializeGPT(diskNumber))
            {
                ReportError(worker, "Failed to initialize GPT");
                return false;
            }

            var diskSizeMB = DiskMgr_GetDiskSize(diskNumber);
            const ulong efiSize = 300;
            const ulong msrSize = 16;
            const ulong recoverySize = 1024;
            var windowsSize = diskSizeMB - efiSize - msrSize - recoverySize - 4; // little buffer

            // EFI Partition
            ReportProgress(worker, "Creating EFI partition...");
            if (!DiskMgr_CreateGPTPartition(diskNumber, efiSize, GUID_BASIC_DATA))
            {
                ReportError(worker, "Failed to create EFI partition");
                return false;
            }

            ReportProgress(worker, "Assigning drive letter to EFI partition...");
            if (!DiskMgr_AssignDriveLetter(diskNumber, 0, bootDrive[0]))
            {
                ReportError(worker, "Failed to assign drive letter to EFI partition");
                return false;
            }

            ReportProgress(worker, "Formatting EFI partition...");
            var efiFormatSummary = DiskMgr_FormatPartition(bootDrive[0], "FAT32", "EFI", true);
            if (efiFormatSummary != 0)
            {
                ReportError(worker, "Failed to format EFI partition");
                return false;
            }

            ReportProgress(worker, "Changing EFI partition type...");
            if (!DiskMgr_ChangeGPTPartitionType(diskNumber, 0, GUID_EFI_SYSTEM))
            {
                ReportError(worker, "Failed to change EFI partition type");
                return false;
            }

            // MSR Partition
            ReportProgress(worker, "Creating MSR partition...");
            if (!DiskMgr_CreateGPTPartition(diskNumber, msrSize, GUID_MSR))
            {
                ReportError(worker, "Failed to create MSR partition");
                return false;
            }

            // Windows Partition
            ReportProgress(worker, "Creating Windows partition...");
            if (!DiskMgr_CreateGPTPartition(diskNumber, windowsSize, GUID_BASIC_DATA))
            {
                ReportError(worker, "Failed to create Windows partition");
                return false;
            }

            ReportProgress(worker, "Assigning drive letter to Windows partition...");
            if (!DiskMgr_AssignDriveLetter(diskNumber, 2, windowsDrive[0]))
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
            if (!DiskMgr_CreateGPTPartition(diskNumber, recoverySize, GUID_BASIC_DATA))
            {
                ReportError(worker, "Failed to create Recovery partition");
                return false;
            }

            ReportProgress(worker, "Assigning drive letter to Recovery partition...");
            if (!DiskMgr_AssignDriveLetter(diskNumber, 3, recoveryDrive[0]))
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

            ReportProgress(worker, "Changing Recovery partition type...");
            if (!DiskMgr_ChangeGPTPartitionType(diskNumber, 3, GUID_RECOVERY))
            {
                ReportError(worker, "Failed to change Recovery partition type");
                return false;
            }

            // Some partition attributes
            ReportProgress(worker, "Setting partition attributes...");
            if (!DiskMgr_SetGPTPartitionAttributes(diskNumber, 3, GPT_ATTR_REQUIRED_PARTITION | GPT_ATTR_NO_AUTO_MOUNT)) // Recovery
            {
                ReportError(worker, "Failed to set Recovery partition attributes");
                return false;
            }

            if (!DiskMgr_SetGPTPartitionAttributes(diskNumber, 0, GPT_ATTR_REQUIRED_PARTITION | GPT_ATTR_NO_AUTO_MOUNT)) // EFI
            {
                ReportError(worker, "Failed to set EFI partition attributes");
                return false;
            }

            ReportProgress(worker, "Layout created successfully");
            return true;
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
        /// <returns>true if the standard layout is created successfully; otherwise, false.</returns>
        public bool CreateStandardLayout(string bootDrive, string windowsDrive, BackgroundWorker worker = null)
        {
            ReportProgress(worker, "Cleaning disk...");
            if (!DiskMgr_CleanDisk(diskNumber))
            {
                ReportError(worker, "Failed to clean disk");
                return false;
            }

            ReportProgress(worker, "Initializing GPT...");
            if (!DiskMgr_InitializeGPT(diskNumber))
            {
                ReportError(worker, "Failed to initialize GPT");
                return false;
            }

            var diskSizeMB = DiskMgr_GetDiskSize(diskNumber);
            const ulong efiSize = 300;
            const ulong msrSize = 16;
            var windowsSize = diskSizeMB - efiSize - msrSize - 4;

            // EFI Partition
            ReportProgress(worker, "Creating EFI partition...");
            if (!DiskMgr_CreateGPTPartition(diskNumber, efiSize, GUID_EFI_SYSTEM))
            {
                ReportError(worker, "Failed to create EFI partition");
                return false;
            }

            ReportProgress(worker, "Assigning drive letter to EFI partition...");
            if (!DiskMgr_AssignDriveLetter(diskNumber, 0, bootDrive[0]))
            {
                ReportError(worker, "Failed to assign drive letter to EFI partition");
                return false;
            }

            ReportProgress(worker, "Formatting EFI partition...");
            var efiFormatSummary = DiskMgr_FormatPartition(bootDrive[0], "FAT32", "EFI", true);
            if (efiFormatSummary != 0)
            {
                ReportError(worker, "Failed to format EFI partition");
                return false;
            }

            // MSR Partition
            ReportProgress(worker, "Creating MSR partition...");
            if (!DiskMgr_CreateGPTPartition(diskNumber, msrSize, GUID_MSR))
            {
                ReportError(worker, "Failed to create MSR partition");
                return false;
            }

            // Windows Partition
            ReportProgress(worker, "Creating Windows partition...");
            if (!DiskMgr_CreateGPTPartition(diskNumber, windowsSize, GUID_BASIC_DATA))
            {
                ReportError(worker, "Failed to create Windows partition");
                return false;
            }

            ReportProgress(worker, "Assigning drive letter to Windows partition...");
            if (!DiskMgr_AssignDriveLetter(diskNumber, 2, windowsDrive[0]))
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