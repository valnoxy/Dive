#include "pch.h"

#include <cwctype>
#include <windows.h>
#include <winternl.h>
#include <initguid.h>
#include <iostream>
#include <vds.h>
#include <stdio.h>

#pragma comment(lib, "ole32.lib")

#define SafeRelease(x) { if (NULL != x) { x->Release(); x = NULL; } }
#define SafeCoFree(x) { if (NULL != x) { CoTaskMemFree(x); x = NULL; } }
#define ObjectNameInformation (OBJECT_INFORMATION_CLASS)1

extern "C" __declspec(dllexport) HRESULT DiskMgr_FormatPartition(
    wchar_t driveLetter,
    const wchar_t* fileSystem,
    const wchar_t* label,
    bool quickFormat)
{
    if (!fileSystem || wcslen(fileSystem) == 0)
    {
        std::wcout << L"[DiskMgr] Invalid file system\n";
	    return E_INVALIDARG;
    }

    driveLetter = std::towupper(driveLetter);
    if (driveLetter < L'A' || driveLetter > L'Z')
    {
        std::wcout << L"[DiskMgr] Invalid device letter: " << driveLetter << "\n";
        return E_INVALIDARG;
    }
    std::wcout << L"[DiskMgr] Begin DiskMgr_FormatPartition for: " << driveLetter << "\n";

    HRESULT hResult;
    HRESULT asyncRes;
    ULONG ulFetchCount;

    // VDS Interfaces
    IVdsServiceLoader* pLoader = NULL;
    IVdsService* pService = NULL;
    IVdsSwProvider* pProvider = NULL;
    IVdsPack* pPack = NULL;
    IVdsVolume* pVolume = NULL;
    IVdsVolumeMF* pVolumeMF = NULL;
    IVdsVolumeMF2* pVolumeMF2 = NULL;
    IVdsAsync* pAsync = NULL;
    IUnknown* pUnknown = NULL;
    IEnumVdsObject* pEnumVdsSwProviders = NULL;
    IEnumVdsObject* pEnumVdsPacks = NULL;
    IEnumVdsObject* pEnumVolumes = NULL;

    VDS_ASYNC_OUTPUT asyncOut;
    wchar_t** accessPaths = NULL;
    wchar_t targetDrivePath[4];
    bool formatComplete = false;

    swprintf_s(targetDrivePath, L"%c:\\", driveLetter);

    // init
    hResult = CoInitializeEx(NULL, COINIT_APARTMENTTHREADED);
    if (hResult == RPC_E_CHANGED_MODE)
    {
        hResult = S_OK;
    }
    else if (FAILED(hResult))
    {
        std::wcout << L"[DiskMgr] Failed CoInitializeEx: " << hResult << "\n";
        return hResult;
    }

    // IVdsServiceLoader
    hResult = CoCreateInstance(CLSID_VdsLoader, NULL, CLSCTX_LOCAL_SERVER,
        IID_IVdsServiceLoader, (void**)&pLoader);
    if (FAILED(hResult))
    {
        std::wcout << L"[DiskMgr] Failed CoCreateInstance: " << hResult << "\n";
        goto cleanup;
    }

    hResult = pLoader->LoadService(NULL, &pService);
    SafeRelease(pLoader);
    if (FAILED(hResult))
    {
        std::wcout << L"[DiskMgr] Failed LoadService: " << hResult << "\n";
        goto cleanup;
    }

    hResult = pService->WaitForServiceReady();
    if (FAILED(hResult))
    {
        std::wcout << L"[DiskMgr] Failed WaitForServiceReady on pService: " << hResult << "\n";
	    goto cleanup;
    }

    // Update vds
    hResult = pService->Reenumerate();
    if (FAILED(hResult))
    {
        std::wcout << L"[DiskMgr] Failed ReEnumerate: " << hResult << "\n";
	    goto cleanup;
    }

    hResult = pService->Refresh();
    if (FAILED(hResult))
    {
        std::wcout << L"[DiskMgr] Failed Refresh on pService: " << hResult << "\n"; 
    	goto cleanup;
    }

    // request
    hResult = pService->QueryProviders(VDS_QUERY_SOFTWARE_PROVIDERS, &pEnumVdsSwProviders);
    if (FAILED(hResult))
    {
        std::wcout << L"[DiskMgr] Failed QueryProviders: " << hResult << "\n";
	    goto cleanup;
    }

    while ((hResult = pEnumVdsSwProviders->Next(1, &pUnknown, &ulFetchCount)) == S_OK)
    {
        hResult = pUnknown->QueryInterface(&pProvider);
        SafeRelease(pUnknown);
        if (FAILED(hResult))
        {
            std::wcout << L"[DiskMgr] Failed QueryInterface -> &pProvider: " << hResult << "\n";
            continue;
        }

        hResult = pProvider->QueryPacks(&pEnumVdsPacks);
        SafeRelease(pProvider);
        if (FAILED(hResult))
        {
            std::wcout << L"[DiskMgr] Failed QueryPacks -> &pEnumVdsPacks: " << hResult << "\n";
            continue;
        }

        while ((hResult = pEnumVdsPacks->Next(1, &pUnknown, &ulFetchCount)) == S_OK)
        {
            hResult = pUnknown->QueryInterface(&pPack);
            SafeRelease(pUnknown);
            if (FAILED(hResult))
            {
                std::wcout << L"[DiskMgr] Failed QueryInterface -> &pPack: " << hResult << "\n";
                continue;
            }

            hResult = pPack->QueryVolumes(&pEnumVolumes);
            SafeRelease(pPack);
            if (FAILED(hResult))
            {
                std::wcout << L"[DiskMgr] Failed QueryVolumes -> &pEnumVolumes: " << hResult << "\n";
                continue;
            }

            while ((hResult = pEnumVolumes->Next(1, &pUnknown, &ulFetchCount)) == S_OK)
            {
                hResult = pUnknown->QueryInterface(&pVolume);
                SafeRelease(pUnknown);
                if (FAILED(hResult))
                {
                    std::wcout << L"[DiskMgr] Failed QueryInterface -> &pVolume: " << hResult << "\n";
                    continue;
                }

                hResult = pVolume->QueryInterface(&pVolumeMF);
                SafeRelease(pVolume);
                if (FAILED(hResult))
                {
                    std::wcout << L"[DiskMgr] Failed QueryInterface -> &pVolumeMF: " << hResult << "\n";
                    continue;
                }

                hResult = pVolumeMF->QueryAccessPaths(&accessPaths, (PLONG)&ulFetchCount);
                if (FAILED(hResult))
                {
                    std::wcout << L"[DiskMgr] Failed QueryAccessPaths: " << hResult << "\n";
                    SafeRelease(pVolumeMF);
                    continue;
                }

                bool foundVolume = false;
                for (ULONG i = 0; i < ulFetchCount; i++)
                {
                    if (accessPaths[i] && wcslen(accessPaths[i]) >= 2)
                    {
                        wchar_t volLetter = towupper(accessPaths[i][0]);
                        if (volLetter == driveLetter)
                        {
                            foundVolume = true;
                            break;
                        }
                    }
                }

                SafeCoFree(accessPaths);
                accessPaths = NULL;

                if (!foundVolume)
                {
                    //std::wcout << L"[DiskMgr] Volume not found!\n";
                    SafeRelease(pVolumeMF);
                    continue;
                }

                hResult = pVolumeMF->QueryInterface(&pVolumeMF2);
                SafeRelease(pVolumeMF);
                if (FAILED(hResult))
                {
                    std::wcout << L"[DiskMgr] Failed QueryInterface -> &pVolumeMF2: " << hResult << "\n";
	                goto cleanup;
                }

                // prepare parameters
                std::wcout << L"[DiskMgr] Preparing format parameters\n";
                wchar_t fsNameBuffer[32];
                wchar_t fsLabelBuffer[32];
                wcscpy_s(fsNameBuffer, fileSystem);
                wcscpy_s(fsLabelBuffer, label ? label : L"");

                std::wcout << L"[DiskMgr] Formatting partition ...\n";
                hResult = pVolumeMF2->FormatEx(
                    fsNameBuffer,                // File system
                    0,                           // Revision
                    0,                           // Cluster size
                    fsLabelBuffer,               // Volume label
                    TRUE,                        // Force
                    quickFormat ? TRUE : FALSE,  // Quick Format
                    FALSE,                       // Enable Compression
                    &pAsync                      // Async 
                );

                if (FAILED(hResult))
                {
                    std::wcout << L"[DiskMgr] Format failed: "<< hResult <<"\n";
                    SafeRelease(pVolumeMF2);
                    goto cleanup;
                }

                // wait for completion
                asyncRes = pAsync->Wait(&hResult, &asyncOut);
                SafeRelease(pAsync);
                SafeRelease(pVolumeMF2);

                if (FAILED(asyncRes))
                {
                    std::wcout << L"[DiskMgr] Format failed (asyncRes): " << hResult << "\n";
                    hResult = asyncRes;
                    goto cleanup;
                }

                if (FAILED(hResult))
                {
                    std::wcout << L"[DiskMgr] Format failed (after async): " << hResult << "\n";
					goto cleanup;
                }

                formatComplete = true;
                std::wcout << L"[DiskMgr] Format completed! -> Result: " << hResult << "\n";
                goto cleanup;
            }

            SafeRelease(pEnumVolumes);
            pEnumVolumes = NULL;
        }

        SafeRelease(pEnumVdsPacks);
        pEnumVdsPacks = NULL;
    }

    hResult = VDS_E_OBJECT_NOT_FOUND;

cleanup:
    SafeCoFree(accessPaths);
    SafeRelease(pEnumVolumes);
    SafeRelease(pEnumVdsPacks);
    SafeRelease(pEnumVdsSwProviders);
    SafeRelease(pAsync);
    SafeRelease(pVolumeMF2);
    SafeRelease(pVolumeMF);
    SafeRelease(pVolume);
    SafeRelease(pPack);
    SafeRelease(pProvider);
    SafeRelease(pService);
    SafeRelease(pLoader);
    SafeRelease(pUnknown);

    CoUninitialize();

    if (formatComplete)
        return S_OK;

    return hResult;
}