/*
 * Dive (formally deploya) - Deployment is very easy.
 * Copyright (c) 2018 - 2024 Exploitox.
 *
 * Component Name: DiskOperation
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

#pragma comment(lib, "wbemuuid.lib")

std::string last_error_message;

extern "C" __declspec(dllexport) const char* GetLastErrorMessage() {
    return last_error_message.c_str();
}

void SetLastErrorMessage(const std::string& message) {
    last_error_message = message;
}

void DebugOutput(const std::string& message) {
    OutputDebugStringA(message.c_str());
}

bool AssignDriveLetter(const std::wstring& volume_path, const char drive_letter) {
	const std::wstring drive_letter_str = std::wstring(1, drive_letter) + L":\\";

    if (SetVolumeMountPoint(drive_letter_str.c_str(), volume_path.c_str())) {
        std::wcout << L"[DiskOperations.dll] Drive Letter " << drive_letter_str << L" assigned successfully." << '\n';
        return true;
    }
    std::cerr << L"[DiskOperations.dll] Failed to assign drive letter: " << GetLastError() << '\n';
    return false;
}

bool OpenDiskHandle(const int disk_number, HANDLE& disk_handle) {
	const std::string device_path = R"(\\.\PhysicalDrive)" + std::to_string(disk_number);
    disk_handle = CreateFileA(
        device_path.c_str(),
        GENERIC_READ | GENERIC_WRITE,
        FILE_SHARE_READ | FILE_SHARE_WRITE,
        nullptr,
        OPEN_EXISTING,
        0,
        nullptr
    );

    if (disk_handle == INVALID_HANDLE_VALUE) {
        DWORD error = GetLastError();
        std::wcerr << L"[DiskOperations.dll] Error: Failed to open disk. Error code: " << error << L"\n";
        return false;
    }

    return (disk_handle != INVALID_HANDLE_VALUE);
}

bool CleanDisk(const int disk_number) {
    HANDLE disk_handle;
    if (!OpenDiskHandle(disk_number, disk_handle)) {
        std::cerr << "[DiskOperations.dll] Error: Hard Disk could not be opened.\n";
        return false;
    }

    DWORD bytes_returned;
    const BOOL success = DeviceIoControl(
        disk_handle,
        IOCTL_DISK_DELETE_DRIVE_LAYOUT,
        nullptr,
        0,
        nullptr,
        0,
        &bytes_returned,
        nullptr
    );

    CloseHandle(disk_handle);

    if (!success) {
        std::cerr << "[DiskOperations.dll] Failed to clean hard disk.\n";
        return false;
    }

    std::cout << "[DiskOperations.dll] Hard Disk cleaned successfully.\n";
    return true;
}

bool ChangePartitionType(int disk_number, DWORD partition_index, GUID new_partition_type) {
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

    if (partition_index >= layout_info->PartitionCount) {
        std::cerr << "[DiskOperations.dll] Error: Partition index out of range.\n";
        delete[] layout_buffer;
        CloseHandle(disk_handle);
        return false;
    }

    PARTITION_INFORMATION_EX& partition_info = layout_info->PartitionEntry[partition_index]; // Partition basierend auf Index

    if (partition_info.PartitionStyle != PARTITION_STYLE_GPT) {
        std::cerr << "[DiskOperations.dll] Partition is not GPT style.\n";
        delete[] layout_buffer;
        CloseHandle(disk_handle);
        return false;
    }

    partition_info.Gpt.PartitionType = new_partition_type;

    // Update die Partitionstabelle
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

    if (!success) {
        std::cerr << "[DiskOperations.dll] Error: Failed to update partition type.\n";
        return false;
    }

    std::cout << "[DiskOperations.dll] Partition type successfully updated.\n";
    return true;
}