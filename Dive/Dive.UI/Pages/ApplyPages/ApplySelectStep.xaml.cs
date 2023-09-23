using Dive.Core;
using Dive.Core.Common;
using Dive.UI.Common;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Media;

namespace Dive.UI.Pages.ApplyPages
{
    /// <summary>
    /// Interaktionslogik für ApplySelectStep.xaml
    /// </summary>
    public partial class ApplySelectStep
    {
        private readonly BackgroundWorker _applyBackgroundWorker;
        private bool _isCanceled = false;
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

            ApplyDetails.UseNTLDR = DiskSelectStep.ContentWindow!.IsNTLDRChecked();
            ApplyDetails.UseRecovery = DiskSelectStep.ContentWindow.IsRecoveryChecked();

            // Validate deployment settings
            switch (OemInfo.UseOemInfo)
            {
                case true:
                    if (OemInfo.SupportPhone == null
                        && OemInfo.LogoPath == null
                        && OemInfo.Manufacturer == null
                        && OemInfo.Model == null
                        && OemInfo.SupportHours == null
                        && OemInfo.SupportURL == null)
                        OemInfo.UseOemInfo = false;
                    break;
            }

            switch (DeploymentInfo.UseUserInfo)
            {
                case true:
                    if (String.IsNullOrEmpty(DeploymentInfo.Username)
                        && String.IsNullOrEmpty(DeploymentInfo.Password) == null)
                        DeploymentInfo.UseUserInfo = false;
                    break;
            }

            // Set active Image to card
            ImageName.Text = ApplyDetails.Name;
            ImageFile.Text = ApplyDetails.FileName;
            var img = new ImageSourceConverter();
            try
            {
                ImageIcon.Source = (ImageSource)img.ConvertFromString(ApplyDetails.IconPath);
            }
            catch
            {
                // ignored
            }

            // Background worker for deployment
            _applyBackgroundWorker = new BackgroundWorker();
            _applyBackgroundWorker.WorkerReportsProgress = true;
            _applyBackgroundWorker.WorkerSupportsCancellation = true;
            _applyBackgroundWorker.DoWork += ApplyWim;
            _applyBackgroundWorker.ProgressChanged += applyBackgroundWorker_ProgressChanged;
            _applyBackgroundWorker.RunWorkerAsync();
        }

