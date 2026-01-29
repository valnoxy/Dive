using Dive.Core.Common;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;

namespace Dive.Core.Action.USMT
{
    public class USMTAction
    {
        internal static BackgroundWorker Bw = null;
        internal static string LoadStateExe = "";
        internal static string ScanStateExe = "";
        internal static string MigHostExe = "";

        /// <summary>
        /// Prepares the current environment by extracting all necessary files.
        /// </summary>
        /// <param name="tempPath">Path to a temporary folder (should NOT be Temp itself!)</param>
        /// <param name="repositoryPath">Path to the repository</param>
        /// <param name="worker">UI Background worker (if exists)</param>
        /// <returns>True if the command executed successfully; otherwise, false.</returns>
        public static bool PrepareEnvironment(string tempPath, string repositoryPath, BackgroundWorker worker = null)
        {
            Bw = worker;

            worker?.ReportProgress(0, new ActionWorker
            {
                Action = Progress.PrepareUSMT,
                IsError = false,
                IsWarning = false,
                IsDebug = false,
                IsIndeterminate = true,
                Message = "Extracting USMT files ..."
            });

            if (Directory.Exists(tempPath))
                Directory.Delete(tempPath, true);

            if (!Directory.Exists(tempPath))
                Directory.CreateDirectory(tempPath!);
            if (!Directory.Exists(repositoryPath))
                Directory.CreateDirectory(repositoryPath!);

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
                worker?.ReportProgress(0, new ActionWorker
                {
                    Action = Progress.PrepareUSMT,
                    IsError = true,
                    IsWarning = false,
                    IsDebug = false,
                    IsIndeterminate = true,
                    Message = "File 'LoadState.exe' cannot be found."
                });
                return false;
            }
            if (!File.Exists(ScanStateExe))
            {
                worker?.ReportProgress(0, new ActionWorker
                {
                    Action = Progress.PrepareUSMT,
                    IsError = true,
                    IsWarning = false,
                    IsDebug = false,
                    IsIndeterminate = true,
                    Message = "File 'ScanState.exe' cannot be found."
                });
                return false;
            }
            if (!File.Exists(MigHostExe))
            {
                worker?.ReportProgress(0, new ActionWorker
                {
                    Action = Progress.PrepareUSMT,
                    IsError = true,
                    IsWarning = false,
                    IsDebug = false,
                    IsIndeterminate = true,
                    Message = "File 'MigHost.exe' cannot be found."
                });
                return false;
            }

            worker?.ReportProgress(0, new ActionWorker
            {
                Action = Progress.PrepareUSMT,
                IsError = false,
                IsWarning = false,
                IsDebug = true,
                IsIndeterminate = true,
                Message = "Done."
            });
            return true;
        }

        private static void ExtractAndUnzipResource(byte[] resourceData, string outputPath, string zipFileName)
        {
            var zipFilePath = Path.Combine(outputPath, zipFileName);
            File.WriteAllBytes(zipFilePath, resourceData);

            var extractPath = Path.Combine(outputPath, Path.GetFileNameWithoutExtension(zipFileName));
            ZipFile.ExtractToDirectory(zipFilePath, extractPath);
        }

        /// <summary>
        /// Executes ScanState.exe with the specified parameters and writes the output to the provided stream.
        /// </summary>
        /// <param name="scanStatePath">The full path to ScanState.exe.</param>
        /// <param name="repositoryPath">The path to the repository where the user profile data will be saved.</param>
        /// <param name="userList">The user SID to include in the scan.</param>
        /// <param name="outputWriter">Action delegate to handle command output.</param>
        /// <param name="worker">Optional BackgroundWorker for reporting progress.</param>
        /// <returns>True if the command executed successfully; otherwise, false.</returns>
        public static bool ScanState(string scanStatePath, string repositoryPath, string userList, Action<string> outputWriter, BackgroundWorker worker = null)
        {
            Bw = worker;

            worker?.ReportProgress(0, new ActionWorker
            {
                Action = Progress.PrepareUSMT,
                IsError = false,
                IsWarning = false,
                IsDebug = false,
                IsIndeterminate = true,
                Message = "Saving Profile Data ..."
            });

            try
            {
                using (var process = new Process())
                {
                    process.StartInfo.FileName = scanStatePath;
                    process.StartInfo.Arguments = $"\"{repositoryPath}\" /ue:*\\* /ui:{userList} /l:scanstate.log /config:Config_AppsAndSettings.xml /i:MigUser.xml /c /r:3 /o";
                    process.StartInfo.WorkingDirectory = Path.GetDirectoryName(scanStatePath)!;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.CreateNoWindow = true;
                    process.OutputDataReceived += (s, e) => { if (e.Data != null) outputWriter?.Invoke(e.Data); };
                    process.ErrorDataReceived += (s, e) => { if (e.Data != null) outputWriter?.Invoke("[ERROR] " + e.Data); };

                    process.Start();

                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    process.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                worker?.ReportProgress(0, new ActionWorker
                {
                    Action = Progress.PrepareUSMT,
                    IsError = true,
                    IsWarning = false,
                    IsDebug = false,
                    IsIndeterminate = false,
                    Message = $"ScanState failed: {ex.Message}"
                });

                return false;
            }

            worker?.ReportProgress(0, new ActionWorker
            {
                Action = Progress.PrepareUSMT,
                IsError = false,
                IsWarning = false,
                IsDebug = true,
                IsIndeterminate = true,
                Message = "Done."
            });

            return true;
        }
    }
}
