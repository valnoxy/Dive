using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media;
using deploya_core;
using System.Xml;
using deployaUI.Common;
using System.Management;
using System.Text.RegularExpressions;

namespace deployaUI
{
    /// <summary>
    /// Interaktionslogik für AutoDive.xaml
    /// </summary>
    public partial class AutoDive : Wpf.Ui.Controls.UiWindow
    {
        // AutoDive config
        private static string _version = null;
        private static string _diskSN = null;
        private static string _diskId = null;
        private static string _diskName = null;
        private static string _bootloader = null;
        private static string _firmware = null;
        private static bool _useCloudDeployment = false;
        private static string _imageFile = null;
        private static string _imageIndex = null;

        // DeploymentInfo
        private static string _defaultUsername = null;
        private static string _defaultPassword = null;
        private static bool _useAutoInit = false;

        public AutoDive()
        {
            InitializeComponent();

            // Read & verify configuration
            try
            {
                DriveInfo[] allDrives = DriveInfo.GetDrives();
                bool IsFound = false;
                foreach (DriveInfo d in allDrives)
                {
                    if (File.Exists(Path.Combine(d.Name, ".diveconfig")))
                    {
                        IniFile config = new IniFile(Path.Combine(d.Name, ".diveconfig"));
                        _version = config.Read("Version", "AutoDive");
                        _diskSN = config.Read("DiskSN", "AutoDive");
                        _bootloader = config.Read("Bootloader", "AutoDive");
                        _firmware = config.Read("Firmware", "AutoDive");
                        _useCloudDeployment = config.Read("UseCloudDeployment", "AutoDive").ToLower() == "true";
                        _imageFile = config.Read("ImageFile", "AutoDive");
                        _imageIndex = config.Read("ImageIndex", "AutoDive");
                        _defaultUsername = config.Read("DefaultUsername", "DeploymentInfo");
                        _defaultPassword = config.Read("DefaultPassword", "DeploymentInfo");
                        _useAutoInit = config.Read("UseAutoInit", "DeploymentInfo").ToLower() == "true";

                        // Validate configuration
                        if (String.IsNullOrEmpty(_version)
                            || String.IsNullOrEmpty(_diskSN)
                            || String.IsNullOrEmpty(_bootloader)
                            || String.IsNullOrEmpty(_firmware)
                            || String.IsNullOrEmpty(_imageFile)
                            || String.IsNullOrEmpty(_imageIndex))
                        {
                            ExceptionMessage("The existing Windows deployment configuration file is invalid.");
                        }
                        else IsFound = true;
                    }
                }
                if (IsFound == false) ExceptionMessage("No Windows deployment configuration file could be found.");
            }
            catch
            {
                ExceptionMessage("The existing Windows deployment configuration file is invalid.");
            }

            try
            {
                
                if (IdentifyImage(_imageFile, _imageIndex) == false) ExceptionMessage("The requested image file was not found.");
                if (LoadDisks(_diskSN) == false) ExceptionMessage("The specified disk was not found on this system.");

                ImageName.Text = Common.ApplyDetails.Name;
                ImageFile.Text = Common.ApplyDetails.FileName;
                HDDName.Text = _diskName;

                if (_defaultUsername != null || _defaultPassword != null)
                    UseDeploymentInfo.Text = "Using Deployment Info";
                else
                    UseDeploymentInfo.Text = "Standard Installation";

                if (_useAutoInit)
                    UseAutoInit.Text = "Using AutoInit";
                else
                    UseAutoInit.Text = "No AutoInit";

                if (Common.ApplyDetails.UseEFI)
                    Firmware.Text = "EFI";
                else
                    Firmware.Text = "BIOS";

                if (Common.ApplyDetails.UseNTLDR)
                    Bootloader.Text = "NTLDR";
                else
                    Bootloader.Text = "BOOTMGR";

                try
                {
                    ImageSourceConverter img = new ImageSourceConverter();
                    ImageIcon.Source = (ImageSource)img.ConvertFromString(Common.ApplyDetails.IconPath);
                }
                catch {}
            }
            catch (Exception ex)
            {
                string message = $"An error has occurred: {ex.Message}";
                string title = "AutoDive";
                string btn1 = "Exit";

                var w = new MessageUI(title, message, btn1);
                if (w.ShowDialog() == false)
                {
                    string summary = w.Summary;
                    if (summary == "Btn1")
                    {
                        Environment.Exit(1);
                    }
                }
            }

        }

