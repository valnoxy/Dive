using deploya_core;
using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Media;

namespace deployaUI.Pages.ApplyPages
{
    /// <summary>
    /// Interaktionslogik für ApplySelectStep.xaml
    /// </summary>
    public partial class ApplySelectStep : UserControl
    {
        private BackgroundWorker applyBackgroundWorker;
        bool IsCanceled = false;
        
        public ApplySelectStep()
        {
            InitializeComponent();

            if (ApplyContent.ContentWindow != null)
            {
                ApplyContent.ContentWindow.NextBtn.IsEnabled = false;
                ApplyContent.ContentWindow.BackBtn.IsEnabled = false;
                ApplyContent.ContentWindow.CancelBtn.IsEnabled = false;
            }
            if (CloudContent.ContentWindow != null)
            {
                CloudContent.ContentWindow.NextBtn.IsEnabled = false;
                CloudContent.ContentWindow.BackBtn.IsEnabled = false;
                CloudContent.ContentWindow.CancelBtn.IsEnabled = false;
            }

            if (DiskSelectStep.ContentWindow.IsNTLDRChecked())
                Common.ApplyDetails.UseNTLDR = true;
            else 
                Common.ApplyDetails.UseNTLDR = false;

            if (DiskSelectStep.ContentWindow.IsRecoveryChecked())
                Common.ApplyDetails.UseRecovery = true;
            else
                Common.ApplyDetails.UseRecovery = false;

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

            Common.DeploymentInfo.PreConfigOnlyUser = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<unattend xmlns=""urn:schemas-microsoft-com:unattend"">
    <settings pass=""oobeSystem"">
        <component name=""Microsoft-Windows-Shell-Setup"" processorArchitecture=""amd64"" publicKeyToken=""31bf3856ad364e35"" language=""neutral"" versionScope=""nonSxS"" xmlns:wcm=""http://schemas.microsoft.com/WMIConfig/2002/State"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
            <UserAccounts>
                <LocalAccounts>
                    <LocalAccount wcm:action=""add"">
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
                    <Value>{Common.DeploymentInfo.Password}</Value>
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

            // Set active Image to card
            ImageName.Text = Common.ApplyDetails.Name;
            ImageFile.Text = Common.ApplyDetails.FileName;
            ImageSourceConverter img = new ImageSourceConverter();
            try
            {
                ImageIcon.Source = (ImageSource)img.ConvertFromString(Common.ApplyDetails.IconPath);
            }
            catch
            {
                // ignored
            }

            // Backgrond worker for deployment
            applyBackgroundWorker = new BackgroundWorker();
            applyBackgroundWorker.WorkerReportsProgress = true;
            applyBackgroundWorker.WorkerSupportsCancellation = true;
            applyBackgroundWorker.DoWork += ApplyWim;
            applyBackgroundWorker.ProgressChanged += applyBackgroundWorker_ProgressChanged;
            applyBackgroundWorker.RunWorkerAsync();
        }

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
                    ProgrText.Text = "Installation completed. Press 'Next' to restart your computer.";
                    ProgrBar.Value = 100;
                    if (ApplyContent.ContentWindow != null)
                        ApplyContent.ContentWindow.NextBtn.IsEnabled = true;
                    if (CloudContent.ContentWindow != null)
                        CloudContent.ContentWindow.NextBtn.IsEnabled = true;
                    break;
                #endregion

                #region Error message handling
                case 301:                           // 301: Failed at preparing disk
                    ProgrText.Text = "Failed at preparing disk. Please check your disk and try again.";
                    ProgrBar.Value = 0;
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
                    break;
                case 302:                           // 302: Failed at applying WIM
                    ProgrText.Text = "Failed at applying WIM. Please check your image and try again.";
                    ProgrBar.Value = 0;
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
                    break;
                case 303:                           // 303: Failed at installing bootloader
                    ProgrText.Text = "Failed at installing bootloader. Please check your image and try again.";
                    ProgrBar.Value = 0;
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
                    break;
                case 304:                           // 304: Failed at installing recovery
                    ProgrText.Text = "Failed at installing recovery. Please check your image and try again.";
                    ProgrBar.Value = 0;
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
                    break;
                case 305:                           // 305: Failed at installing unattend.xml
                    ProgrText.Text = "Failed at copying unattend.xml to disk. Please check your image or config and try again.";
                    ProgrBar.Value = 0;
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

            Output.WriteLine($"Windows drive: {windowsDrive}");
            Output.WriteLine($"Boot drive: {bootDrive}");
            Output.WriteLine($"Recovery drive: {recoveryDrive}");
            Output.WriteLine($"Partition style: {partStyle}");

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

                if (Common.DeploymentInfo.Username != null && Common.DeploymentInfo.Password == null)
                {
                    config = Common.DeploymentInfo.PreConfigOnlyUser;
                }

                if (Common.DeploymentInfo.Username == "Administrator")
                {
                    config = Common.DeploymentInfo.Password != null ? Common.DeploymentInfo.PreConfigAdminWithoutPass : Common.DeploymentInfo.PreConfigAdminPass;
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
    }
}
