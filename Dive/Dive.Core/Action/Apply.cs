using Microsoft.Dism;
using Microsoft.Wim;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Dive.Core.Common;
using Newtonsoft.Json;

namespace Dive.Core.Action
{
    internal class Apply
    {
        internal static BackgroundWorker Bw = null;

        internal static void WriteToDisk(string imagePath, int index, string drive, BackgroundWorker worker = null)
        {
            Bw = worker;

            using var file = WimgApi.CreateFile(imagePath, WimFileAccess.Read, WimCreationDisposition.OpenExisting, WimCreateFileOptions.None, WimCompressionType.None);
            WimgApi.SetTemporaryPath(file, Environment.GetEnvironmentVariable("TEMP"));
            WimgApi.RegisterMessageCallback(file, new WimMessageCallback(ApplyCallbackMethod));
            try
            {
                using var imageHandle = WimgApi.LoadImage(file, index);
                WimgApi.ApplyImage(imageHandle, drive, WimApplyImageOptions.None);
            }
            finally
            {
                WimgApi.UnregisterMessageCallback(file, new WimMessageCallback(ApplyCallbackMethod));
            }
        }

        internal static void AddDriverToDisk(string windowsDrive, List<string> driverPath, BackgroundWorker worker = null)
        {
            Bw = worker;
            try
            {
                var driverCount = driverPath.Count;
                worker?.ReportProgress(0, JsonConvert.SerializeObject(new ActionWorker
                {
                    IsDebug = true,
                    Message = $"[Driver] Entering Injection process with {driverCount} drivers ..."
                }));
                worker?.ReportProgress(0, JsonConvert.SerializeObject(new ActionWorker
                {
                    IsDebug = true,
                    Message = $"[Driver] Initialize DISM API"
                }));
                DismApi.Initialize(DismLogLevel.LogErrors);
                worker?.ReportProgress(0, JsonConvert.SerializeObject(new ActionWorker
                {
                    IsDebug = true,
                    Message = $"[Driver] Open offline session ..."
                }));

                DismSession session = null;
                try
                {
                    worker?.ReportProgress(0, JsonConvert.SerializeObject(new ActionWorker
                    {
                        Action = Progress.InstallDrivers,
                        IsError = false,
                        IsIndeterminate = true,
                        Message = "Opening Dism Session for Driver injection ..."
                    }));
                    session = DismApi.OpenOfflineSession(windowsDrive);
                    worker?.ReportProgress(0, JsonConvert.SerializeObject(new ActionWorker
                    {
                        IsDebug = true,
                        Message = $"[Driver] Session opened."
                    }));
                }
                catch (Exception ex)
                {
                    worker?.ReportProgress(0, JsonConvert.SerializeObject(new ActionWorker
                    {
                        Action = Progress.InstallDrivers,
                        IsError = true,
                        Message = $"[Driver] Failed to open offline session: " + ex.Message
                    }));
                    worker?.ReportProgress(0, JsonConvert.SerializeObject(new ActionWorker
                    {
                        IsDebug = true,
                        Message = $"[Driver] Shutting down API ..."
                    }));
                    DismApi.Shutdown();
                    return;
                }

                var currentDriverCount = 0;
                foreach (var driver in driverPath)
                {
                    worker?.ReportProgress(0, JsonConvert.SerializeObject(new ActionWorker
                    {
                        IsDebug = true,
                        Message = $"[Driver] Begin driver installation ..."
                    }));
                    try
                    {
                        currentDriverCount++;
                        worker?.ReportProgress(0, JsonConvert.SerializeObject(new ActionWorker
                        {
                            IsDebug = true,
                            Message = $"[Driver] Current Driver ({currentDriverCount}): {driver}"
                        }));
                        worker?.ReportProgress(0, JsonConvert.SerializeObject(new ActionWorker
                        {
                            Action = Progress.InstallDrivers,
                            IsError = false,
                            IsIndeterminate = true,
                            Message = $"Injecting driver {currentDriverCount} of {driverCount} ..."
                        }));
                        DismApi.AddDriver(session, driver, true);
                    }
                    catch (Exception ex)
                    {
                        worker?.ReportProgress(0, JsonConvert.SerializeObject(new ActionWorker
                        {
                            Action = Progress.InstallDrivers,
                            IsWarning = true,
                            IsIndeterminate = false,
                            Message = $"Failed to inject driver {driver}."
                        }));
                    }
                }

                worker?.ReportProgress(0, JsonConvert.SerializeObject(new ActionWorker
                {
                    IsDebug = true,
                    Message = $"[Driver] Closing session ..."
                }));
                session.Close();
                worker?.ReportProgress(0, JsonConvert.SerializeObject(new ActionWorker
                {
                    IsDebug = true,
                    Message = $"[Driver] Shutting down DISM API ..."
                }));
                DismApi.Shutdown();
            }
            catch (Exception ex)
            {
                worker?.ReportProgress(0, JsonConvert.SerializeObject(new ActionWorker
                {
                    Action = Progress.InstallDrivers,
                    IsError = true,
                    IsIndeterminate = false,
                    Message = "Unknown error at installing drivers."
                }));
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
                    Bw?.ReportProgress(wimMessageProgress.PercentComplete, JsonConvert.SerializeObject(new ActionWorker
                    {
                        Action = Progress.ApplyImage,
                        IsError = false,
                        IsIndeterminate = false,
                        Message = $"Applying image ({wimMessageProgress.PercentComplete}% - {estimatedTime})..."
                    }));
                    break;

                case WimMessageType.Error:
                    var wimMessageError = (WimMessageError)message;
                    Bw?.ReportProgress(0, JsonConvert.SerializeObject(new ActionWorker
                    {
                        Action = Progress.ApplyImage,
                        IsError = true,
                        IsIndeterminate = false,
                        Message = $"Error: {(object)wimMessageError.Path} {(object)wimMessageError.Win32ErrorCode}"
                    }));
                    break;

                case WimMessageType.Warning:
                    var wimMessageWarning = (WimMessageWarning)message;
                    Bw?.ReportProgress(0, JsonConvert.SerializeObject(new ActionWorker
                    {
                        Action = Progress.PrepareDisk,
                        IsWarning = true,
                        IsIndeterminate = false,
                        Message = $"Error: {(object)wimMessageWarning.Path} {(object)wimMessageWarning.Win32ErrorCode}"
                    })); 
                    break;
            }
            return WimMessageResult.Success;
        }
    }
}
