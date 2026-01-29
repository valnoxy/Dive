/*
 * Dive - Deployment is very easy.
 * Copyright (c) 2018 - 2026 Exploitox.
 *
 * Component Name: DiskMgr
 */

#include "pch.h"
#include "diskengine.h"

#include <iostream>
#include <string>
#include <combaseapi.h>

static std::wstring g_lastError;

void SetLastError(const std::wstring& error) {
    g_lastError = error;
    std::wcerr << L"[DiskMgr] Error: " << error << L"\n";
}

const wchar_t* DiskMgr_GetLastError() {
    return g_lastError.c_str();
}
	
bool DiskMgr_OpenDisk(int diskNumber, HANDLE* outHandle) {
    std::string devicePath = R"(\\.\PhysicalDrive)" + std::to_string(diskNumber);
    *outHandle = CreateFileA(
        devicePath.c_str(),
        GENERIC_READ | GENERIC_WRITE,
        FILE_SHARE_READ | FILE_SHARE_WRITE,
        nullptr,
        OPEN_EXISTING,
        0,
        nullptr
    );

    if (*outHandle == INVALID_HANDLE_VALUE) {
        SetLastError(L"Failed to open disk " + std::to_wstring(diskNumber));
        return false;
    }
    return true;
}

void DiskMgr_CloseDisk(HANDLE diskHandle) {
    if (diskHandle != INVALID_HANDLE_VALUE) {
        CloseHandle(diskHandle);
    }
}

bool DiskMgr_CleanDisk(int diskNumber) {
    HANDLE diskHandle;
    if (!DiskMgr_OpenDisk(diskNumber, &diskHandle)) {
        return false;
    }

    DWORD bytesReturned;
    BOOL success = DeviceIoControl(
        diskHandle,
        IOCTL_DISK_DELETE_DRIVE_LAYOUT,
        nullptr, 0,
        nullptr, 0,
        &bytesReturned,
        nullptr
    );

    DiskMgr_CloseDisk(diskHandle);

    if (!success) {
        SetLastError(L"Failed to clean disk");
        return false;
    }

    std::wcout << L"[DiskMgr] Disk cleaned successfully\n";
    return true;
}

bool DiskMgr_InitializeMBR(int diskNumber) {
    HANDLE diskHandle;
    if (!DiskMgr_OpenDisk(diskNumber, &diskHandle)) {
        return false;
    }

    DRIVE_LAYOUT_INFORMATION_EX layoutInfo = {};
    layoutInfo.PartitionStyle = PARTITION_STYLE_MBR;
    layoutInfo.PartitionCount = 0;
    layoutInfo.Mbr.Signature = (DWORD)time(nullptr);

    DWORD bytesReturned;
    BOOL success = DeviceIoControl(
        diskHandle,
        IOCTL_DISK_SET_DRIVE_LAYOUT_EX,
        &layoutInfo,
        sizeof(layoutInfo),
        nullptr, 0,
        &bytesReturned,
        nullptr
    );

    DiskMgr_CloseDisk(diskHandle);

    if (!success) {
        SetLastError(L"Failed to initialize MBR");
        return false;
    }

    std::wcout << L"[DiskMgr] MBR initialized successfully\n";
    return true;
}

