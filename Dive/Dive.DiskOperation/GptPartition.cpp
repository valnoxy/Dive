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

constexpr GUID guid_null = { 0x00000000, 0x0000, 0x0000, {0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00} };

bool ConvertToGPT(const int disk_number) {
    HANDLE disk_handle;
    if (!OpenDiskHandle(disk_number, disk_handle)) {
        std::cerr << "[DiskOperations.dll] Error: Hard Disk could not be opened.\n";
        return false;
    }

    DRIVE_LAYOUT_INFORMATION_EX layout_info = { 0 };
    layout_info.PartitionStyle = PARTITION_STYLE_GPT;
    layout_info.Gpt.DiskId = guid_null; // Generate automatically
    layout_info.Gpt.MaxPartitionCount = 128;

    DWORD bytesReturned;
    BOOL success = DeviceIoControl(
        disk_handle,
        IOCTL_DISK_SET_DRIVE_LAYOUT_EX,
        &layout_info,
        sizeof(layout_info),
        nullptr,
        0,
        &bytesReturned,
        nullptr
    );

    CloseHandle(disk_handle);

    if (!success) {
        std::cerr << "[DiskOperations.dll] Failed to convert to GPT.\n";
        return false;
    }

    std::cout << "[DiskOperations.dll] Disk converted successfully to GPT.\n";
    return true;
}

bool CreateGPTPartition(int disk_number, ULONGLONG size_in_mb, GUID partition_type, WCHAR drive_letter) {
    HANDLE disk_handle;
    if (!OpenDiskHandle(disk_number, disk_handle)) {
        std::cerr << "[DiskOperations.dll] Error: Hard Disk could not be opened.\n";
        return false;
    }

    // Layout structure
    DWORD bytes_returned;
    DWORD layout_size = sizeof(DRIVE_LAYOUT_INFORMATION_EX) + (sizeof(PARTITION_INFORMATION_EX) * 128);
    auto layout_buffer = new BYTE[layout_size];

    if (!DeviceIoControl(disk_handle, IOCTL_DISK_GET_DRIVE_LAYOUT_EX,
        nullptr, 0,
        layout_buffer, layout_size,
        &bytes_returned, nullptr)) {
        std::cerr << "[DiskOperations.dll] Error while retrieving the layout.\n";
        delete[] layout_buffer;
        CloseHandle(disk_handle);
        return false;
    }

    auto layout_info = reinterpret_cast<DRIVE_LAYOUT_INFORMATION_EX*>(layout_buffer);

    if (layout_info->PartitionCount >= 128) {
        std::cerr << "[DiskOperations.dll] Error: Maximum number of partitions reached.\n";
        delete[] layout_buffer;
        CloseHandle(disk_handle);
        return false;
    }

    PARTITION_INFORMATION_EX partition_info = {};
    partition_info.PartitionStyle = PARTITION_STYLE_GPT;

    // Generate Partition ID GUID
    GUID partitionId;
    HRESULT hr = CoCreateGuid(&partitionId);
    if (FAILED(hr)) {
        std::cerr << "[DiskOperations.dll] Error: Failed to generate the partition ID.\n";
        delete[] layout_buffer;
        CloseHandle(disk_handle);
        return false;
    }
    partition_info.Gpt.PartitionId = partitionId;

    // Define partition start offset and size
    partition_info.StartingOffset.QuadPart = (layout_info->PartitionCount == 0)
        ? static_cast<long long>(1 * 1024) * 1024 // 1 MB for the first partition
        : layout_info->PartitionEntry[layout_info->PartitionCount - 1].StartingOffset.QuadPart +
        layout_info->PartitionEntry[layout_info->PartitionCount - 1].PartitionLength.QuadPart + (1 * 1024 * 1024); // 1 MB gap

    partition_info.PartitionLength.QuadPart = size_in_mb * 1024 * 1024;
    partition_info.Gpt.PartitionType = partition_type;
    partition_info.Gpt.Attributes = 0;

    layout_info->PartitionEntry[layout_info->PartitionCount] = partition_info;
    layout_info->PartitionCount++;

    // Create partition
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

    // some cleanup
    delete[] layout_buffer;
    CloseHandle(disk_handle);

    if (!success) {
        std::cerr << "[DiskOperations.dll] Error: Failed to create the partition.\n";
        return false;
    }

    // Assign letter if requested
    if (drive_letter != L'\0') {
        WCHAR partitionIdString[40];
        StringFromGUID2(partition_info.Gpt.PartitionId, partitionIdString, 40);

        // Debug infos
        std::wcout << L"[DiskOperations.dll] Partition ID GUID: " << partitionIdString << L"\n";
        std::wstring volumePath = L"\\\\?\\Volume" + std::wstring(partitionIdString) + L"\\";
        std::wcout << L"[DiskOperations.dll] Partition VolumePath: " << volumePath << L"\n";
        AssignDriveLetter(volumePath, drive_letter);
    }

    std::cout << "[DiskOperations.dll] Partition created successfully.\n";
    return true;
}
