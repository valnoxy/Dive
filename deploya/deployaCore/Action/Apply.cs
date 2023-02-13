using Microsoft.Dism;
using Microsoft.Wim;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace deployaCore.Action
{
    internal class Apply
    {
        internal static BackgroundWorker BW = null;

        internal static void WriteToDisk(string imagePath, int index, string drive, BackgroundWorker worker = null)
        {
            BW = worker;

            string path = drive;
            using (WimHandle file = WimgApi.CreateFile(imagePath, WimFileAccess.Read, WimCreationDisposition.OpenExisting, WimCreateFileOptions.None, WimCompressionType.None))
            {
                WimgApi.SetTemporaryPath(file, Environment.GetEnvironmentVariable("TEMP"));
                WimgApi.RegisterMessageCallback(file, new WimMessageCallback(ApplyCallbackMethod));
                try
                {
                    using (WimHandle imageHandle = WimgApi.LoadImage(file, index))
                        WimgApi.ApplyImage(imageHandle, path, WimApplyImageOptions.None);
                }
                finally
                {
                    WimgApi.UnregisterMessageCallback(file, new WimMessageCallback(ApplyCallbackMethod));
                }
            }
        }

        internal static void AddDriverToDisk(string windowsPath, List<string> driverPath, BackgroundWorker worker = null)
        {
            BW = worker;

            foreach (var driver in driverPath.Where(File.Exists))
            {
                using var session = DismApi.OpenOfflineSession(windowsPath);

                try
                {
                    DismApi.AddDriver(session, driver, true);
                    BW.ReportProgress(202, ""); // Update progress text
                }
                catch
                {

                }
            }
        }

        private static WimMessageResult ApplyCallbackMethod(WimMessageType messageType, object message, object userData)
        {
            switch (messageType)
            {
                case WimMessageType.Progress:
                    WimMessageProgress wimMessageProgress = (WimMessageProgress)message;
                    if (BW != null)
                    {
                        BW.ReportProgress(wimMessageProgress.PercentComplete, ""); // Update progress bar
                        BW.ReportProgress(202, ""); // Update progress text
                    }
                    ConsoleUtility.WriteProgressBar(wimMessageProgress.PercentComplete, true);
                    break;

                case WimMessageType.Error:
                    WimMessageError wimMessageError = (WimMessageError)message;
                    Console.WriteLine($"Error: {0} ({1})", (object)wimMessageError.Path, (object)wimMessageError.Win32ErrorCode);
                    if (BW != null) { BW.ReportProgress(302, ""); }
                    break;

                case WimMessageType.Warning:
                    WimMessageWarning wimMessageWarning = (WimMessageWarning)message;
                    Console.WriteLine($"Warning: {0} ({1})", (object)wimMessageWarning.Path, (object)wimMessageWarning.Win32ErrorCode);
                    break;
            }
            return WimMessageResult.Success;
        }
    }
}