bool DiskMgr_CreateMBRPartition(int diskNumber, unsigned long long sizeInMB, unsigned char partitionType, bool makeActive) {
    HANDLE diskHandle;
    if (!DiskMgr_OpenDisk(diskNumber, &diskHandle)) {
        return false;
    }

    DWORD layoutSize = sizeof(DRIVE_LAYOUT_INFORMATION_EX) + (sizeof(PARTITION_INFORMATION_EX) * 128);
    auto layoutBuffer = new BYTE[layoutSize];
    DWORD bytesReturned;

    if (!DeviceIoControl(diskHandle, IOCTL_DISK_GET_DRIVE_LAYOUT_EX,
        nullptr, 0, layoutBuffer, layoutSize, &bytesReturned, nullptr)) {
        SetLastError(L"Failed to get drive layout");
        delete[] layoutBuffer;
        DiskMgr_CloseDisk(diskHandle);
        return false;
    }

    auto layoutInfo = reinterpret_cast<DRIVE_LAYOUT_INFORMATION_EX*>(layoutBuffer);

    if (layoutInfo->PartitionCount >= 4) {
        SetLastError(L"Maximum MBR partition count reached");
        delete[] layoutBuffer;
        DiskMgr_CloseDisk(diskHandle);
        return false;
    }

    PARTITION_INFORMATION_EX partitionInfo = {};
    partitionInfo.PartitionStyle = PARTITION_STYLE_MBR;
    partitionInfo.Mbr.PartitionType = partitionType;
    partitionInfo.Mbr.BootIndicator = makeActive;
    partitionInfo.Mbr.RecognizedPartition = TRUE;
    partitionInfo.Mbr.HiddenSectors = 0;

    partitionInfo.StartingOffset.QuadPart = (layoutInfo->PartitionCount == 0)
        ? 1024 * 1024LL  // 1 MB offset
        : layoutInfo->PartitionEntry[layoutInfo->PartitionCount - 1].StartingOffset.QuadPart +
          layoutInfo->PartitionEntry[layoutInfo->PartitionCount - 1].PartitionLength.QuadPart + (1024 * 1024LL);

    partitionInfo.PartitionLength.QuadPart = sizeInMB * 1024 * 1024;
    partitionInfo.PartitionNumber = layoutInfo->PartitionCount + 1;

    layoutInfo->PartitionEntry[layoutInfo->PartitionCount] = partitionInfo;
    layoutInfo->PartitionCount++;

    BOOL success = DeviceIoControl(
        diskHandle,
        IOCTL_DISK_SET_DRIVE_LAYOUT_EX,
        layoutInfo,
        sizeof(DRIVE_LAYOUT_INFORMATION_EX) + (layoutInfo->PartitionCount * sizeof(PARTITION_INFORMATION_EX)),
        nullptr, 0,
        &bytesReturned,
        nullptr
    );

    delete[] layoutBuffer;
    DiskMgr_CloseDisk(diskHandle);

    if (!success) {
        SetLastError(L"Failed to create MBR partition");
        return false;
    }

    std::wcout << L"[DiskMgr] MBR partition created successfully\n";
    return true;
}

bool DiskMgr_SetMBRPartitionActive(int diskNumber, int partitionIndex) {
    HANDLE diskHandle;
    if (!DiskMgr_OpenDisk(diskNumber, &diskHandle)) {
        return false;
    }

    DWORD layoutSize = sizeof(DRIVE_LAYOUT_INFORMATION_EX) + (sizeof(PARTITION_INFORMATION_EX) * 128);
    auto layoutBuffer = new BYTE[layoutSize];
    DWORD bytesReturned;

    if (!DeviceIoControl(diskHandle, IOCTL_DISK_GET_DRIVE_LAYOUT_EX,
        nullptr, 0, layoutBuffer, layoutSize, &bytesReturned, nullptr)) {
        SetLastError(L"Failed to get drive layout");
        delete[] layoutBuffer;
        DiskMgr_CloseDisk(diskHandle);
        return false;
    }

    auto layoutInfo = reinterpret_cast<DRIVE_LAYOUT_INFORMATION_EX*>(layoutBuffer);

    if (partitionIndex >= (int)layoutInfo->PartitionCount) {
        SetLastError(L"Invalid partition index");
        delete[] layoutBuffer;
        DiskMgr_CloseDisk(diskHandle);
        return false;
    }

    // Set all partitions to inactive
    for (DWORD i = 0; i < layoutInfo->PartitionCount; i++) {
        layoutInfo->PartitionEntry[i].Mbr.BootIndicator = FALSE;
    }

    // Set target partition to active
    layoutInfo->PartitionEntry[partitionIndex].Mbr.BootIndicator = TRUE;

    BOOL success = DeviceIoControl(
        diskHandle,
        IOCTL_DISK_SET_DRIVE_LAYOUT_EX,
        layoutInfo,
        sizeof(DRIVE_LAYOUT_INFORMATION_EX) + (layoutInfo->PartitionCount * sizeof(PARTITION_INFORMATION_EX)),
        nullptr, 0,
        &bytesReturned,
        nullptr
    );

    delete[] layoutBuffer;
    DiskMgr_CloseDisk(diskHandle);

    if (!success) {
        SetLastError(L"Failed to set partition active");
        return false;
    }

    std::wcout << L"[DiskMgr] Partition set to active\n";
    return true;
}