        private void applyBackgroundWorker_ProgressChanged(object? sender, ProgressChangedEventArgs e)
        {
            var responseJson = e.UserState as string;
            if (string.IsNullOrEmpty(responseJson)) return;

            try
            {
                var response = JsonConvert.DeserializeObject<ActionWorker>(responseJson);
                if (response!.IsError)
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

                    _isCanceled = true;
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
            catch (Exception ex)
            {
                Debug.WriteLine($"Error while parsing JSON response: {ex.Message}", ConsoleColor.Red);
            }
        }

        private void ApplyWim(object? sender, DoWorkEventArgs e)
        {
            #region Environment definition

            var worker = sender as BackgroundWorker;
            var firmware = ApplyDetails.UseEFI switch // Firmware definition
            {
                true => Entities.Firmware.EFI,
                false => Entities.Firmware.BIOS
            };
            var bootloader = ApplyDetails.UseNTLDR switch // Bootloader definition
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

            switch (ApplyDetails.UseNTLDR)
            {
                // pre-Vista
                case true:
                    letters = Actions.GetSystemLetters(Entities.PartitionStyle.Single);
                    partStyle = Entities.PartitionStyle.Single;
                    break;

                case false:
                {
                    switch (ApplyDetails.UseRecovery)
                    {
                            case true:
                                letters = Actions.GetSystemLetters(Entities.PartitionStyle.Full);
                                partStyle = Entities.PartitionStyle.Full;
                                break;

                            case false:
                                // If Vista is used, we need to use the Single partition layout
                                if (ApplyDetails.Name.ToLower().Contains("windows vista"))
                                {
                                    // Except if EFI is used, then we need to use the SeparateBoot partition layout
                                    if (ApplyDetails.UseEFI)
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


            Debug.Write("Using partition style: ");
            Debug.Write($"{partStyle}\n", true, ConsoleColor.DarkYellow);

            Debug.Write("New Windows drive: ");
            Debug.Write($"{windowsDrive}\n", true, ConsoleColor.DarkYellow);

            Debug.Write("New Boot drive: ");
            Debug.Write($"{bootDrive}\n", true, ConsoleColor.DarkYellow);

            Debug.Write("New Recovery drive: ");
            Debug.Write($"{recoveryDrive}\n", true, ConsoleColor.DarkYellow);

            #endregion

            #region Prepare Disk

            // Prepare disk
            Actions.PrepareDisk(firmware, bootloader, ApplyDetails.DiskIndex, partStyle, ApplyDetails.UseRecovery, windowsDrive, bootDrive, recoveryDrive, worker);
            if (_isCanceled)
            {
                e.Cancel = true;
                return;
            }

            #endregion

            #region Apply Image

            // Apply image
            Actions.ApplyWim(windowsDrive, ApplyDetails.FileName, ApplyDetails.Index, worker);
            if (_isCanceled)
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
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (_isCanceled)
            {
                e.Cancel = true;
                return;
            }

            #endregion

            #region Install Recovery (only for Vista and higher)

            // Install Recovery (only for Vista and higher)
            if (bootloader == Entities.Bootloader.BOOTMGR && ApplyDetails.UseRecovery)
            {
                Actions.InstallRecovery($"{windowsDrive}Windows", recoveryDrive, DeploymentOption.AddDiveToWinRE, worker);
            
                if (_isCanceled)
                {
                    e.Cancel = true;
                    return;
                }
            }

            #endregion

            #region Install unattend file (only for Vista and higher)

            // Install unattend file (only for Vista and higher)
            if (DeploymentInfo.UseUserInfo || OemInfo.UseOemInfo)
            {
                Debug.WriteLine("Building config...");

                // Building config
                var config = "";
                UnattendMode? um = null;

                if (!DeploymentInfo.UseUserInfo && OemInfo.UseOemInfo)
                {
                    um = UnattendMode.OnlyOem;
                }
                else
                {
                    // Administrator / User with OEM infos
                    if (!string.IsNullOrEmpty(DeploymentInfo.Username)
                        && !string.IsNullOrEmpty(DeploymentInfo.Password)
                        && OemInfo.UseOemInfo)
                    {
                        um = DeploymentInfo.Username != "Administrator" ? UnattendMode.User : UnattendMode.Admin;
                    }

                    // Administrator / User with OEM infos, but without password
                    if (!string.IsNullOrEmpty(DeploymentInfo.Username)
                        && string.IsNullOrEmpty(DeploymentInfo.Password)
                        && OemInfo.UseOemInfo)
                    {
                        um = DeploymentInfo.Username != "Administrator" ? UnattendMode.UserWithoutPassword : UnattendMode.AdminWithoutPassword;
                    }

                    // Administrator / User without OEM infos
                    if (!string.IsNullOrEmpty(DeploymentInfo.Username)
                        && !string.IsNullOrEmpty(DeploymentInfo.Password)
                        && OemInfo.UseOemInfo == false)
                    {
                        um = DeploymentInfo.Username != "Administrator" ? UnattendMode.UserWithoutOem : UnattendMode.AdminWithoutOem;
                    }

                    // Administrator / User without OEM infos and password
                    if (!string.IsNullOrEmpty(DeploymentInfo.Username)
                        && string.IsNullOrEmpty(DeploymentInfo.Password)
                        && OemInfo.UseOemInfo == false)
                    {
                        um = DeploymentInfo.Username != "Administrator" ? UnattendMode.UserWithoutPasswordAndOem : UnattendMode.AdminWithoutPasswordAndOem;
                    }
                }
                
                // Custom file
                config = File.Exists(DeploymentInfo.CustomFilePath) ? File.ReadAllText(DeploymentInfo.CustomFilePath) : UnattendBuilder.Build(um);

                if (string.IsNullOrEmpty(config)) 
                    throw new Exception("Could not build or read unattend configuration file.");
                //Debug.WriteLine(config);
                 
                Actions.InstallUnattend($"{windowsDrive}Windows", config, OemInfo.LogoPath, DeploymentOption.UseSMode, worker);
                Debug.Write($"Configuration file written to ");
                Debug.WriteLine($"{windowsDrive}Windows\\Panther\\unattend.xml", ConsoleColor.DarkYellow);

                if (_isCanceled)
                {
                    e.Cancel = true;
                    return;
                }
            }


            #endregion

            #region Install Drivers (only for Vista and higher)
            
            // Install Drivers (only for Vista and higher)
            if (ApplyDetails.DriverList != null)
            {
                _driverCount = ApplyDetails.DriverList.Count;
                Actions.InstallDriver(windowsDrive, ApplyDetails.DriverList, worker);

                if (_isCanceled)
                {
                    e.Cancel = true;
                    return;
                }
            }

            #endregion

            #region Install UefiSeven (only for Vista and 7 with EFI)

            // Install UefiSeven (only for Vista and 7 with EFI)
            if (WindowsModification.InstallUefiSeven)
            {
                Dive.Core.Action.UefiSeven.InstallUefiSeven(bootDrive, 
                    WindowsModification.UsToggleSkipErros, 
                    WindowsModification.UsToggleFakeVesa,
                    WindowsModification.UsToggleVerbose,
                    WindowsModification.UsToggleLog, 
                    worker);

                if (_isCanceled)
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
