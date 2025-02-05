using System;
using System.IO;
using Dive.Core;
using System.Xml;
using Dive.UI.Common;
using System.Management;
using System.Windows.Media;
using System.ComponentModel;
using Dive.UI.Pages;
using Dive.Core.Common;
using Dive.UI.Common.Configuration;
using Dive.Core.Action.Deployment;
using Dive.UI.Common.UserInterface;

namespace Dive.UI.AutoDive
{
    /// <summary>
    /// Interaktionslogik für AutoDiveUI.xaml
    /// </summary>
    public partial class AutoDiveUi_Old
    {
        // AutoDive config
        private static string? _version;
        private static string? _diskId = null;
        private static string? _diskName = null;
        private static string? _bootloader = null;
        private static string _firmware = null;
        private static bool _useRecovery = false;
        private static string _imageFile = null;
        private static string _imageIndex = null;

        // DeploymentInfo
        private static string _defaultUsername = null;
        private static string _defaultPassword = null;
        private static bool _useAutoInit = false;

        // AutoDive Drive
        private static string _driveLetter = null;

        private static readonly ApplyDetails ApplyDetailsInstance = ApplyDetails.Instance;
        private static readonly DeploymentInfo DeploymentInfoInstance = DeploymentInfo.Instance;
        private static readonly DeploymentOption DeploymentOptionInstance = DeploymentOption.Instance;
        private static readonly OemInfo OemInfoInstance = OemInfo.Instance;

        public AutoDiveUi_Old()
        {
            InitializeComponent();

            // Read & verify configuration
            try
            {
                var allDrives = DriveInfo.GetDrives();
                var IsFound = false;
                foreach (var d in allDrives)
                {
                    if (!File.Exists(Path.Combine(d.Name, ".diveconfig"))) continue;
                    _driveLetter = d.Name;

                    var config = new IniFile(Path.Combine(d.Name, ".diveconfig"));
                    _version = config.Read("Version", "AutoDive");
                    _diskId = config.Read("DiskID", "AutoDive");
                    _bootloader = config.Read("Bootloader", "AutoDive");
                    _firmware = config.Read("Firmware", "AutoDive");
                    _imageFile = config.Read("ImageFile", "AutoDive");
                    _imageIndex = config.Read("ImageIndex", "AutoDive");
                    _defaultUsername = config.Read("DefaultUsername", "DeploymentInfo");
                    _defaultPassword = config.Read("DefaultPassword", "DeploymentInfo");
                    _useAutoInit = config.Read("UseAutoInit", "DeploymentInfo").ToLower() == "true";
                    _useRecovery = config.Read("RecoveryPartition", "AutoDive").ToLower() == "true";

                    // Validate configuration
                    if (string.IsNullOrEmpty(_version)
                        || string.IsNullOrEmpty(_diskId)
                        || string.IsNullOrEmpty(_bootloader)
                        || string.IsNullOrEmpty(_firmware)
                        || string.IsNullOrEmpty(_imageFile)
                        || string.IsNullOrEmpty(_imageIndex))
                    {
                        Modules.ExceptionMessage("The existing Windows deployment configuration file is invalid.");
                    }
                    else IsFound = true;
                }
                if (IsFound == false) Modules.ExceptionMessage("No Windows deployment configuration file could be found.");
            }
            catch (Exception ex)
            {
                Modules.ExceptionMessage($"An error has occurred while trying to read the Windows deployment configuration file.\n{ex.Message}");
            }


            // Parsing configuration
            // -------------------------------------------
            try
            {
                // ApplyDetails
                ApplyDetailsInstance.Index = Convert.ToInt32(_imageIndex);
                ApplyDetailsInstance.UseEFI = _firmware.ToLower() == "efi";
                ApplyDetailsInstance.UseNTLDR = _bootloader.ToLower() == "ntldr";
                ApplyDetailsInstance.UseRecovery = _useRecovery;
                ApplyDetailsInstance.DiskIndex = Convert.ToInt32(_diskId);

                _imageFile = Path.Combine(_driveLetter, "WIMs", _imageFile);

                if (IdentifyImage(_imageFile, _imageIndex) == false)
                    Modules.ExceptionMessage("The requested image file was not found.");
                if (LoadDisks(_diskId) == false)
                    Modules.ExceptionMessage("The specified disk was not found on this system.");

                ImageName.Text = ApplyDetailsInstance.Name;
                ImageFile.Text = ApplyDetailsInstance.FileName;
                HDDName.Text = _diskName;

                UseDeploymentInfo.Text = _defaultUsername != null || _defaultPassword != null
                    ? "Using Deployment Info"
                    : "Standard Installation";
                UseAutoInit.Text = _useAutoInit ? "Using AutoInit" : "No AutoInit";
                Firmware.Text = ApplyDetailsInstance.UseEFI ? "EFI" : "BIOS";
                Bootloader.Text = ApplyDetailsInstance.UseNTLDR ? "NTLDR" : "BOOTMGR";

                try
                {
                    var img = new ImageSourceConverter();
                    ImageIcon.Source = (ImageSource)img.ConvertFromString(ApplyDetailsInstance.IconPath)!;
                }
                catch
                {
                    // ignored
                }

                // Deployment Info
                DeploymentInfoInstance.Password = _defaultPassword;
                DeploymentInfoInstance.Username = _defaultUsername;

            }
            catch (Exception ex)
            {
                Modules.ExceptionMessage($"An error has occurred: {ex.Message}");
            }

            // Backgrond worker for deployment
            applyBackgroundWorker = new BackgroundWorker();
            applyBackgroundWorker.WorkerReportsProgress = true;
            applyBackgroundWorker.WorkerSupportsCancellation = true;
            applyBackgroundWorker.DoWork += ApplyWim;
            applyBackgroundWorker.ProgressChanged += applyBackgroundWorker_ProgressChanged;
            applyBackgroundWorker.RunWorkerAsync();
        }

