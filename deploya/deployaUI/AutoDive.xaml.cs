using System;
using System.IO;
using System.Windows.Media;
using deploya_core;
using System.Xml;
using deployaUI.Common;
using System.Management;
using System.ComponentModel;

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

                UseDeploymentInfo.Text = _defaultUsername != null || _defaultPassword != null
                    ? "Using Deployment Info"
                    : "Standard Installation";
                UseAutoInit.Text = _useAutoInit ? "Using AutoInit" : "No AutoInit";
                Firmware.Text = Common.ApplyDetails.UseEFI ? "EFI" : "BIOS";
                Bootloader.Text = Common.ApplyDetails.UseNTLDR ? "NTLDR" : "BOOTMGR";

                try
                {
                    ImageSourceConverter img = new ImageSourceConverter();
                    ImageIcon.Source = (ImageSource)img.ConvertFromString(Common.ApplyDetails.IconPath)!;
                }
                catch
                {
                    // ignored
                }
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

            // Update unattend.xml config
            Common.DeploymentInfo.PreConfigUserPass = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<unattend xmlns=""urn:schemas-microsoft-com:unattend"">
    <settings pass=""oobeSystem"">
        <component name=""Microsoft-Windows-Shell-Setup"" processorArchitecture=""amd64"" publicKeyToken=""31bf3856ad364e35"" language=""neutral"" versionScope=""nonSxS"" xmlns:wcm=""http://schemas.microsoft.com/WMIConfig/2002/State"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
            <UserAccounts>
                <LocalAccounts>
                    <LocalAccount wcm:action=""add"">
                        <Password>
                            <Value>{Common.DeploymentInfo.Password}</Value>
                            <PlainText>true</PlainText>
                        </Password>
                        <Name>{Common.DeploymentInfo.Username}</Name>
                        <Group>Administratoren</Group>
                    </LocalAccount>
                </LocalAccounts>
            </UserAccounts>
        </component>
    </settings>
    <cpi:offlineImage cpi:source=""wim:e:/wims/win11-beta.wim#Windows 11 Pro"" xmlns:cpi=""urn:schemas-microsoft-com:cpi"" />
</unattend>";

            Common.DeploymentInfo.PreConfigAdminPass = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<unattend xmlns=""urn:schemas-microsoft-com:unattend"">
    <settings pass=""oobeSystem"">
        <component name=""Microsoft-Windows-Shell-Setup"" processorArchitecture=""amd64"" publicKeyToken=""31bf3856ad364e35"" language=""neutral"" versionScope=""nonSxS"" xmlns:wcm=""http://schemas.microsoft.com/WMIConfig/2002/State"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
            <UserAccounts>
                <AdministratorPassword>
                    <Value>{Common.DeploymentInfo.Username}</Value>
                    <PlainText>true</PlainText>
                </AdministratorPassword>
            </UserAccounts>
            <AutoLogon>
                <Username>Administrator</Username>
                <Password>
                    <Value>{Common.DeploymentInfo.Password}</Value>
                    <PlainText>true</PlainText>
                </Password>
            </AutoLogon>
        </component>
    </settings>
    <cpi:offlineImage cpi:source=""wim:e:/wims/win11-beta.wim#Windows 11 Pro"" xmlns:cpi=""urn:schemas-microsoft-com:cpi"" />
</unattend>
";

            Common.DeploymentInfo.PreConfigAdminWithoutPass = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<unattend xmlns=""urn:schemas-microsoft-com:unattend"">
    <settings pass=""oobeSystem"">
        <component name=""Microsoft-Windows-Shell-Setup"" processorArchitecture=""amd64"" publicKeyToken=""31bf3856ad364e35"" language=""neutral"" versionScope=""nonSxS"" xmlns:wcm=""http://schemas.microsoft.com/WMIConfig/2002/State"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
            <AutoLogon>
                <Username>Administrator</Username>
            </AutoLogon>
        </component>
    </settings>
    <cpi:offlineImage cpi:source=""wim:e:/wims/win11-beta.wim#Windows 11 Pro"" xmlns:cpi=""urn:schemas-microsoft-com:cpi"" />
