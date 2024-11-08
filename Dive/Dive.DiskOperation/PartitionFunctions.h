#ifndef PARTITION_FUNCTIONS_H
#define PARTITION_FUNCTIONS_H

#include <windows.h>
#include <string>

extern "C" __declspec(dllexport) bool CleanDisk(int disk_number);
extern "C" __declspec(dllexport) bool ChangePartitionType(int disk_number, DWORD partition_index, GUID new_partition_type);
extern "C" __declspec(dllexport) ULONG GetDiskSize(const char* device_id);

extern "C" __declspec(dllexport) bool ConvertToGPT(int disk_number);
extern "C" __declspec(dllexport) bool CreateGPTPartition(int disk_number, ULONGLONG size_in_mb, GUID partition_type, WCHAR drive_letter);

extern "C" __declspec(dllexport) bool CreateMBRPartition(int disk_number, ULONGLONG size_in_mb, uint8_t partitionType, WCHAR driveLetter);

#endif  // PARTITION_FUNCTIONS_H