        private static bool IdentifyImage(string imagePath, string imageIndex)
        {
            try
            {
                var action = Actions.GetInfo(imagePath);

                var doc = new XmlDocument();
                doc.LoadXml(action);

                var imageNames = doc.DocumentElement.SelectNodes("/WIM/IMAGE");

                string product_id = "", product_name = "", product_size = "";

                foreach (XmlNode node in imageNames)
                {
                    product_id = node.Attributes?["INDEX"]?.Value;
                    product_name = node.SelectSingleNode("NAME").InnerText;
                    product_size = node.SelectSingleNode("TOTALBYTES").InnerText;
                    var sizeInGB = Modules.convertSize(Convert.ToDouble(product_size));

                    Debug.WriteLine("--- Image ---", ConsoleColor.White);
                    Debug.WriteLine($"ID : {product_id}", ConsoleColor.White);
                    Debug.WriteLine($"Name : {product_name}", ConsoleColor.White);
                    Debug.WriteLine($"Size : {sizeInGB}", ConsoleColor.White);
                    Debug.WriteLine("--- Image ---\n", ConsoleColor.White);

                    var imageVersion = "";

                    // Windows Client
                    if (product_name.ToLower().Contains("windows 2000"))
                        imageVersion = "windows-2000";
                    if (product_name.ToLower().Contains("windows xp"))
                        imageVersion = "windows-xp";
                    if (product_name.ToLower().Contains("windows vista"))
                        imageVersion = "windows-vista";
                    if (product_name.ToLower().Contains("windows 7"))
                        imageVersion = "windows-7";
                    if (product_name.ToLower().Contains("windows 8") || product_name.ToLower().Contains("windows 8.1"))
                        imageVersion = "windows-10";
                    if (product_name.ToLower().Contains("windows 10"))
                        imageVersion = "windows-10";
                    if (product_name.ToLower().Contains("windows 11"))
                        imageVersion = "windows-11";

                    // Windows Server
                    if (product_name.ToLower().Contains("windows server 2003"))
                        imageVersion = "windows-xp";
                    if (product_name.ToLower().Contains("windows server 2008"))
                        imageVersion = "windows-7";
                    if (product_name.ToLower().Contains("windows server 2012"))
                        imageVersion = "windows-server-2012";
                    if (product_name.ToLower().Contains("windows Server 2016"))
                        imageVersion = "windows-server-2012";
                    if (product_name.ToLower().Contains("windows server 2019"))
                        imageVersion = "windows-server-2012";
                    if (product_name.ToLower().Contains("windows server 2022"))
                        imageVersion = "windows-server-2012";

                    // Exploitox Internal
                    if (product_name.ToLower().Contains("ai operating system") || product_name.ToLower().Contains("aios"))
                        imageVersion = "aios";

                    // Microsoft Internal / Other OS
                    if (product_name.ToLower().Contains("windows core os") || product_name.ToLower().Contains("wcos"))
                        imageVersion = "windows-10";
                    if (product_name.ToLower().Contains("phone"))
                        imageVersion = "windows-10";

                    // Beta
                    if (product_name.ToLower().Contains("whistler"))
                        imageVersion = "windows-xp";
                    if (product_name.ToLower().Contains("longhorn"))
                        imageVersion = "windows-vista";
                    if (product_name.ToLower().Contains("blue"))
                        imageVersion = "windows-10";

                    if (product_id != imageIndex) continue;
                    ApplyDetailsInstance.Name = product_name;
                    ApplyDetailsInstance.IconPath = $"pack://application:,,,/assets/icon-{imageVersion}-40.png";
                    ApplyDetailsInstance.FileName = _imageFile;
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message, ConsoleColor.Red);
                return false;
            }
        }

