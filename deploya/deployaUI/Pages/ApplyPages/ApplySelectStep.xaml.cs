using deployaCore;
using deployaCore.Common;
using deployaUI.Common;
using Newtonsoft.Json;
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
        private int _driverCount = 0;

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

            Common.ApplyDetails.UseNTLDR = DiskSelectStep.ContentWindow.IsNTLDRChecked();
            Common.ApplyDetails.UseRecovery = DiskSelectStep.ContentWindow.IsRecoveryChecked();

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
            var img = new ImageSourceConverter();
            try
            {
                ImageIcon.Source = (ImageSource)img.ConvertFromString(Common.ApplyDetails.IconPath);
            }
            catch
            {
                // ignored
            }

            // Background worker for deployment
            applyBackgroundWorker = new BackgroundWorker();
            applyBackgroundWorker.WorkerReportsProgress = true;
            applyBackgroundWorker.WorkerSupportsCancellation = true;
            applyBackgroundWorker.DoWork += ApplyWim;
            applyBackgroundWorker.ProgressChanged += applyBackgroundWorker_ProgressChanged;
            applyBackgroundWorker.RunWorkerAsync();
        }

        private void applyBackgroundWorker_ProgressChanged(object? sender, ProgressChangedEventArgs e)
        {
            var responseJson = e.UserState as string;
            if (string.IsNullOrEmpty(responseJson)) return;
            
            var response = JsonConvert.DeserializeObject<ActionWorker>(responseJson);
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
                if (response.Message == "Job Done.")
                {
                    ProgrText.Text = 
                        File.Exists("X:\\Windows\\System32\\wpeutil.exe") 
                            ? "Installation completed. Press 'Next' to restart your computer." 
                            : "Installation completed. Press 'Next' to close Dive.";
                    ProgrBar.Value = 100;
                    if (ApplyContent.ContentWindow != null)
                        ApplyContent.ContentWindow.NextBtn.IsEnabled = true;
                    if (CloudContent.ContentWindow != null)
                        CloudContent.ContentWindow.NextBtn.IsEnabled = true;
                    return;
                }
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
                    {
                        ProgrBar.IsIndeterminate = false;
                        ProgrBar.Value = e.ProgressPercentage;
                    }
                    Debug.RefreshProgressBar(Progress.PrepareDisk, e.ProgressPercentage, response.Message);
                    break;
                }
                case Progress.ApplyImage:
                {
                    ProgrText.Text = response.Message;
                    if (response.IsIndeterminate)
                        ProgrBar.IsIndeterminate = true;
                    else
                    {
                        ProgrBar.IsIndeterminate = false;
                        ProgrBar.Value = e.ProgressPercentage;
                    }
                    Debug.RefreshProgressBar(Progress.ApplyImage, e.ProgressPercentage, response.Message);
                    break;
                }
                case Progress.InstallBootloader:
                {
                    ProgrText.Text = response.Message;
                    if (response.IsIndeterminate)
                        ProgrBar.IsIndeterminate = true;
                    else
                    {
                        ProgrBar.IsIndeterminate = false;
                        ProgrBar.Value = e.ProgressPercentage;
                    }
                    Debug.RefreshProgressBar(Progress.InstallBootloader, e.ProgressPercentage, response.Message);
                    break;
                }
                case Progress.InstallRecovery:
                {
                    ProgrText.Text = response.Message;
                    if (response.IsIndeterminate)
                        ProgrBar.IsIndeterminate = true;
                    else
                    {
                        ProgrBar.IsIndeterminate = false;
                        ProgrBar.Value = e.ProgressPercentage;
                    }
                    Debug.RefreshProgressBar(Progress.InstallRecovery, e.ProgressPercentage, response.Message);
                    break;
                }
                case Progress.InstallUnattend:
                {
                    ProgrText.Text = response.Message;
                    if (response.IsIndeterminate)
                        ProgrBar.IsIndeterminate = true;
                    else
                    {
                        ProgrBar.IsIndeterminate = false;
                        ProgrBar.Value = e.ProgressPercentage;
                    }
                    Debug.RefreshProgressBar(Progress.InstallUnattend, e.ProgressPercentage, response.Message);
                    break;
                }
                case Progress.InstallDrivers:
                {
                    ProgrText.Text = response.Message;
                    if (response.IsIndeterminate)
                        ProgrBar.IsIndeterminate = true;
                    else
                    {
                        ProgrBar.IsIndeterminate = false;
                        ProgrBar.Value = e.ProgressPercentage;
                    }
                    Debug.RefreshProgressBar(Progress.InstallDrivers, e.ProgressPercentage, response.Message);
                    break;
                }
                case Progress.InstallUefiSeven:
                {
                    ProgrText.Text = response.Message;
                    if (response.IsIndeterminate)
                        ProgrBar.IsIndeterminate = true;
                    else
                    {
                        ProgrBar.IsIndeterminate = false;
                        ProgrBar.Value = e.ProgressPercentage;
                    }
                    Debug.RefreshProgressBar(Progress.InstallUefiSeven, e.ProgressPercentage, response.Message);
                    break;
                }
            }
        }

        private void ApplyWim(object? sender, DoWorkEventArgs e)
        {
            #region Environment definition

            var worker = sender as BackgroundWorker;
            var firmware = Common.ApplyDetails.UseEFI switch // Firmware definition
            {
                true => Entities.Firmware.EFI,
                false => Entities.Firmware.BIOS
            };
            var bootloader = Common.ApplyDetails.UseNTLDR switch // Bootloader definition
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
                                    // Except if EFI is used, then we need to use the SeparateBoot partition layout
                                    if (Common.ApplyDetails.UseEFI)
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

            Common.Debug.Write("New Windows drive: ");
            Common.Debug.Write($"{windowsDrive}\n", true, ConsoleColor.DarkYellow);

            Common.Debug.Write("New Boot drive: ");
            Common.Debug.Write($"{bootDrive}\n", true, ConsoleColor.DarkYellow);

            Common.Debug.Write("New Recovery drive: ");
            Common.Debug.Write($"{recoveryDrive}\n", true, ConsoleColor.DarkYellow);

            Common.Debug.Write("Using partition style: ");
            Common.Debug.Write($"{partStyle}\n", true, ConsoleColor.DarkYellow);

            #endregion

            #region Prepare Disk

            // Prepare disk
            Actions.PrepareDisk(firmware, bootloader, Common.ApplyDetails.DiskIndex, partStyle, Common.ApplyDetails.UseRecovery, windowsDrive, bootDrive, recoveryDrive, worker);
            if (IsCanceled)
            {
                e.Cancel = true;
                return;
            }

            #endregion

            #region Apply Image

            // Apply image
            Actions.ApplyWim(windowsDrive, Common.ApplyDetails.FileName, Common.ApplyDetails.Index, worker);
            if (IsCanceled)
            {
                e.Cancel = true;
                return;
            }

            #endregion

            #region Install Bootloader

            // Install Bootloader
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

            #endregion

            #region Install Recovery (only for Vista and higher)

            // Install Recovery (only for Vista and higher)
            if (bootloader == Entities.Bootloader.BOOTMGR && Common.ApplyDetails.UseRecovery)
            {
                Actions.InstallRecovery($"{windowsDrive}Windows", recoveryDrive, Common.DeploymentOption.AddDiveToWinRE, worker);
            
                if (IsCanceled)
                {
                    e.Cancel = true;
                    return;
                }
            }

            #endregion

            #region Install unattend file (only for Vista and higher)

            // Install unattend file (only for Vista and higher)
            if (Common.DeploymentInfo.UseUserInfo || Common.OemInfo.UseOemInfo)
            {
                // Building config
                var config = "";
                Common.UnattendMode? um = null;

                if (!Common.DeploymentInfo.UseUserInfo && Common.OemInfo.UseOemInfo)
                {
                    um = Common.UnattendMode.OnlyOem;
                }
                else
                {
                    // Administrator / User with OEM infos
                    if (!string.IsNullOrEmpty(Common.DeploymentInfo.Username)
                        && !string.IsNullOrEmpty(Common.DeploymentInfo.Password)
                        && Common.OemInfo.UseOemInfo)
                    {
                        um = Common.DeploymentInfo.Username != "Administrator" ? UnattendMode.User : UnattendMode.Admin;
                    }

                    // Administrator / User with OEM infos, but without password
                    if (!string.IsNullOrEmpty(Common.DeploymentInfo.Username)
                        && string.IsNullOrEmpty(Common.DeploymentInfo.Password)
                        && Common.OemInfo.UseOemInfo)
                    {
                        um = Common.DeploymentInfo.Username != "Administrator" ? UnattendMode.UserWithoutPassword : UnattendMode.AdminWithoutPassword;
                    }

                    // Administrator / User without OEM infos
                    if (Common.DeploymentInfo.Username == "Administrator"
                        && !string.IsNullOrEmpty(Common.DeploymentInfo.Password)
                        && Common.OemInfo.UseOemInfo == false)
                    {
                        um = Common.DeploymentInfo.Username != "Administrator" ? UnattendMode.UserWithoutOem : UnattendMode.AdminWithoutOem;
                    }

                    // Administrator without OEM infos and password
                    if (Common.DeploymentInfo.Username == "Administrator"
                        && string.IsNullOrEmpty(Common.DeploymentInfo.Password)
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
                 
                Actions.InstallUnattend($"{windowsDrive}Windows", config, Common.OemInfo.LogoPath, Common.DeploymentOption.UseSMode, worker);

                if (IsCanceled)
                {
                    e.Cancel = true;
                    return;
                }
            }


            #endregion

            #region Install Drivers (only for Vista and higher)
            
            // Install Drivers (only for Vista and higher)
            if (Common.ApplyDetails.DriverList != null)
            {
                _driverCount = Common.ApplyDetails.DriverList.Count;
                Actions.InstallDriver(windowsDrive, Common.ApplyDetails.DriverList, worker);

                if (IsCanceled)
                {
                    e.Cancel = true;
                    return;
                }
            }

            #endregion

            #region Install UefiSeven (only for Vista and 7 with EFI)

            // Install UefiSeven (only for Vista and 7 with EFI)
            if (Common.WindowsModification.InstallUefiSeven)
            {
                deployaCore.Action.UefiSeven.InstallUefiSeven(bootDrive, 
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

            #endregion

            // Installation complete
            worker?.ReportProgress(100, JsonConvert.SerializeObject(new ActionWorker
            {
                IsDebug = true,
                Message = "Job Done."
            }));
        }
    }
}
