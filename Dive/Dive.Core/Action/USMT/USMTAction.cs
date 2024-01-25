using Dive.Core.Common;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.IO.Compression;
using System.IO;
using System.Reflection;

namespace Dive.Core.Action.USMT
{
    internal class USMTAction
    {
        internal static BackgroundWorker Bw = null;
        internal static string LoadStateExe = "";
        internal static string ScanStateExe = "";
        internal static string MigHostExe = "";

        /// <summary>
        /// Prepares the current environment by extracting all necessary files.
        /// </summary>
        /// <param name="tempPath">Path to a temporary folder (should NOT be Temp itself!)</param>
        /// <param name="worker">UI Background worker (if exists)</param>
        /// <returns>Result as boolean (True = Success, False = Failure)</returns>
        internal static bool PrepareEnvironment(string tempPath, BackgroundWorker worker = null)
        {
            Bw = worker;

            worker?.ReportProgress(0, JsonConvert.SerializeObject(new ActionWorker
            {
                Action = Progress.PrepareUSMT,
                IsError = false,
                IsWarning = false,
                IsDebug = false,
                IsIndeterminate = true,
                Message = "Extracting USMT files ..."
            }));

            if (!Directory.Exists(tempPath))
                Directory.CreateDirectory(tempPath!);

            var usmtX86Data = Assets.USMT.USMT_x86;
            var usmtX64Data = Assets.USMT.USMT_x64;
            ExtractAndUnzipResource(usmtX86Data, tempPath, "USMT_x86.zip");
            ExtractAndUnzipResource(usmtX64Data, tempPath, "USMT_x64.zip");

            // Validate the existence of LoadState, ScanState & MigHost
            LoadStateExe = Path.Combine(tempPath, "LoadState.exe");
            LoadStateExe = Path.Combine(tempPath, "ScanState.exe");
            LoadStateExe = Path.Combine(tempPath, "MigHost.exe");

            if (!File.Exists(LoadStateExe))
            {
                worker?.ReportProgress(0, JsonConvert.SerializeObject(new ActionWorker
                {
                    Action = Progress.PrepareUSMT,
                    IsError = true,
                    IsWarning = false,
                    IsDebug = false,
                    IsIndeterminate = true,
                    Message = "File 'LoadState.exe' cannot be found."
                }));
                return false;
            }
            if (!File.Exists(ScanStateExe))
            {
                worker?.ReportProgress(0, JsonConvert.SerializeObject(new ActionWorker
                {
                    Action = Progress.PrepareUSMT,
                    IsError = true,
                    IsWarning = false,
                    IsDebug = false,
                    IsIndeterminate = true,
                    Message = "File 'ScabState.exe' cannot be found."
                }));
                return false;
            }
            if (!File.Exists(MigHostExe))
            {
                worker?.ReportProgress(0, JsonConvert.SerializeObject(new ActionWorker
                {
                    Action = Progress.PrepareUSMT,
                    IsError = true,
                    IsWarning = false,
                    IsDebug = false,
                    IsIndeterminate = true,
                    Message = "File 'MigHost.exe' cannot be found."
                }));
                return false;
            }

            worker?.ReportProgress(0, JsonConvert.SerializeObject(new ActionWorker
            {
                Action = Progress.PrepareUSMT,
                IsError = false,
                IsWarning = false,
                IsDebug = true,
                IsIndeterminate = true,
                Message = "Done."
            }));
            return true;
        }

        private static void ExtractAndUnzipResource(byte[] resourceData, string outputPath, string zipFileName)
        {
            var zipFilePath = Path.Combine(outputPath, zipFileName);
            File.WriteAllBytes(zipFilePath, resourceData);

            var extractPath = Path.Combine(outputPath, Path.GetFileNameWithoutExtension(zipFileName));
            ZipFile.ExtractToDirectory(zipFilePath, extractPath);
        }
    }
}