        private bool LoadDisks(string diskId)
        {
            try
            {
                var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive");
                foreach (var info in searcher.Get())
                {
                    var deviceId = "";
                    var model = "";
                    var interfaceType = "";
                    var serial = "";
                    var size = "";
                    var sizeInGb = "";

                    if (info["DeviceID"] != null) deviceId = info["DeviceID"].ToString();
                    if (info["Model"] != null) model = info["Model"].ToString();
                    if (info["InterfaceType"] != null) interfaceType = info["InterfaceType"].ToString();
                    if (info["SerialNumber"] != null) serial = info["SerialNumber"].ToString();
                    if (info["Size"] != null) size = info["Size"].ToString();
                    if (info["Size"] != null) sizeInGb = Modules.convertSize(Convert.ToDouble(info["Size"]));

                    Debug.WriteLine($"DeviceID: {deviceId}", ConsoleColor.White);
                    Debug.WriteLine($"Model: {model}", ConsoleColor.White);
                    Debug.WriteLine($"Interface: {interfaceType}", ConsoleColor.White);
                    Debug.WriteLine($"Serial: {serial}", ConsoleColor.White);
                    Debug.WriteLine($"Size: {size}", ConsoleColor.White);
                    Debug.WriteLine($"Size in GB: {sizeInGb}", ConsoleColor.White);

                    if (deviceId == $"\\\\.\\PHYSICALDRIVE{diskId}")
                    {
                        _diskName = model;
                        return true;
                    }
                    Debug.WriteLine("==========================================================", ConsoleColor.DarkGray);
                }
            }
            catch
            {
                return false;
            }

            return false;
        }

        private BackgroundWorker applyBackgroundWorker;
        bool IsCanceled = false;
        private void applyBackgroundWorker_ProgressChanged(object? sender, ProgressChangedEventArgs e)
        {
            var response = (ActionWorker)e.UserState!;

            if (response == null!) return;

            if (response.IsError)
            {
                ProgrText.Text = response.Message;
                ProgrBar.Value = e.ProgressPercentage;

                Debug.WriteLine(response.Message, ConsoleColor.Red);

                if (ApplyContent.ContentWindow != null)
                {
                    ApplyContent.ContentWindow.NextBtn.IsEnabled = false;
                    ApplyContent.ContentWindow.BackBtn.IsEnabled = false;
                    ApplyContent.ContentWindow.CancelBtn.IsEnabled = true;
                }
                if (CloudContent.ContentWindow != null)
                {
                    CloudContent.ContentWindow.NextBtn.IsEnabled = false;
                    CloudContent.ContentWindow.BackBtn.IsEnabled = false;
                    CloudContent.ContentWindow.CancelBtn.IsEnabled = true;
                }

                IsCanceled = true;
                return;
            }

            if (response.IsWarning)
            {
                Debug.WriteLine(response.Message, ConsoleColor.Yellow);
                return;
            }

            if (response.IsDebug)
            {
                Debug.WriteLine(response.Message);
                return;
            }

            switch (response.Action)
            {
                case Progress.PrepareDisk:
                    {
                        ProgrText.Text = response.Message;
                        if (response.IsIndeterminate)
                            ProgrBar.IsIndeterminate = true;
                        else
                            ProgrBar.Value = e.ProgressPercentage;
                        Debug.RefreshProgressBar(Progress.PrepareDisk, e.ProgressPercentage, response.Message);
                        break;
                    }
                case Progress.ApplyImage:
                    {
                        ProgrText.Text = response.Message;
                        if (response.IsIndeterminate)
                            ProgrBar.IsIndeterminate = true;
                        else
                            ProgrBar.Value = e.ProgressPercentage;
                        Debug.RefreshProgressBar(Progress.ApplyImage, e.ProgressPercentage, response.Message);
                        break;
                    }
                case Progress.InstallBootloader:
                    {
                        ProgrText.Text = response.Message;
                        if (response.IsIndeterminate)
                            ProgrBar.IsIndeterminate = true;
                        else
                            ProgrBar.Value = e.ProgressPercentage;
                        Debug.RefreshProgressBar(Progress.InstallBootloader, e.ProgressPercentage, response.Message);
                        break;
                    }
            }
        }

