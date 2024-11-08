/*
 * Dive (formally deploya) - Deployment is very easy.
 * Copyright (c) 2018 - 2024 Exploitox.
 *
 * Component Name: DiskOperations
 *
 * Dive is licensed under MIT License (https://github.com/valnoxy/dive/blob/main/LICENSE).
 * So you are allowed to use freely and modify the application.
 * I will not be responsible for any outcome.
 * Proceed with any action at your own risk.
 *
 * Source code: https://github.com/valnoxy/dive
 */

#include "pch.h"
#include "PartitionFunctions.h"

#define IOCTL_DISK_GET_DRIVE_LAYOUT 0x00074000
#define IOCTL_DISK_SET_PARTITION_INFO 0x00074018
#define IOCTL_DISK_DELETE_DRIVE_LAYOUT 0x00074020

struct MBR {
    uint8_t bootCode[446]; // Bootloader Code
    struct PartitionEntry {
        uint8_t status;         // Partition Status (active or inactive)
        uint8_t startCHS[3];    // Start CHS (Cylinder, Head, Sector)
        uint8_t partitionType;  // Partition type (FAT32, NTFS, etc.)
        uint8_t endCHS[3];      // End CHS
        uint32_t startLBA;      // Start LBA (Logical Block Address)
        uint32_t totalSectors;
    } partition[4];

    uint16_t signature; // Signature 0xAA55
};


bool CheckMBRExists(HANDLE disk_handle) {
    uint8_t mbr[512];
    DWORD bytesRead;
    if (!ReadFile(disk_handle, mbr, sizeof(mbr), &bytesRead, NULL)) {
        DWORD error = GetLastError();
        std::wcerr << L"Error reading MBR. Error code: " << error << L"\n";
        return false;
    }

    if (mbr[510] == 0x55 && mbr[511] == 0xAA) {
        std::cout << "MBR exists and is valid." << '\n';
        return true;
    }
    return false;
}

bool InitializeMBR(HANDLE disk_handle) {
    MBR mbr = {};
    mbr.signature = 0xAA55; // MBR signature

    // Initialisiere alle Partitionseinträge auf 0
    for (auto& partition : mbr.partition) {
        partition.status = 0x00;
        partition.startLBA = 0;
        partition.totalSectors = 0;
        partition.partitionType = 0;
    }

    DWORD bytesWritten;
    if (!WriteFile(disk_handle, &mbr, sizeof(mbr), &bytesWritten, nullptr)) {
        DWORD error = GetLastError();
        std::wcerr << L"Error writing MBR. Error code: " << error << L"\n";
        return false;
    }

    std::cout << "MBR initialized successfully.\n";
    return true;
}

void CreatePartitionEntry(MBR::PartitionEntry& entry, uint32_t startLBA, uint32_t totalSectors, uint8_t partitionType) {
    entry.status = 0x80; // Active partition
    entry.startLBA = startLBA;
    entry.totalSectors = totalSectors;
    entry.partitionType = partitionType;
}

bool CreateMBRPartition(int disk_number, ULONGLONG size_in_mb, uint8_t partitionType, WCHAR driveLetter) {
    HANDLE disk_handle;
    if (!OpenDiskHandle(disk_number, disk_handle)) {
        std::cerr << "[DiskOperations] Error: Hard Disk could not be opened.\n";
        return false;
    }

    // Layout structure
    DWORD bytes_returned;
    DWORD layout_size = sizeof(DRIVE_LAYOUT_INFORMATION_EX) + (sizeof(PARTITION_INFORMATION_EX) * 128);
    auto layout_buffer = new BYTE[layout_size];

    // Retrieve the current disk layout
    if (!DeviceIoControl(disk_handle, IOCTL_DISK_GET_DRIVE_LAYOUT_EX,
        nullptr, 0,
        layout_buffer, layout_size,
        &bytes_returned, nullptr)) {
        DWORD error = GetLastError();
        std::cerr << "[DiskOperations] Error while retrieving the layout. Error code: " << error << std::endl;
        delete[] layout_buffer;
        CloseHandle(disk_handle);
        return false;
    }

    auto layout_info = reinterpret_cast<DRIVE_LAYOUT_INFORMATION_EX*>(layout_buffer);

    // Ensure there is space for more partitions
    if (layout_info->PartitionCount >= 128) {
        std::cerr << "[DiskOperations] Error: Maximum number of partitions reached.\n";
        delete[] layout_buffer;
        CloseHandle(disk_handle);
        return false;
    }

    // Initialize partition entry for MBR style
    PARTITION_INFORMATION_EX partition_info = {};
    partition_info.PartitionStyle = PARTITION_STYLE_MBR;  // Specify MBR style
    partition_info.Mbr.PartitionType = partitionType;
    partition_info.Mbr.BootIndicator = 0;  // Set to 0 if no bootable partition
    partition_info.Mbr.HiddenSectors = 0; // Set to 0 for default

    // Set the starting offset and partition length
    partition_info.StartingOffset.QuadPart = (layout_info->PartitionCount == 0)
        ? static_cast<LONGLONG>(1 * 1024 * 1024)  // Start 1 MB from the beginning
        : layout_info->PartitionEntry[layout_info->PartitionCount - 1].StartingOffset.QuadPart +
        layout_info->PartitionEntry[layout_info->PartitionCount - 1].PartitionLength.QuadPart + (1 * 1024 * 1024);  // 1 MB gap
    partition_info.PartitionLength.QuadPart = size_in_mb * 1024 * 1024;  // Convert MB to bytes

    // Add partition to layout
    layout_info->PartitionEntry[layout_info->PartitionCount] = partition_info;
    layout_info->PartitionCount++;

    // Write the updated layout back to the disk
    BOOL success = DeviceIoControl(
        disk_handle,
        IOCTL_DISK_SET_DRIVE_LAYOUT_EX,
        layout_info,
        sizeof(DRIVE_LAYOUT_INFORMATION_EX) + (layout_info->PartitionCount * sizeof(PARTITION_INFORMATION_EX)),
        nullptr,
        0,
        &bytes_returned,
        nullptr
    );

    // Cleanup
    delete[] layout_buffer;
    CloseHandle(disk_handle);

    // Check if partition creation succeeded
    if (!success) {
        DWORD error = GetLastError();
        std::cerr << "[DiskOperations] Error: Failed to create the MBR partition. Error code: " << error << std::endl;
        return false;
    }

    // If a drive letter is provided, assign it
    if (driveLetter != L'\0') {
        WCHAR partitionIdString[40];
        std::wstring volumePath = L"\\\\?\\Volume" + std::wstring(partitionIdString) + L"\\";
        std::wcout << L"[DiskOperations] Partition VolumePath: " << volumePath << L"\n";
        // AssignDriveLetter(volumePath, driveLetter); // Assuming you have this function to assign the drive letter
    }

    std::cout << "[DiskOperations] MBR Partition created successfully.\n";
    return true;
}