bool DiskMgr_InitializeGPT(int diskNumber) {
    HANDLE diskHandle;
    if (!DiskMgr_OpenDisk(diskNumber, &diskHandle)) {
        return false;
    }

    DRIVE_LAYOUT_INFORMATION_EX layoutInfo = {};
    layoutInfo.PartitionStyle = PARTITION_STYLE_GPT;
    layoutInfo.PartitionCount = 0;
    
    GUID diskId;
    CoCreateGuid(&diskId);
    layoutInfo.Gpt.DiskId = diskId;
    layoutInfo.Gpt.MaxPartitionCount = 128;
    layoutInfo.Gpt.StartingUsableOffset.QuadPart = 1024 * 1024; // 1 MB

    DWORD bytesReturned;
    BOOL success = DeviceIoControl(
        diskHandle,
        IOCTL_DISK_SET_DRIVE_LAYOUT_EX,
        &layoutInfo,
        sizeof(layoutInfo),
        nullptr, 0,
        &bytesReturned,
        nullptr
    );

    DiskMgr_CloseDisk(diskHandle);

    if (!success) {
        SetLastError(L"Failed to initialize GPT");
        return false;
    }

    std::wcout << L"[DiskMgr] GPT initialized successfully\n";
    return true;
}

bool DiskMgr_CreateGPTPartition(int diskNumber, unsigned long long sizeInMB, GUID partitionType) {
    HANDLE diskHandle;
    if (!DiskMgr_OpenDisk(diskNumber, &diskHandle)) {
        return false;
    }

    DWORD layoutSize = sizeof(DRIVE_LAYOUT_INFORMATION_EX) + (sizeof(PARTITION_INFORMATION_EX) * 128);
    auto layoutBuffer = new BYTE[layoutSize];
    DWORD bytesReturned;

    if (!DeviceIoControl(diskHandle, IOCTL_DISK_GET_DRIVE_LAYOUT_EX,
        nullptr, 0, layoutBuffer, layoutSize, &bytesReturned, nullptr)) {
        SetLastError(L"Failed to get drive layout");
        delete[] layoutBuffer;
        DiskMgr_CloseDisk(diskHandle);
        return false;
    }

    auto layoutInfo = reinterpret_cast<DRIVE_LAYOUT_INFORMATION_EX*>(layoutBuffer);

    if (layoutInfo->PartitionCount >= 128) {
        SetLastError(L"Maximum GPT partition count reached");
        delete[] layoutBuffer;
        DiskMgr_CloseDisk(diskHandle);
        return false;
    }

    PARTITION_INFORMATION_EX partitionInfo = {};
    partitionInfo.PartitionStyle = PARTITION_STYLE_GPT;
    partitionInfo.Gpt.PartitionType = partitionType;
    
    GUID partitionId;
    CoCreateGuid(&partitionId);
    partitionInfo.Gpt.PartitionId = partitionId;
    partitionInfo.Gpt.Attributes = 0;
    wcscpy_s(partitionInfo.Gpt.Name, L"");

    partitionInfo.StartingOffset.QuadPart = (layoutInfo->PartitionCount == 0)
        ? 1024 * 1024LL  // 1 MB offset
        : layoutInfo->PartitionEntry[layoutInfo->PartitionCount - 1].StartingOffset.QuadPart +
          layoutInfo->PartitionEntry[layoutInfo->PartitionCount - 1].PartitionLength.QuadPart + (1024 * 1024LL);

    partitionInfo.PartitionLength.QuadPart = sizeInMB * 1024 * 1024;
    partitionInfo.PartitionNumber = layoutInfo->PartitionCount + 1;

    layoutInfo->PartitionEntry[layoutInfo->PartitionCount] = partitionInfo;
    layoutInfo->PartitionCount++;

    BOOL success = DeviceIoControl(
        diskHandle,
        IOCTL_DISK_SET_DRIVE_LAYOUT_EX,
        layoutInfo,
        sizeof(DRIVE_LAYOUT_INFORMATION_EX) + (layoutInfo->PartitionCount * sizeof(PARTITION_INFORMATION_EX)),
        nullptr, 0,
        &bytesReturned,
        nullptr
    );

    delete[] layoutBuffer;
    DiskMgr_CloseDisk(diskHandle);

    if (!success) {
        SetLastError(L"Failed to create GPT partition");
        return false;
    }

    std::wcout << L"[DiskMgr] GPT partition created successfully\n";
    return true;
}