        private void ApplyWim(object? sender, DoWorkEventArgs e)
        {
            #region Environment definition

            var worker = sender as BackgroundWorker;
            var firmware = ApplyDetailsInstance.UseEFI switch // Firmware definition
            {
                true => Entities.Firmware.EFI,
                false => Entities.Firmware.BIOS
            };
            var bootloader = ApplyDetailsInstance.UseNTLDR switch // Bootloader definition
            {
                true => Entities.Bootloader.NTLDR,
                false => Entities.Bootloader.BOOTMGR,
            };

            // Partition layout definition
            char[] letters;
            Entities.PartitionStyle partStyle;

            // Letters definition:
            // [0] = Windows drive
            // [1] = Boot drive
            // [2] = Recovery

            switch (ApplyDetailsInstance.UseNTLDR)
            {
                // pre-Vista
                case true:
                    letters = Actions.GetSystemLetters(Entities.PartitionStyle.Single);
                    partStyle = Entities.PartitionStyle.Single;
                    break;

                case false:
                    {
                        switch (ApplyDetailsInstance.UseRecovery)
                        {
                            case true:
                                letters = Actions.GetSystemLetters(Entities.PartitionStyle.Full);
                                partStyle = Entities.PartitionStyle.Full;
                                break;

                            case false:
                                // If Vista is used, we need to use the Single partition layout
                                if (ApplyDetailsInstance.Name.ToLower().Contains("windows vista"))
                                {
                                    // Except if EFI is used, then we need to use the SeparateBoot partition layout
                                    if (ApplyDetailsInstance.UseEFI)
                                    {
                                        letters = Actions.GetSystemLetters(Entities.PartitionStyle.SeparateBoot);
                                        partStyle = Entities.PartitionStyle.SeparateBoot;
                                    }
                                    else
                                    {
                                        letters = Actions.GetSystemLetters(Entities.PartitionStyle.Single);
                                        partStyle = Entities.PartitionStyle.Single;
                                    }
                                }
                                else
                                {
                                    letters = Actions.GetSystemLetters(Entities.PartitionStyle.SeparateBoot);
                                    partStyle = Entities.PartitionStyle.SeparateBoot;
                                }
                                break;
                        }
                        break;
                    }
            }

            string windowsDrive = "\0", bootDrive = "\0", recoveryDrive = "\0";
            switch (partStyle)
            {
                case Entities.PartitionStyle.Single:
                    windowsDrive = $"{letters[0]}:\\";
                    break;
                case Entities.PartitionStyle.SeparateBoot:
                    windowsDrive = $"{letters[0]}:\\";
                    bootDrive = $"{letters[1]}:\\";
                    break;
                case Entities.PartitionStyle.Full:
                    windowsDrive = $"{letters[0]}:\\";
                    bootDrive = $"{letters[1]}:\\";
                    recoveryDrive = $"{letters[2]}:\\";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Debug.Write("New Windows drive: ");
            Debug.Write($"{windowsDrive}\n", true, ConsoleColor.DarkYellow);

            Debug.Write("New Boot drive: ");
            Debug.Write($"{bootDrive}\n", true, ConsoleColor.DarkYellow);

            Debug.Write("New Recovery drive: ");
            Debug.Write($"{recoveryDrive}\n", true, ConsoleColor.DarkYellow);

            Debug.Write("Using partition style: ");
            Debug.Write($"{partStyle}\n", true, ConsoleColor.DarkYellow);

            #endregion

            // Initialize worker progress
            worker?.ReportProgress(0, "");       // Value 0

            // Prepare disk
            worker?.ReportProgress(201, "");     // Prepare Disk Text
            Actions.PrepareDisk(firmware, bootloader, ApplyDetailsInstance.DiskIndex, false, partStyle, ApplyDetailsInstance.UseRecovery, windowsDrive, bootDrive, recoveryDrive, worker);
            if (IsCanceled)
            {
                e.Cancel = true;
                return;
            }

            // Apply image
            worker?.ReportProgress(202, "");     // Applying Image Text
            worker?.ReportProgress(0, "");       // Value 0
            Actions.ApplyWim(windowsDrive, ApplyDetailsInstance.FileName, ApplyDetailsInstance.Index, worker);
            if (IsCanceled)
            {
                e.Cancel = true;
                return;
            }

            // Install Bootloader
            worker?.ReportProgress(203, "");     // Installing Bootloader Text
            switch (partStyle)
            {
                case Entities.PartitionStyle.SeparateBoot:
                case Entities.PartitionStyle.Full:
                    Actions.InstallBootloader(firmware, bootloader, windowsDrive, bootDrive, worker);
                    break;
                case Entities.PartitionStyle.Single:
                    Actions.InstallBootloader(firmware, bootloader, windowsDrive, windowsDrive, worker);
                    break;
            }

            if (IsCanceled)
            {
                e.Cancel = true;
                return;
            }

            // Install Recovery (only for Vista and higher)
            if (bootloader == Entities.Bootloader.BOOTMGR && ApplyDetailsInstance.UseRecovery)
            {
                worker?.ReportProgress(204, "");     // Installing Bootloader Text
                Actions.InstallRecovery($"{windowsDrive}Windows", recoveryDrive, DeploymentOptionInstance.AddDiveToWinRE, worker);

                if (IsCanceled)
                {
                    e.Cancel = true;
                    return;
                }
            }

            // Install unattend file (only for Vista and higher)
            if (DeploymentInfoInstance.UseUserInfo || OemInfoInstance.UseOemInfo)
            {
                worker?.ReportProgress(205, "");     // Installing unattend file

                // Building config
                var config = File.Exists(DeploymentInfoInstance.CustomFilePath) ? File.ReadAllText(DeploymentInfoInstance.CustomFilePath) : Common.UnattendBuilder.Build();
                Debug.WriteLine(config);
                Actions.InstallUnattend($"{windowsDrive}Windows", config, OemInfoInstance.LogoPath, DeploymentOptionInstance.UseSMode, worker);

                if (IsCanceled)
                {
                    e.Cancel = true;
                    return;
                }
            }

            // Install Drivers (only for Vista and higher)
            if (ApplyDetailsInstance.DriverList != null)
            {
                worker?.ReportProgress(207, "");     // Installing Drivers Text

                var _driverCount = ApplyDetailsInstance.DriverList.Count;
                Actions.InstallDriver(windowsDrive, ApplyDetailsInstance.DriverList, worker);

                if (IsCanceled)
                {
                    e.Cancel = true;
                    return;
                }
            }

            // Install UefiSeven (only for Vista and 7 with EFI)
            if (Common.WindowsModification.InstallUefiSeven)
            {
                worker?.ReportProgress(206, "");     // Installing UefiSeven
                UefiSeven.InstallUefiSeven(bootDrive,
                    Common.WindowsModification.UsToggleSkipErros,
                    Common.WindowsModification.UsToggleFakeVesa,
                    Common.WindowsModification.UsToggleVerbose,
                    Common.WindowsModification.UsToggleLog,
                    worker);

                if (IsCanceled)
                {
                    e.Cancel = true;
                    return;
                }
            }

            // Installation complete
            worker?.ReportProgress(250, "");     // Installation complete Text
        }
    }
}
