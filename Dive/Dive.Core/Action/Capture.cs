using Dive.Core.Common;
using Microsoft.Wim;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using System.Xml;
using Newtonsoft.Json;

namespace Dive.Core.Action
{
    internal class Capture
    {
        internal static BackgroundWorker Bw = null;

        private static string[] _excludedPaths;

        public static void CreateWim(string name, string description, string pathToCapture, string pathToImage, BackgroundWorker worker)
        {
            Bw = worker;
            using var wimHandle = WimgApi.CreateFile(pathToImage,
                WimFileAccess.Write,
                WimCreationDisposition.CreateAlways,
                WimCreateFileOptions.None,
                WimCompressionType.Xpress);
            WimgApi.SetTemporaryPath(wimHandle, Environment.GetEnvironmentVariable("TEMP"));
            WimgApi.RegisterMessageCallback(wimHandle, CaptureCallbackMethod);

            // Exclude paths
            _excludedPaths = new []{
                $"{pathToCapture}$ntfs.log",
                $"{pathToCapture}hiberfil.sys",
                $"{pathToCapture}pagefile.sys",
                $"{pathToCapture}swapfile.sys",
                $"{pathToCapture}System Volume Information",
                $"{pathToCapture}RECYCLER",
                $"{pathToCapture}$RECYCLE.BIN",
                $"{pathToCapture}Windows\\CSC"
            };

            try
            {
                using var imageHandle = WimgApi.CaptureImage(wimHandle, pathToCapture, WimCaptureImageOptions.None);
                var xmlDocument = BuildImageInfo(name, description);
                WimgApi.SetImageInformation(imageHandle, xmlDocument);
            }
            catch (Exception ex)
            {
                worker?.ReportProgress(0, JsonConvert.SerializeObject(new ActionWorker
                {
                    Action = Progress.CaptureDisk,
                    IsError = true,
                    Message = ex.Message
                }));
            }
            finally
            {
                // Be sure to unregister the callback method
                //
                WimgApi.UnregisterMessageCallback(wimHandle, CaptureCallbackMethod);
            }
        }

        internal static string BuildImageInfo(string name, string description)
        {
            var wimDescription = new Dive.Core.Common.WIMDescription.WIMIMAGE()
            {
                NAME = name,
                DESCRIPTION = description,
                INDEX = 1
            };

            var emptyNamespaces = new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty });
            var serializer = new XmlSerializer(typeof(WIMDescription.WIMIMAGE));
            var settings = new XmlWriterSettings
            {
                Indent = true,
                OmitXmlDeclaration = true
            };

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
                    var wimMessageProgress = (WimMessageProgress)message;
                    var estimatedTime = new TimeSpan(
                        wimMessageProgress.EstimatedTimeRemaining.Days,
                        wimMessageProgress.EstimatedTimeRemaining.Hours,
                        wimMessageProgress.EstimatedTimeRemaining.Minutes,
                        wimMessageProgress.EstimatedTimeRemaining.Seconds);
                    Bw?.ReportProgress(wimMessageProgress.PercentComplete, JsonConvert.SerializeObject(new ActionWorker
                    {
                        Action = Progress.CaptureDisk,
                        IsError = false,
                        IsIndeterminate = false,
                        Message = $"Capturing disk ({wimMessageProgress.PercentComplete}% - {estimatedTime})..."
                    }));
                    break;

                case WimMessageType.Process:
                    var processMessage = (WimMessageProcess)message;

                    var b = _excludedPaths.Any(s => processMessage.Path.Contains(s));
                    if (b)
                    {
                        processMessage.Process = false;
                    }

                    Bw?.ReportProgress(0, JsonConvert.SerializeObject(new ActionWorker
                    {
                        IsDebug = true,
                        Message = $"Copying: {processMessage.Path}"
                    }));
                    break;

                case WimMessageType.Scanning:
                    var wimMessageScanning = (WimMessageScanning)message;
                    Bw?.ReportProgress(0, JsonConvert.SerializeObject(new ActionWorker
                    {
                        IsDebug = true,
                        Message = $"Scanning: {wimMessageScanning.Count}"
                    }));
                    break;

                case WimMessageType.Compress:
                    var wimMessageCompress = (WimMessageCompress)message;
                    Bw?.ReportProgress(0, JsonConvert.SerializeObject(new ActionWorker
                    {
                        IsDebug = true,
                        Message = $"Compress: {wimMessageCompress.Path} {wimMessageCompress.Compress}"
                    }));
                    break;

                case WimMessageType.Error:
                    var wimMessageError = (WimMessageError)message;
                    Bw?.ReportProgress(0, JsonConvert.SerializeObject(new ActionWorker
                    {
                        Action = Progress.CaptureDisk,
                        IsError = true,
                        IsIndeterminate = false,
                        Message = $"Error: {(object)wimMessageError.Path} {(object)wimMessageError.Win32ErrorCode}"
                    }));
                    break;

                case WimMessageType.Warning:
                    var wimMessageWarning = (WimMessageWarning)message;
                    Bw?.ReportProgress(0, JsonConvert.SerializeObject(new ActionWorker
                    {
                        Action = Progress.CaptureDisk,
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
