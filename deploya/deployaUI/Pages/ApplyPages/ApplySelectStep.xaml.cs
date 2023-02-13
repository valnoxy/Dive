using deployaCore;
using deployaCore.Common;
using deployaUI.Common;
using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Media;

namespace deployaUI.Pages.ApplyPages
{
    /// <summary>
    /// Interaktionslogik für ApplySelectStep.xaml
    /// </summary>
    public partial class ApplySelectStep : System.Windows.Controls.UserControl
    {
        private BackgroundWorker applyBackgroundWorker;
        bool IsCanceled = false;
        private int currentDriver = 0;
        private int driverCount = 0;

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

            // Validate deployment settings
            switch (Common.OemInfo.UseOemInfo)
            {
                case true:
                    if (Common.OemInfo.SupportPhone == null
                        && Common.OemInfo.LogoPath == null
                        && Common.OemInfo.Manufacturer == null
                        && Common.OemInfo.Model == null
                        && Common.OemInfo.SupportHours == null
                        && Common.OemInfo.SupportURL == null)
                        Common.OemInfo.UseOemInfo = false;
                    break;
            }

            switch (Common.DeploymentInfo.UseUserInfo)
            {
                case true:
                    if (String.IsNullOrEmpty(Common.DeploymentInfo.Username)
                        && String.IsNullOrEmpty(Common.DeploymentInfo.Password) == null)
                        Common.DeploymentInfo.UseUserInfo = false;
                    break;
            }

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
            //   306: Failed at copying oem logo
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
                case 206:                           // 206: ProgText -> Injecting drivers
                    ProgrText.Text = $"Injecting drivers ({currentDriver} of {driverCount}) ...";
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
                case 306:                           // 306: Failed at copying oem logo
                    ProgrText.Text = "Failed at copying oem logo to disk. Please check your config and try again.";
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
                                // If Vista is used, we need to use the Single partition layout
                                if (Common.ApplyDetails.Name.ToLower().Contains("windows vista"))
                                {
                                    letters = Actions.GetSystemLetters(Entities.PartitionStyle.Single);
                                    partStyle = Entities.PartitionStyle.Single;
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

            Output.WriteLine($"Windows drive: {windowsDrive}");
            Output.WriteLine($"Boot drive: {bootDrive}");
            Output.WriteLine($"Recovery drive: {recoveryDrive}");
            Output.WriteLine($"Partition style: {partStyle}");

            #endregion


            // Initialize worker progress
            worker.ReportProgress(0, "");       // Value 0

            // Prepare disk
            worker.ReportProgress(201, "");     // Prepare Disk Text
            Actions.PrepareDisk(firmware, bootloader, ui, Common.ApplyDetails.DiskIndex, partStyle, Common.ApplyDetails.UseRecovery, windowsDrive, bootDrive, recoveryDrive, worker);
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
            switch (partStyle)
            {
                case Entities.PartitionStyle.SeparateBoot:
                case Entities.PartitionStyle.Full:
                    Actions.InstallBootloader(firmware, bootloader, ui, windowsDrive, bootDrive, worker);
                    break;
                case Entities.PartitionStyle.Single:
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
            if (Common.DeploymentInfo.UseUserInfo || Common.OemInfo.UseOemInfo)
            {
                worker.ReportProgress(205, "");     // Installing unattend file

                // Building config
                string config = "";
                Common.UnattendMode? um = null;

                if (!Common.DeploymentInfo.UseUserInfo && Common.OemInfo.UseOemInfo)
                {
                    um = Common.UnattendMode.OnlyOem;
                }
                else
                {
                    // Administrator / User with OEM infos
                    if (!String.IsNullOrEmpty(Common.DeploymentInfo.Username)
                        && !String.IsNullOrEmpty(Common.DeploymentInfo.Password)
                        && Common.OemInfo.UseOemInfo)
                    {
                        um = Common.DeploymentInfo.Username != "Administrator" ? UnattendMode.User : UnattendMode.Admin;
                    }

                    // Administrator / User with OEM infos, but without password
                    if (!String.IsNullOrEmpty(Common.DeploymentInfo.Username)
                        && String.IsNullOrEmpty(Common.DeploymentInfo.Password)
                        && Common.OemInfo.UseOemInfo)
                    {
                        um = Common.DeploymentInfo.Username != "Administrator" ? UnattendMode.UserWithoutPassword : UnattendMode.AdminWithoutPassword;
                    }

                    // Administrator / User without OEM infos
                    if (Common.DeploymentInfo.Username == "Administrator"
                        && !String.IsNullOrEmpty(Common.DeploymentInfo.Password)
                        && Common.OemInfo.UseOemInfo == false)
                    {
                        um = Common.DeploymentInfo.Username != "Administrator" ? UnattendMode.UserWithoutOem : UnattendMode.AdminWithoutOem;
                    }

                    // Administrator without OEM infos and password
                    if (Common.DeploymentInfo.Username == "Administrator"
                        && String.IsNullOrEmpty(Common.DeploymentInfo.Password)
                        && Common.OemInfo.UseOemInfo == false)
                    {
                        um = Common.DeploymentInfo.Username != "Administrator" ? UnattendMode.UserWithoutPasswordAndOem : UnattendMode.AdminWithoutPasswordAndOem;
                    }
                }
                
                // Custom file
                if (File.Exists(Common.DeploymentInfo.CustomFilePath))
                {
                    config = File.ReadAllText(Common.DeploymentInfo.CustomFilePath);
                }

                if (config == "")
                    config = Common.UnattendBuilder.Build(um);

                Debug.WriteLine(config);
                 
                Actions.InstallUnattend(ui, $"{windowsDrive}Windows", config, Common.OemInfo.LogoPath, Common.DeploymentOption.UseSMode, worker);

                if (IsCanceled)
                {
                    e.Cancel = true;
                    return;
                }
            }

            // Install Drivers (only for Vista and higher)
            if (Common.ApplyDetails.DriverList != null)
            {
                worker.ReportProgress(206, "");     // Installing Drivers Text
                Actions.InstallDriver(ui, $"{windowsDrive}Windows", Common.ApplyDetails.DriverList, worker);

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