bool DiskMgr_SetGPTPartitionAttributes(int diskNumber, int partitionIndex, unsigned long long attributes) {
    HANDLE diskHandle;
    if (!DiskMgr_OpenDisk(diskNumber, &diskHandle)) {
        return false;
    }

    DWORD layoutSize = sizeof(DRIVE_LAYOUT_INFORMATION_EX) + (sizeof(PARTITION_INFORMATION_EX) * 128);
    auto layoutBuffer = new BYTE[layoutSize];
    DWORD bytesReturned;

    if (!DeviceIoControl(diskHandle, IOCTL_DISK_GET_DRIVE_LAYOUT_EX,
        nullptr, 0, layoutBuffer, layoutSize, &bytesReturned, nullptr)) {
        SetLastError(L"Failed to get drive layout");
        delete[] layoutBuffer;
        DiskMgr_CloseDisk(diskHandle);
        return false;
    }

    auto layoutInfo = reinterpret_cast<DRIVE_LAYOUT_INFORMATION_EX*>(layoutBuffer);

    if (partitionIndex >= (int)layoutInfo->PartitionCount) {
        SetLastError(L"Invalid partition index");
        delete[] layoutBuffer;
        DiskMgr_CloseDisk(diskHandle);
        return false;
    }

    layoutInfo->PartitionEntry[partitionIndex].Gpt.Attributes = attributes;

    BOOL success = DeviceIoControl(
        diskHandle,
        IOCTL_DISK_SET_DRIVE_LAYOUT_EX,
        layoutInfo,
        sizeof(DRIVE_LAYOUT_INFORMATION_EX) + (layoutInfo->PartitionCount * sizeof(PARTITION_INFORMATION_EX)),
        nullptr, 0,
        &bytesReturned,
        nullptr
    );

    delete[] layoutBuffer;
    DiskMgr_CloseDisk(diskHandle);

    if (!success) {
        SetLastError(L"Failed to set partition attributes");
        return false;
    }

    std::wcout << L"[DiskMgr] Partition attributes set successfully\n";
    return true;
}

unsigned long long DiskMgr_GetDiskSize(int diskNumber) {
    HANDLE diskHandle;
    if (!DiskMgr_OpenDisk(diskNumber, &diskHandle)) {
        return 0;
    }

    DISK_GEOMETRY_EX diskGeometry = {};
    DWORD bytesReturned;

    if (!DeviceIoControl(diskHandle, IOCTL_DISK_GET_DRIVE_GEOMETRY_EX,
        nullptr, 0, &diskGeometry, sizeof(diskGeometry), &bytesReturned, nullptr)) {
        SetLastError(L"Failed to get disk geometry");
        DiskMgr_CloseDisk(diskHandle);
        return 0;
    }

    DiskMgr_CloseDisk(diskHandle);
    return diskGeometry.DiskSize.QuadPart / (1024 * 1024); // Return in MB
}

