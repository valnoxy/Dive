﻿using Dive.Core.Common;
using Microsoft.Wim;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml.Serialization;
using System.Xml;
using Newtonsoft.Json;

namespace Dive.Core.Action.Capturing
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
            _excludedPaths = new[] {
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
                var pathToKernel = Path.Combine(pathToCapture, "Windows", "System32", "ntoskrnl.exe");
                var xmlDocument = BuildImageInfo(name, description, pathToKernel);
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

        internal static string BuildImageInfo(string name, string description, string pathToKernel)
        {
            // Gather infos from Kernel
            if (!File.Exists(pathToKernel))
            {
                return "Error";
            }

            var kernelInfo = FileVersionInfo.GetVersionInfo(pathToKernel);
            var blocks = kernelInfo.ProductVersion!.Split('.');
            if (blocks.Length != 4)
            {
                return "Error";
            }

            var arch = GetArchFromFile(pathToKernel);

            var wimDescription = new WIMDescription.WIMIMAGE
            {
                NAME = name,
                DESCRIPTION = description,
                INDEX = 1,
                WINDOWS = new WIMDescription.WIMIMAGEWINDOWS
                {
                    ARCH = arch,
                    VERSION = new WIMDescription.WIMIMAGEWINDOWSVERSION
                    {
                        MAJOR = Convert.ToUInt16(blocks[0]),
                        MINOR = Convert.ToUInt16(blocks[1]),
                        BUILD = Convert.ToUInt16(blocks[2]),
                        SPBUILD = Convert.ToUInt16(blocks[3]),
                        //BRANCH = "" // TODO
                    }
                }
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

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        static extern bool GetBinaryType(string lpApplicationName,
            out BinaryType lpBinaryType);
        public enum BinaryType : uint
        {
            SCS_32BIT_BINARY = 0, // A 32-bit Windows-based application
            SCS_64BIT_BINARY = 6, // A 64-bit Windows-based application.
            SCS_DOS_BINARY = 1, // An MS-DOS – based application
            SCS_OS216_BINARY = 5, // A 16-bit OS/2-based application
            SCS_PIF_BINARY = 3, // A PIF file that executes an MS-DOS – based application
            SCS_POSIX_BINARY = 4, // A POSIX – based application
            SCS_WOW_BINARY = 2 // A 16-bit Windows-based application
        }

        private static int GetArchFromFile(string filePath)
        {
            try
            {
                GetBinaryType(filePath, out var architecture);
                var productArch = architecture switch
                {
                    BinaryType.SCS_32BIT_BINARY => 0,
                    BinaryType.SCS_64BIT_BINARY => 9,
                    _ => -1
                };
                return productArch;
            }
            catch
            {
                return -1;
            }
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
                        Message = $"Error: {wimMessageError.Path} {wimMessageError.Win32ErrorCode}"
                    }));
                    break;

                case WimMessageType.Warning:
                    var wimMessageWarning = (WimMessageWarning)message;
                    Bw?.ReportProgress(0, JsonConvert.SerializeObject(new ActionWorker
                    {
                        Action = Progress.CaptureDisk,
                        IsWarning = true,
                        IsIndeterminate = false,
                        Message = $"Error: {wimMessageWarning.Path} {wimMessageWarning.Win32ErrorCode}"
                    }));
                    break;
            }
            return WimMessageResult.Success;
        }
    }
}
