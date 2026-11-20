using Dive.Core.Common;
using Microsoft.Dism;
using Microsoft.Wim;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

namespace Dive.Core.Action.Deployment
{
    internal class Apply
    {
        internal static BackgroundWorker Bw;
        private static string _firstApplyError;

        internal static void WriteToDisk(string imagePath, int index, string drive, BackgroundWorker worker = null)
        {
            Bw = worker;
            _firstApplyError = null;

            var imageExtension = Path.GetExtension(imagePath);
            var option = imageExtension switch
            {
                ".wim" => WimCreateFileOptions.None,
                ".esd" => WimCreateFileOptions.Chunked, // 0x20000000
                _ => WimCreateFileOptions.None
            };

            using var file = WimgApi.CreateFile(imagePath, WimFileAccess.Read, WimCreationDisposition.OpenExisting, option, WimCompressionType.None);
            var tempDir = Path.Combine(Environment.GetEnvironmentVariable("TEMP")!, "Dive");
            Directory.CreateDirectory(tempDir);
            WimgApi.SetTemporaryPath(file, tempDir);
            WimgApi.RegisterMessageCallback(file, ApplyCallbackMethod);
            try
            {
                using var imageHandle = WimgApi.LoadImage(file, index);
                WimgApi.ApplyImage(imageHandle, drive, WimApplyImageOptions.None);
            }
            finally
            {
                WimgApi.UnregisterMessageCallback(file, ApplyCallbackMethod);
            }

            // Errors on single files are only reported through the callback.
            if (_firstApplyError != null)
                throw new DeploymentException(Progress.ApplyImage, $"Failed to apply the Windows image: {_firstApplyError}");
        }

        internal static void AddDriverToDisk(string windowsDrive, List<string> driverPath, BackgroundWorker worker = null)
        {
            Bw = worker;
            try
            {
                var driverCount = driverPath.Count;
                worker?.ReportProgress(0, new ActionWorker
                {
                    IsDebug = true,
                    Message = $"[Driver] Entering Injection process with {driverCount} drivers ..."
                });
                worker?.ReportProgress(0, new ActionWorker
                {
                    IsDebug = true,
                    Message = $"[Driver] Initialize DISM API"
                });
                DismApi.Initialize(DismLogLevel.LogErrors);
                worker?.ReportProgress(0, new ActionWorker
                {
                    IsDebug = true,
                    Message = $"[Driver] Open offline session ..."
                });

                DismSession session = null;
                try
                {
                    worker?.ReportProgress(0, new ActionWorker
                    {
                        Action = Progress.InstallDrivers,
                        IsError = false,
                        IsIndeterminate = true,
                        Message = "Opening Dism Session for Driver injection ..."
                    });
                    session = DismApi.OpenOfflineSession(windowsDrive);
                    worker?.ReportProgress(0, new ActionWorker
                    {
                        IsDebug = true,
                        Message = $"[Driver] Session opened."
                    });
                }
                catch (Exception ex) when (ex is not DeploymentException)
                {
                    worker?.ReportProgress(0, new ActionWorker
                    {
                        IsDebug = true,
                        Message = $"[Driver] Shutting down API ..."
                    });
                    DismApi.Shutdown();
                    throw new DeploymentException(Progress.InstallDrivers, "Failed to open the offline DISM session: " + ex.Message, ex);
                }

                var currentDriverCount = 0;
                foreach (var driver in driverPath)
                {
                    worker?.ReportProgress(0, new ActionWorker
                    {
                        IsDebug = true,
                        Message = $"[Driver] Begin driver installation ..."
                    });
                    try
                    {
                        currentDriverCount++;
                        worker?.ReportProgress(0, new ActionWorker
                        {
                            IsDebug = true,
                            Message = $"[Driver] Current Driver ({currentDriverCount}): {driver}"
                        });
                        worker?.ReportProgress(0, new ActionWorker
                        {
                            Action = Progress.InstallDrivers,
                            IsError = false,
                            IsIndeterminate = true,
                            Message = $"Injecting driver {currentDriverCount} of {driverCount} ..."
                        });
                        DismApi.AddDriver(session, driver, true);
                    }
                    catch (Exception ex) when (ex is not DeploymentException)
                    {
                        worker?.ReportProgress(0, new ActionWorker
                        {
                            Action = Progress.InstallDrivers,
                            IsWarning = true,
                            IsIndeterminate = false,
                            Message = $"Failed to inject driver {driver}: {ex.Message}"
                        });
                    }
                }

                worker?.ReportProgress(0, new ActionWorker
                {
                    IsDebug = true,
                    Message = $"[Driver] Closing session ..."
                });
                session.Close();
                worker?.ReportProgress(0, new ActionWorker
                {
                    IsDebug = true,
                    Message = $"[Driver] Shutting down DISM API ..."
                });
                DismApi.Shutdown();
            }
            catch (Exception ex) when (ex is not DeploymentException)
            {
                throw new DeploymentException(Progress.InstallDrivers, "Failed to install drivers: " + ex.Message, ex);
            }
        }

        private static WimMessageResult ApplyCallbackMethod(WimMessageType messageType, object message, object userData)
        {
            switch (messageType)
            {
                case WimMessageType.Progress:
                    var wimMessageProgress = (WimMessageProgress)message;
                    var estimatedTime = new TimeSpan(
                        wimMessageProgress.EstimatedTimeRemaining.Days,
                        wimMessageProgress.EstimatedTimeRemaining.Hours,
                        wimMessageProgress.EstimatedTimeRemaining.Minutes,
                        wimMessageProgress.EstimatedTimeRemaining.Seconds);
                    Bw?.ReportProgress(wimMessageProgress.PercentComplete, new ActionWorker
                    {
                        Action = Progress.ApplyImage,
                        IsError = false,
                        IsIndeterminate = false,
                        Message = $"Applying image ({wimMessageProgress.PercentComplete}% - {estimatedTime})..."
                    });
                    break;

                case WimMessageType.Error:
                    var wimMessageError = (WimMessageError)message;
                    var errorText = $"{wimMessageError.Path} (Win32 error {wimMessageError.Win32ErrorCode})";
                    _firstApplyError ??= errorText;
                    Bw?.ReportProgress(0, new ActionWorker
                    {
                        Action = Progress.ApplyImage,
                        IsWarning = true,
                        Message = $"Error while applying file: {errorText}"
                    });
                    break;

                case WimMessageType.Warning:
                    var wimMessageWarning = (WimMessageWarning)message;
                    Bw?.ReportProgress(0, new ActionWorker
                    {
                        Action = Progress.ApplyImage,
                        IsWarning = true,
                        Message = $"Warning while applying file: {wimMessageWarning.Path} (Win32 error {wimMessageWarning.Win32ErrorCode})"
                    });
                    break;
            }
            return WimMessageResult.Success;
        }
    }
}
