using Microsoft.Dism;
using Microsoft.Wim;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using deployaCore.Common;
using static System.Collections.Specialized.BitVector32;

namespace deployaCore.Action
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
                Output.WriteLine($"[Driver] Entering Injection process with {driverCount} drivers ...");
                Output.WriteLine("[Driver] Initialize Dism API ...");
                DismApi.Initialize(DismLogLevel.LogErrors);
                Output.WriteLine("[Driver] Open offline session ...");

                DismSession session = null;
                try
                {
                    if (Bw != null) Bw.ReportProgress(207, "Initialize"); // Update progress text
                    session = DismApi.OpenOfflineSession(windowsDrive);
                    Output.WriteLine("[Driver] Session opened. ");
                }
                catch (Exception ex)
                {
                    Output.WriteLine("[Driver] Failed to open offline session: " + ex.Message);
                    Output.WriteLine("[Driver] Shutting down API ...");
                    DismApi.Shutdown();
                    return;
                }

                var currentDriverCount = 0;
                foreach (var driver in driverPath)
                {
                    Output.WriteLine("[Driver] Begin driver installation ...");
                    try
                    {
                        currentDriverCount++;
                        Output.WriteLine($"[Driver] Current Driver ({currentDriverCount}): {driver}");
                        if (Bw != null) Bw.ReportProgress(207, currentDriverCount); // Update progress text
                        DismApi.AddDriver(session, driver, true);
                    }
                    catch (Exception ex)
                    {
                        Output.WriteLine("[Driver] Failed to inject driver: " + ex.Message);
                        if (Bw != null) Bw?.ReportProgress(308, "");
                    }
                }

                Output.WriteLine("[Driver] Closing session ...");
                session.Close();
                Output.WriteLine("[Driver] Shutting down API ...");
                DismApi.Shutdown();
                Output.WriteLine("[Driver] Job completed. Returning now ...");
                return;
            }
            catch (Exception ex)
            {
                Output.WriteLine("[Driver] Unknown error: " + ex.Message);
                DismApi.Shutdown();
            }
        }

        private static WimMessageResult ApplyCallbackMethod(WimMessageType messageType, object message, object userData)
        {
            switch (messageType)
            {
                case WimMessageType.Progress:
                    var wimMessageProgress = (WimMessageProgress)message;
                    if (Bw != null)
                    {
                        Bw.ReportProgress(wimMessageProgress.PercentComplete, ""); // Update progress bar
                        Bw.ReportProgress(202, ""); // Update progress text
                    }
                    ConsoleUtility.WriteProgressBar(wimMessageProgress.PercentComplete, true);
                    break;

                case WimMessageType.Error:
                    var wimMessageError = (WimMessageError)message;
                    Console.WriteLine($"Error: {0} ({1})", (object)wimMessageError.Path, (object)wimMessageError.Win32ErrorCode);
                    Bw?.ReportProgress(302, "");

                    break;

                case WimMessageType.Warning:
                    var wimMessageWarning = (WimMessageWarning)message;
                    Console.WriteLine($"Warning: {0} ({1})", (object)wimMessageWarning.Path, (object)wimMessageWarning.Win32ErrorCode);
                    break;
            }
            return WimMessageResult.Success;
        }
    }
}
