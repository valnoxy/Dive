using deployaCore.Common;
using Microsoft.Wim;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Xml.Serialization;
using System.Xml;

namespace deployaCore.Action
{
    internal class Capture
    {
        internal static BackgroundWorker BW = null;
        public static void CreateWim(string name, string description, string pathToCapture, string pathToImage, BackgroundWorker worker)
        {
            BW = worker;
            List<string> excludedPaths = new List<string>() {
                "C:\\$ntfs.log",
                "C:\\hiberfil.sys",
                "C:\\pagefile.sys",
                "C:\\swapfile.sys",
                "C:\\System Volume Information",
                "C:\\RECYCLER",
                "C:\\Windows\\CSC"
            };

            using (WimHandle wimHandle = WimgApi.CreateFile(pathToImage,
                       WimFileAccess.Write,
                       WimCreationDisposition.CreateAlways,
                       WimCreateFileOptions.None,
                       WimCompressionType.Xpress))
            {

                WimgApi.SetTemporaryPath(wimHandle, Environment.GetEnvironmentVariable("TEMP"));
                WimgApi.RegisterMessageCallback(wimHandle, CaptureCallbackMethod);
                try
                {
                    using (WimHandle imageHandle = WimgApi.CaptureImage(wimHandle, pathToCapture, WimCaptureImageOptions.None))
                    {
                        var xmlDocument = BuildImageInfo(name, description);
                        WimgApi.SetImageInformation(imageHandle, xmlDocument);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("CaptureImage error: " + ex.Message);
                }
                finally
                {
                    // Be sure to unregister the callback method
                    //
                    WimgApi.UnregisterMessageCallback(wimHandle, CaptureCallbackMethod);
                }
            }
        }

        internal static string BuildImageInfo(string name, string description)
        {
            var wimDescription = new deployaCore.Common.WIMDescription.WIMIMAGE()
            {
                NAME = name,
                DESCRIPTION = description,
                INDEX = 1
            };

            var emptyNamespaces = new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty });
            var serializer = new XmlSerializer(typeof(WIMDescription.WIMIMAGE));
            var settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.OmitXmlDeclaration = true;

            using var stream = new StringWriter();
            using var writer = XmlWriter.Create(stream, settings);
            serializer.Serialize(writer, wimDescription, emptyNamespaces);
            return stream.ToString();
        }

        private static WimMessageResult CaptureCallbackMethod(WimMessageType messageType, object message, object userData)
        {
            switch (messageType)
            {
                case WimMessageType.Progress:
                    WimMessageProgress wimMessageProgress = (WimMessageProgress)message;
                    if (BW != null)
                    {
                        BW.ReportProgress(wimMessageProgress.PercentComplete, ""); // Update progress bar
                        //BW.ReportProgress(201, ""); // Update progress text
                    }
                    ConsoleUtility.WriteProgressBar(wimMessageProgress.PercentComplete, true);
                    break;

                case WimMessageType.Process:
                    WimMessageProcess processMessage = (WimMessageProcess)message;
                    if (BW != null)
                    {
                        BW.ReportProgress(202, ""); // Update progress text
                        Output.WriteLine("Copying: " + processMessage.Path);
                    }
                    break;

                case WimMessageType.Scanning:
                    WimMessageScanning wimMessageScanning = (WimMessageScanning)message;
                    if (BW != null)
                    {
                        BW.ReportProgress(203, ""); // Update progress text
                        Output.WriteLine("Scanning: " + wimMessageScanning.Count);
                    }
                    break;

                case WimMessageType.Compress:
                    WimMessageCompress wimMessageCompress = (WimMessageCompress)message;
                    if (BW != null)
                    {
                        BW.ReportProgress(204, ""); // Update progress text
                        Output.WriteLine("Compress: " + wimMessageCompress.Path + " " + wimMessageCompress.Compress.ToString());
                    }
                    break;

                case WimMessageType.Error:
                    WimMessageError wimMessageError = (WimMessageError)message;
                    Console.WriteLine($"Error: {0} ({1})", (object)wimMessageError.Path, (object)wimMessageError.Win32ErrorCode);
                    if (BW != null) { BW.ReportProgress(301, ""); }
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