int DiskMgr_GetPartitionCount(int diskNumber) {
    HANDLE diskHandle;
    if (!DiskMgr_OpenDisk(diskNumber, &diskHandle)) {
        return -1;
    }

    DWORD layoutSize = sizeof(DRIVE_LAYOUT_INFORMATION_EX) + (sizeof(PARTITION_INFORMATION_EX) * 128);
    auto layoutBuffer = new BYTE[layoutSize];
    DWORD bytesReturned;

    if (!DeviceIoControl(diskHandle, IOCTL_DISK_GET_DRIVE_LAYOUT_EX,
        nullptr, 0, layoutBuffer, layoutSize, &bytesReturned, nullptr)) {
        delete[] layoutBuffer;
        DiskMgr_CloseDisk(diskHandle);
        return -1;
    }

    auto layoutInfo = reinterpret_cast<DRIVE_LAYOUT_INFORMATION_EX*>(layoutBuffer);
    int count = layoutInfo->PartitionCount;

    delete[] layoutBuffer;
    DiskMgr_CloseDisk(diskHandle);
    return count;
}

bool DiskMgr_AssignDriveLetter(int diskNumber, int partitionIndex, wchar_t driveLetter) {
    Sleep(1000);

    // Update disk information
    HANDLE diskHandle;
    if (!DiskMgr_OpenDisk(diskNumber, &diskHandle)) {
        return false;
    }

    DWORD bytesReturned;
    DeviceIoControl(diskHandle, IOCTL_DISK_UPDATE_PROPERTIES, nullptr, 0, nullptr, 0, &bytesReturned, nullptr);
    DiskMgr_CloseDisk(diskHandle);
    
    Sleep(1000);

    // Get the partition volume GUID
    HANDLE findHandle;
    wchar_t volumeName[MAX_PATH];
    findHandle = FindFirstVolumeW(volumeName, MAX_PATH);
    
    if (findHandle == INVALID_HANDLE_VALUE) {
        SetLastError(L"Failed to enumerate volumes");
        return false;
    }

    bool volumeFound = false;
    std::wstring targetVolumeName;

    do {
        // Remove trailing backslash for opening
        size_t len = wcslen(volumeName);
        if (len > 0 && volumeName[len - 1] == L'\\') {
            volumeName[len - 1] = L'\0';
        }

        // Open the volume
        HANDLE volumeHandle = CreateFileW(
            volumeName,
            GENERIC_READ,
            FILE_SHARE_READ | FILE_SHARE_WRITE,
            nullptr,
            OPEN_EXISTING,
            0,
            nullptr
        );

        if (volumeHandle != INVALID_HANDLE_VALUE) {
            // Get volume disk extents
            BYTE buffer[sizeof(VOLUME_DISK_EXTENTS) + sizeof(DISK_EXTENT) * 32];
            PVOLUME_DISK_EXTENTS extents = (PVOLUME_DISK_EXTENTS)buffer;

            if (DeviceIoControl(volumeHandle, IOCTL_VOLUME_GET_VOLUME_DISK_EXTENTS,
                nullptr, 0, extents, sizeof(buffer), &bytesReturned, nullptr)) {
                
                // Check if this volume matches our disk and partition
                for (DWORD i = 0; i < extents->NumberOfDiskExtents; i++) {
                    if (extents->Extents[i].DiskNumber == (DWORD)diskNumber) {
                        
                    	// Get partition layout to match offset
                        HANDLE diskHandle2;
                        if (DiskMgr_OpenDisk(diskNumber, &diskHandle2)) {
                            DWORD layoutSize = sizeof(DRIVE_LAYOUT_INFORMATION_EX) + (sizeof(PARTITION_INFORMATION_EX) * 128);
                            auto layoutBuffer = new BYTE[layoutSize];

                            if (DeviceIoControl(diskHandle2, IOCTL_DISK_GET_DRIVE_LAYOUT_EX,
                                nullptr, 0, layoutBuffer, layoutSize, &bytesReturned, nullptr)) {
                                
                                auto layoutInfo = reinterpret_cast<DRIVE_LAYOUT_INFORMATION_EX*>(layoutBuffer);
                                
                                if (partitionIndex < (int)layoutInfo->PartitionCount) {
                                    LONGLONG partitionOffset = layoutInfo->PartitionEntry[partitionIndex].StartingOffset.QuadPart;
                                    LONGLONG volumeOffset = extents->Extents[i].StartingOffset.QuadPart;
                                    
                                    // Match by offset (allow 1MB tolerance)
                                    if (llabs(volumeOffset - partitionOffset) < 1024 * 1024) {
                                        volumeFound = true;
                                        
                                    	// Restore trailing backslash
                                        if (len > 0 && volumeName[len - 1] != L'\\') {
                                            wcscat_s(volumeName, MAX_PATH, L"\\");
                                        }
                                        targetVolumeName = volumeName;
                                        delete[] layoutBuffer;
                                        DiskMgr_CloseDisk(diskHandle2);
                                        CloseHandle(volumeHandle);
                                        goto volume_found;
                                    }
                                }
                            }
                            delete[] layoutBuffer;
                            DiskMgr_CloseDisk(diskHandle2);
                        }
                    }
                }
            }
            CloseHandle(volumeHandle);
        }

        // Restore trailing backslash for next iteration
        len = wcslen(volumeName);
        if (len > 0 && volumeName[len - 1] != L'\\') {
            wcscat_s(volumeName, MAX_PATH, L"\\");
        }

    } while (FindNextVolumeW(findHandle, volumeName, MAX_PATH));

volume_found:
    FindVolumeClose(findHandle);

    if (!volumeFound) {
        SetLastError(L"Could not find volume for partition");
        return false;
    }

    // Remove any existing drive letter assignment for this volume
    wchar_t existingPaths[MAX_PATH];
    DWORD pathLength;
    if (GetVolumePathNamesForVolumeNameW(targetVolumeName.c_str(), existingPaths, MAX_PATH, &pathLength)) {
        wchar_t* path = existingPaths;
        while (*path) {
            if (wcslen(path) == 3 && path[1] == L':' && path[2] == L'\\') {
                // Remove existing drive letter
                wchar_t oldDrive[4];
                swprintf_s(oldDrive, L"%c:\\", path[0]);
                DeleteVolumeMountPointW(oldDrive);
            }
            path += wcslen(path) + 1;
        }
    }

    // Assign new drive letter using SetVolumeMountPoint (persistent and global)
    wchar_t mountPoint[4];
    swprintf_s(mountPoint, L"%c:\\", driveLetter);

    DeleteVolumeMountPointW(mountPoint);

    if (!SetVolumeMountPointW(mountPoint, targetVolumeName.c_str())) {
        DWORD error = GetLastError();
        
        // Fallback (fucking hack)
    	HKEY hKey;
        wchar_t regPath[MAX_PATH];
        swprintf_s(regPath, L"SYSTEM\\MountedDevices");
        
        if (RegOpenKeyExW(HKEY_LOCAL_MACHINE, regPath, 0, KEY_SET_VALUE, &hKey) == ERROR_SUCCESS) {
            wchar_t valueName[20];
            swprintf_s(valueName, L"\\DosDevices\\%c:", driveLetter);
            
            // Get volume unique ID
            wchar_t uniqueId[MAX_PATH];
            wcscpy_s(uniqueId, targetVolumeName.c_str());
            
            // Remove \\?\ prefix and trailing backslash
            wchar_t* idStart = uniqueId + 4; // Skip "\\?\"
            size_t idLen = wcslen(idStart);
            if (idLen > 0 && idStart[idLen - 1] == L'\\') {
                idStart[idLen - 1] = L'\0';
            }
            
            // Convert to binary format for registry
            BYTE binaryData[100];
            size_t dataSize = (wcslen(idStart) + 1) * sizeof(wchar_t);
            memcpy(binaryData, idStart, dataSize);
            
            RegSetValueExW(hKey, valueName, 0, REG_BINARY, binaryData, (DWORD)dataSize);
            RegCloseKey(hKey);
            
            // Broadcast change
            SendMessageTimeoutW(HWND_BROADCAST, WM_DEVICECHANGE, DBT_DEVICEARRIVAL, 0, 
                SMTO_ABORTIFHUNG, 5000, nullptr);
            
            std::wcout << L"[DiskMgr] Drive letter " << driveLetter << L": assigned successfully (via registry)\n";
            return true;
        }
        
        SetLastError(L"Failed to assign drive letter. Error code: " + std::to_wstring(error));
        return false;
    }

    // Broadcast device change to make it visible globally
    SendMessageTimeoutW(HWND_BROADCAST, WM_DEVICECHANGE, DBT_DEVICEARRIVAL, 0, 
        SMTO_ABORTIFHUNG, 5000, nullptr);

    std::wcout << L"[DiskMgr] Drive letter " << driveLetter << L": assigned successfully\n";
    return true;
}

