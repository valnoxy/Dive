using System;
using System.IO;
using deployaCore;
using System.Xml;
using deployaUI.Common;
using System.Management;
using System.Reflection;
using System.Windows.Media;

namespace deployaUI.AutoDive
{
    /// <summary>
    /// Interaktionslogik für AutoDiveUI.xaml
    /// </summary>
    public partial class AutoDiveUi
    {
        // AutoDive config
        private static string _version = null;
        private static string _diskSN = null;
        private static string? _diskId = null;
        private static string? _diskName = null;
        private static string _bootloader = null;
        private static string _firmware = null;
        private static bool _useCloudDeployment = false;
        private static bool _useRecovery = false;
        private static string _imageFile = null;
        private static string _imageIndex = null;

        // DeploymentInfo
        private static string _defaultUsername = null;
        private static string _defaultPassword = null;
        private static bool _useAutoInit = false;

        public AutoDiveUi()
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
                    var config = new IniFile(Path.Combine(d.Name, ".diveconfig"));
                    _version = config.Read("Version", "AutoDive");
                    _diskSN = config.Read("DiskID", "AutoDive");
                    _bootloader = config.Read("Bootloader", "AutoDive");
                    _firmware = config.Read("Firmware", "AutoDive");
                    _useCloudDeployment = config.Read("UseCloudDeployment", "AutoDive").ToLower() == "true";
                    _imageFile = config.Read("ImageFile", "AutoDive");
                    _imageIndex = config.Read("ImageIndex", "AutoDive");
                    _defaultUsername = config.Read("DefaultUsername", "DeploymentInfo");
                    _defaultPassword = config.Read("DefaultPassword", "DeploymentInfo");
                    _useAutoInit = config.Read("UseAutoInit", "DeploymentInfo").ToLower() == "true";
                    _useRecovery = config.Read("RecoveryPartition", "AutoDive").ToLower() == "true";

                    // Validate configuration
                    if (string.IsNullOrEmpty(_version)
                        || string.IsNullOrEmpty(_diskSN)
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
                Common.ApplyDetails.Index = Convert.ToInt32(_imageIndex);
                Common.ApplyDetails.UseEFI = _firmware.ToLower() == "efi";
                Common.ApplyDetails.UseNTLDR = _bootloader.ToLower() == "ntldr";
                Common.ApplyDetails.UseRecovery = _useRecovery;
                Common.ApplyDetails.DiskIndex = Convert.ToInt32(_diskSN);

                if (IdentifyImage(_imageFile, _imageIndex) == false)
                    Modules.ExceptionMessage("The requested image file was not found.");
                if (LoadDisks(_diskSN) == false)
                    Modules.ExceptionMessage("The specified disk was not found on this system.");

                ImageName.Text = Common.ApplyDetails.Name;
                ImageFile.Text = Common.ApplyDetails.FileName;
                HDDName.Text = _diskName;

                UseDeploymentInfo.Text = _defaultUsername != null || _defaultPassword != null
                    ? "Using Deployment Info"
                    : "Standard Installation";
                UseAutoInit.Text = _useAutoInit ? "Using AutoInit" : "No AutoInit";
                Firmware.Text = Common.ApplyDetails.UseEFI ? "EFI" : "BIOS";
                Bootloader.Text = Common.ApplyDetails.UseNTLDR ? "NTLDR" : "BOOTMGR";

                try
                {
                    var img = new ImageSourceConverter();
                    ImageIcon.Source = (ImageSource)img.ConvertFromString(Common.ApplyDetails.IconPath)!;
                }
                catch
                {
                    // ignored
                }

                // Deployment Info
                Common.DeploymentInfo.Password = _defaultPassword;
                Common.DeploymentInfo.Username = _defaultUsername;

            }
            catch (Exception ex)
            {
                Modules.ExceptionMessage($"An error has occurred: {ex.Message}");
            }

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

                    Common.Debug.WriteLine("--- Image ---", ConsoleColor.White);
                    Common.Debug.WriteLine($"ID : {product_id}", ConsoleColor.White);
                    Common.Debug.WriteLine($"Name : {product_name}", ConsoleColor.White);
                    Common.Debug.WriteLine($"Size : {sizeInGB}", ConsoleColor.White);
                    Common.Debug.WriteLine("--- Image ---\n", ConsoleColor.White);

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
                    Common.ApplyDetails.Name = product_name;
                    Common.ApplyDetails.IconPath = $"pack://application:,,,/assets/icon-{imageVersion}-40.png";
                    Common.ApplyDetails.FileName = _imageFile;
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Common.Debug.WriteLine(ex.Message, ConsoleColor.Red);
                return false;
            }
        }

        private bool LoadDisks(string diskID)
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

                    Common.Debug.WriteLine($"DeviceID: {deviceId}", ConsoleColor.White);
                    Common.Debug.WriteLine($"Model: {model}", ConsoleColor.White);
                    Common.Debug.WriteLine($"Interface: {interfaceType}", ConsoleColor.White);
                    Common.Debug.WriteLine($"Serial: {serial}", ConsoleColor.White);
                    Common.Debug.WriteLine($"Size: {size}", ConsoleColor.White);
                    Common.Debug.WriteLine($"Size in GB: {sizeInGb}", ConsoleColor.White);

                    if (deviceId == diskID)
                    {
                        _diskName = model;
                        return true;
                    }
                    Common.Debug.WriteLine("==========================================================", ConsoleColor.DarkGray);
                }
            }
            catch
            {
                return false;
            }

            return false;
        }
    }
}