</unattend>
";

            // Backgrond worker for deployment
            applyBackgroundWorker = new BackgroundWorker();
            applyBackgroundWorker.WorkerReportsProgress = true;
            applyBackgroundWorker.WorkerSupportsCancellation = true;
            applyBackgroundWorker.DoWork += ApplyWim;
            applyBackgroundWorker.ProgressChanged += applyBackgroundWorker_ProgressChanged;
            applyBackgroundWorker.RunWorkerAsync();
        }

        #region Applying functions

        private BackgroundWorker applyBackgroundWorker;
        bool IsCanceled = false;

        private void applyBackgroundWorker_ProgressChanged(object? sender, ProgressChangedEventArgs e)
        {
            //
            // Value list
            // --------------------------
            // Progressbar handling
            //   101: Progressbar is not Indeterminate
            //   102: Progressbar is Indeterminate
            //
            // Standard Message handling
            //   201: ProgText -> Prepare Disk
            //   202: ProgText -> Apply WIM
            //   203: ProgText -> Install Bootloader
            //   204: ProgText -> Install recovery
            //   205: ProgText -> Install unattend.xml
            //   250: Installation complete
            //
            // Error message handling
            //   301: Failed at preparing disk
            //   302: Failed at applying WIM
            //   303: Failed at installing bootloader
            //   304: Failed at installing recovery
            //   305: Failed at installing unattend.xml
            //
            // Range 0-100 -> Progressbar percentage
            //

            // Progress bar handling
            switch (e.ProgressPercentage)
            {
                #region Progress bar settings
                case 101:                           // 101: Progressbar is not Indeterminate
                    ProgrBar.IsIndeterminate = false;
                    break;
                case 102:                           // 102: Progressbar is Indeterminate
                    ProgrBar.IsIndeterminate = true;
                    break;
                #endregion

                #region Standard message handling
                case 201:                           // 201: ProgText -> Prepare Disk
                    ProgrText.Text = "Preparing disk ...";
                    break;
                case 202:                           // 202: ProgText -> Applying WIM
                    ProgrText.Text = $"Applying Image to disk ({ProgrBar.Value}%) ...";
                    break;
                case 203:                           // 203: ProgText -> Installing Bootloader
                    ProgrText.Text = "Installing bootloader to disk ...";
                    break;
                case 204:                           // 204: ProgText -> Installing recovery
                    ProgrText.Text = "Registering recovery partition to Windows ...";
                    break;
                case 205:                           // 205: ProgText -> Installing unattend.xml
                    ProgrText.Text = "Copying unattend.xml to disk ...";
                    break;
                case 250:                           // 250: Installation complete
                    ProgrText.Text = "Installation completed. Restarting now ...";
                    ProgrBar.Value = 100;
                    if (System.IO.File.Exists("X:\\Windows\\System32\\wpeutil.exe"))
                        System.Diagnostics.Process.Start("wpeutil.exe", "reboot");
                    else
                        System.Diagnostics.Process.Start("shutdown.exe", "-r -t 0");
                    break;
                #endregion

                #region Error message handling
                case 301:                           // 301: Failed at preparing disk
                    ExceptionMessage("Failed at preparing disk. Please check your disk and try again.");
                    IsCanceled = true;
                    break;
                case 302:                           // 302: Failed at applying WIM
                    ExceptionMessage("Failed at applying WIM. Please check your image and try again.");
                    IsCanceled = true;
                    break;
                case 303:                           // 303: Failed at installing bootloader
                    ExceptionMessage("Failed at installing bootloader. Please check your image and try again.");
                    IsCanceled = true;
                    break;
                case 304:                           // 304: Failed at installing recovery
                    ExceptionMessage("Failed at installing recovery. Please check your image and try again.");
                    break;
                case 305:                           // 305: Failed at installing unattend.xml
                    ExceptionMessage("Failed at copying unattend.xml to disk. Please check your image or config and try again.");
                    break;
                #endregion
            }

            // Progressbar percentage
            if (e.ProgressPercentage <= 100)
                this.ProgrBar.Value = e.ProgressPercentage;
        }

        private void ApplyWim(object? sender, DoWorkEventArgs e)
        {
            #region Environment definition

            BackgroundWorker worker = sender as BackgroundWorker;

            Entities.Firmware firmware = new Entities.Firmware();
            Entities.Bootloader bootloader = new Entities.Bootloader();
            Entities.UI ui = new Entities.UI();

            // UI definition
            ui = Entities.UI.Graphical;

            firmware = Common.ApplyDetails.UseEFI switch
            {
                // Firmware definition
                true => Entities.Firmware.EFI,
                false => Entities.Firmware.BIOS
            };

            bootloader = Common.ApplyDetails.UseNTLDR switch
            {
                // Bootloader definition
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

            switch (Common.ApplyDetails.UseNTLDR)
            {
                // pre-Vista
                case true:
                    letters = Actions.GetSystemLetters(Entities.PartitionStyle.Single);
                    partStyle = Entities.PartitionStyle.Single;
                    break;

                case false:
                    {
                        switch (Common.ApplyDetails.UseRecovery)
                        {
                            case true:
                                letters = Actions.GetSystemLetters(Entities.PartitionStyle.Full);
                                partStyle = Entities.PartitionStyle.Full;
                                break;

                            case false:
                                letters = Actions.GetSystemLetters(Entities.PartitionStyle.SeparateBoot);
                                partStyle = Entities.PartitionStyle.SeparateBoot;
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
            }

            #endregion


            // Initialize worker progress
            worker.ReportProgress(0, "");       // Value 0

            // Prepare disk
            worker.ReportProgress(201, "");     // Prepare Disk Text
            Actions.PrepareDisk(firmware, bootloader, ui, Common.ApplyDetails.DiskIndex, Common.ApplyDetails.UseRecovery, windowsDrive, bootDrive, recoveryDrive, worker);
            if (IsCanceled)
            {
                e.Cancel = true;
                return;
            }

            // Apply image
            worker.ReportProgress(202, "");     // Applying Image Text
            worker.ReportProgress(0, "");       // Value 0
            Actions.ApplyWIM(ui, windowsDrive, Common.ApplyDetails.FileName, Common.ApplyDetails.Index, worker);
            if (IsCanceled)
            {
                e.Cancel = true;
                return;
            }

            // Install Bootloader
            worker.ReportProgress(203, "");     // Installing Bootloader Text
            switch (bootloader)
            {
                case Entities.Bootloader.BOOTMGR:
                    Actions.InstallBootloader(firmware, bootloader, ui, $"{windowsDrive}Windows", bootDrive, worker);
                    break;
                case Entities.Bootloader.NTLDR:
                    Actions.InstallBootloader(firmware, bootloader, ui, windowsDrive, windowsDrive, worker);
                    break;
            }

            if (IsCanceled)
            {
                e.Cancel = true;
                return;
            }

            // Install Recovery (only for Vista and higher)
            if (bootloader == Entities.Bootloader.BOOTMGR && Common.ApplyDetails.UseRecovery)
            {
                worker.ReportProgress(204, "");     // Installing Bootloader Text
                Actions.InstallRecovery(ui, $"{windowsDrive}Windows", recoveryDrive, worker);

                if (IsCanceled)
                {
                    e.Cancel = true;
                    return;
                }
            }

            // Install unattend file (only for Vista and higher)
            if (Common.DeploymentInfo.Username != null || Common.DeploymentInfo.CustomFilePath != null)
            {
                worker.ReportProgress(205, "");     // Installing unattend file Text

                // Building config
                string config = "";
                if (Common.DeploymentInfo.Username != null && Common.DeploymentInfo.Password != null)
                {
                    config = Common.DeploymentInfo.PreConfigUserPass;
                }

                if (Common.DeploymentInfo.Username == "Administrator")
                {
                    if (Common.DeploymentInfo.Password != null)
                        config = Common.DeploymentInfo.PreConfigAdminWithoutPass;
                    else
                        config = Common.DeploymentInfo.PreConfigAdminPass;
                }

                if (File.Exists(Common.DeploymentInfo.CustomFilePath))
                {
                    config = File.ReadAllText(Common.DeploymentInfo.CustomFilePath);
                }

                Console.WriteLine(config);

                Actions.InstallUnattend(ui, $"{windowsDrive}Windows", config, worker);

                if (IsCanceled)
                {
                    e.Cancel = true;
                    return;
                }
            }

            // Installation complete
            worker.ReportProgress(250, "");     // Installation complete Text
        }

        #endregion

        #region Modules

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

        #endregion
    }
}