bool DiskMgr_ChangeGPTPartitionType(int diskNumber, int partitionIndex, GUID newPartitionType) {
    HANDLE diskHandle;
    if (!DiskMgr_OpenDisk(diskNumber, &diskHandle)) {
        return false;
    }

    DWORD layoutSize = sizeof(DRIVE_LAYOUT_INFORMATION_EX) + (sizeof(PARTITION_INFORMATION_EX) * 128);
    auto layoutBuffer = new BYTE[layoutSize];
    DWORD bytesReturned;

    if (!DeviceIoControl(diskHandle, IOCTL_DISK_GET_DRIVE_LAYOUT_EX,
        nullptr, 0, layoutBuffer, layoutSize, &bytesReturned, nullptr)) {
        SetLastError(L"Failed to get drive layout");
        delete[] layoutBuffer;
        DiskMgr_CloseDisk(diskHandle);
        return false;
    }

    auto layoutInfo = reinterpret_cast<DRIVE_LAYOUT_INFORMATION_EX*>(layoutBuffer);

    if (partitionIndex >= (int)layoutInfo->PartitionCount) {
        SetLastError(L"Invalid partition index");
        delete[] layoutBuffer;
        DiskMgr_CloseDisk(diskHandle);
        return false;
    }

    // Check if partition is GPT style
    if (layoutInfo->PartitionEntry[partitionIndex].PartitionStyle != PARTITION_STYLE_GPT) {
        SetLastError(L"Partition is not GPT style");
        delete[] layoutBuffer;
        DiskMgr_CloseDisk(diskHandle);
        return false;
    }

    // Change partition type
    layoutInfo->PartitionEntry[partitionIndex].Gpt.PartitionType = newPartitionType;

    BOOL success = DeviceIoControl(
        diskHandle,
        IOCTL_DISK_SET_DRIVE_LAYOUT_EX,
        layoutInfo,
        sizeof(DRIVE_LAYOUT_INFORMATION_EX) + (layoutInfo->PartitionCount * sizeof(PARTITION_INFORMATION_EX)),
        nullptr, 0,
        &bytesReturned,
        nullptr
    );

    delete[] layoutBuffer;
    DiskMgr_CloseDisk(diskHandle);

    if (!success) {
        SetLastError(L"Failed to change partition type");
        return false;
    }

    std::wcout << L"[DiskMgr] Partition type changed successfully\n";
    return true;
}

BOOL APIENTRY DllMain(HMODULE hModule, DWORD ul_reason_for_call, LPVOID lpReserved) {
    switch (ul_reason_for_call) {
    case DLL_PROCESS_ATTACH:
        CoInitialize(nullptr);
        break;
    case DLL_PROCESS_DETACH:
        CoUninitialize();
        break;
    case DLL_THREAD_ATTACH:
    case DLL_THREAD_DETACH:
        break;
    }
    return TRUE;
}