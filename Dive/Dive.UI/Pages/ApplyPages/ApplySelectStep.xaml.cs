using Dive.Core.Common;
using Dive.UI.Common;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Media;
using Dive.UI.Common.Deployment;

namespace Dive.UI.Pages.ApplyPages
{
    /// <summary>
    /// Interaktionslogik für ApplySelectStep.xaml
    /// </summary>
    public partial class ApplySelectStep
    {
        private readonly BackgroundWorker _applyBackgroundWorker = new();

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
            if (OemInfo.UseOemInfo)
            {
                if (string.IsNullOrEmpty(OemInfo.SupportPhone)
                    && string.IsNullOrEmpty(OemInfo.LogoPath)
                    && string.IsNullOrEmpty(OemInfo.Manufacturer)
                    && string.IsNullOrEmpty(OemInfo.Model)
                    && string.IsNullOrEmpty(OemInfo.SupportHours)
                    && string.IsNullOrEmpty(OemInfo.SupportURL))
                {
                    OemInfo.UseOemInfo = false;
                }
            }

            if (DeploymentInfo.UseUserInfo)
            {
                if (string.IsNullOrEmpty(DeploymentInfo.Username)
                    && string.IsNullOrEmpty(DeploymentInfo.Password))
                {
                    DeploymentInfo.UseUserInfo = false;
                }
            }


            // Set active Image to card
            ImageName.Text = ApplyDetails.Name;
            ImageFile.Text = ApplyDetails.FileName;
            var img = new ImageSourceConverter();
            try
            {
                ImageIcon.Source = (ImageSource)img.ConvertFromString(ApplyDetails.IconPath)!;
            }
            catch
            {
                // ignored
            }

            // Background worker for deployment
            _applyBackgroundWorker.WorkerReportsProgress = true;
            _applyBackgroundWorker.WorkerSupportsCancellation = true;
            _applyBackgroundWorker.DoWork += ApplyImage;
            _applyBackgroundWorker.ProgressChanged += ApplyBackgroundWorker_ProgressChanged;
            _applyBackgroundWorker.RunWorkerAsync();
        }

        private void ApplyImage(object? sender, DoWorkEventArgs e) => DeployImage.ApplyWindowsImage(e, _applyBackgroundWorker);

        private void ApplyBackgroundWorker_ProgressChanged(object? sender, ProgressChangedEventArgs e)
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

                    DeployImage.Configuration.IsCanceled = true;
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
    }
}
