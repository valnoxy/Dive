/*
 * Dive - Deployment is very easy.
 * Copyright (c) 2018 - 2026 Exploitox.
 *
 * Component Name: DiskMgr
 */

#pragma once

#include <windows.h>
#include <dbt.h>

#ifndef IOCTL_DISK_UPDATE_PROPERTIES
#define IOCTL_DISK_UPDATE_PROPERTIES CTL_CODE(IOCTL_DISK_BASE, 0x0050, METHOD_BUFFERED, FILE_ANY_ACCESS)
#endif

#define MBR_PARTITION_NTFS 0x07
#define MBR_PARTITION_FAT32 0x0B
#define MBR_PARTITION_ACTIVE 0x80

static const GUID GUID_EFI_SYSTEM = { 0xc12a7328, 0xf81f, 0x11d2, {0xba, 0x4b, 0x00, 0xa0, 0xc9, 0x3e, 0xc9, 0x3b} };
static const GUID GUID_MSR = { 0xe3c9e316, 0x0b5c, 0x4db8, {0x81, 0x7d, 0xf9, 0x2d, 0xf0, 0x02, 0x15, 0xae} };
static const GUID GUID_BASIC_DATA = { 0xebd0a0a2, 0xb9e5, 0x4433, {0x87, 0xc0, 0x68, 0xb6, 0xb7, 0x26, 0x99, 0xc7} };
static const GUID GUID_RECOVERY = { 0xde94bba4, 0x06d1, 0x4d40, {0xa1, 0x6a, 0xbf, 0xd5, 0x01, 0x79, 0xd6, 0xac} };

extern "C" {
    // Disk Management
    __declspec(dllexport) bool DiskMgr_OpenDisk(int diskNumber, HANDLE* outHandle);
    __declspec(dllexport) void DiskMgr_CloseDisk(HANDLE diskHandle);
    __declspec(dllexport) bool DiskMgr_CleanDisk(int diskNumber);
    
    // MBR Operations
    __declspec(dllexport) bool DiskMgr_InitializeMBR(int diskNumber);
    __declspec(dllexport) bool DiskMgr_CreateMBRPartition(int diskNumber, unsigned long long sizeInMB, unsigned char partitionType, bool makeActive);
    __declspec(dllexport) bool DiskMgr_SetMBRPartitionActive(int diskNumber, int partitionIndex);
    
    // GPT Operations
    __declspec(dllexport) bool DiskMgr_InitializeGPT(int diskNumber);
    __declspec(dllexport) bool DiskMgr_CreateGPTPartition(int diskNumber, unsigned long long sizeInMB, GUID partitionType);
    __declspec(dllexport) bool DiskMgr_SetGPTPartitionAttributes(int diskNumber, int partitionIndex, unsigned long long attributes);
    __declspec(dllexport) bool DiskMgr_ChangeGPTPartitionType(int diskNumber, int partitionIndex, GUID newPartitionType);
    
    // Utility Functions
    __declspec(dllexport) bool DiskMgr_FormatPartition(wchar_t driveLetter, const wchar_t* fileSystem, const wchar_t* label, bool quickFormat);
    __declspec(dllexport) bool DiskMgr_AssignDriveLetter(int diskNumber, int partitionIndex, wchar_t driveLetter);
    __declspec(dllexport) int DiskMgr_GetPartitionCount(int diskNumber);
    __declspec(dllexport) unsigned long long DiskMgr_GetDiskSize(int diskNumber);
    
    // Error Handling
    __declspec(dllexport) const wchar_t* DiskMgr_GetLastError();
}