        private void ExceptionMessage(string v)
        {
            string message = v;
            string title = "AutoDive";
            string btn1 = "Exit";

            var w = new MessageUI(title, message, btn1);
            if (w.ShowDialog() == false)
            {
                string summary = w.Summary;
                if (summary == "Btn1")
                {
                    Environment.Exit(1);
                }
            }
        }

        private bool IdentifyImage(string imagePath, string imageIndex)
        {
            try
            {
                string action = Actions.GetInfo(imagePath);

                XmlDocument doc = new XmlDocument();
                doc.LoadXml(action);

                XmlNodeList imageNames = doc.DocumentElement.SelectNodes("/WIM/IMAGE");

                string product_id = "", product_name = "", product_size = "";

                foreach (XmlNode node in imageNames)
                {
                    product_id = node.Attributes?["INDEX"]?.Value;
                    product_name = node.SelectSingleNode("NAME").InnerText;
                    product_size = node.SelectSingleNode("TOTALBYTES").InnerText;
                    string sizeInGB = convertSize(Convert.ToDouble(product_size));

                    Common.Debug.WriteLine("--- Image ---", ConsoleColor.White);
                    Common.Debug.WriteLine($"ID : {product_id}", ConsoleColor.White);
                    Common.Debug.WriteLine($"Name : {product_name}", ConsoleColor.White);
                    Common.Debug.WriteLine($"Size : {sizeInGB}", ConsoleColor.White);
                    Common.Debug.WriteLine("--- Image ---\n", ConsoleColor.White);

                    string imageVersion = "";

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

                    if (product_id == imageIndex)
                    {
                        Common.ApplyDetails.Name = product_name;
                        Common.ApplyDetails.IconPath = $"pack://application:,,,/assets/icon-{imageVersion}-40.png";
                        Common.ApplyDetails.FileName = _imageFile;
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex) 
            { 
                Common.Debug.WriteLine(ex.Message, ConsoleColor.Red);
                return false;
            }
        }

        private bool LoadDisks(string diskSN)
        {
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive");
                foreach (ManagementObject info in searcher.Get())
                {
                    string DeviceID = "";
                    string Model = "";
                    string Interface = "";
                    string Serial = "";
                    string Size = "";
                    string SizeInGB = "";

                    if (info["DeviceID"] != null) DeviceID = info["DeviceID"].ToString();
                    if (info["Model"] != null) Model = info["Model"].ToString();
                    if (info["InterfaceType"] != null) Interface = info["InterfaceType"].ToString();
                    if (info["SerialNumber"] != null) Serial = info["SerialNumber"].ToString();
                    if (info["Size"] != null) Size = info["Size"].ToString();
                    if (info["Size"] != null) SizeInGB = convertSize(Convert.ToDouble(info["Size"])).ToString();

                    Common.Debug.WriteLine($"DeviceID: {DeviceID}", ConsoleColor.White);
                    Common.Debug.WriteLine($"Model: {Model}", ConsoleColor.White);
                    Common.Debug.WriteLine($"Interface: {Interface}", ConsoleColor.White);
                    Common.Debug.WriteLine($"Serial: {Serial}", ConsoleColor.White);
                    Common.Debug.WriteLine($"Size: {Size}", ConsoleColor.White);
                    Common.Debug.WriteLine($"Size in GB: {SizeInGB}", ConsoleColor.White);

                    if (Serial == diskSN)
                    {
                        _diskId = DeviceID;
                        _diskName = Model;
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

        private string convertSize(double size)
        {
            String[] units = new String[] { "B", "KB", "MB", "GB", "TB", "PB" };

            double mod = 1024.0;

            int i = 0;

            while (size >= mod)
            {
                size /= mod;
                i++;
            }
            return Math.Round(size, 2) + units[i];//with 2 decimals
        }
    }
}
