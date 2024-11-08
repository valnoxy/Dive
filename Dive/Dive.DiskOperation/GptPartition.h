#ifndef GPTPARTITION_H
#define GPTPARTITION_H

extern "C" __declspec(dllexport) bool ConvertToGPT(int disk_number);
extern "C" __declspec(dllexport) bool CreateGPTPartition(int disk_number, ULONGLONG size_in_mb, GUID partition_type, WCHAR drive_letter, bool is_gpt);

#endif  // GPTPARTITION_H
