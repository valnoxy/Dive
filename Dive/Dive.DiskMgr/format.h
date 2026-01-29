#ifndef DISKMGR_FORMATPARTITION_H
#define DISKMGR_FORMATPARTITION_H

#include <windows.h>

#ifdef __cplusplus
extern "C" {
#endif

    __declspec(dllexport) HRESULT DiskMgr_FormatPartition(
        wchar_t driveLetter,
        const wchar_t* fileSystem,
        const wchar_t* label,
        bool quickFormat
    );

#ifdef __cplusplus
}
#endif

#endif // DISKMGR_FORMATPARTITION_H