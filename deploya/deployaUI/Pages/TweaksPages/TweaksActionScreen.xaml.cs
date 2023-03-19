using deployaCore;
using deployaCore.Common;
using deployaUI.Common;
using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Media;
using System.Diagnostics;
using System.Management;

namespace deployaUI.Pages.TweaksPages
{
    /// <summary>
    /// Interaktionslogik für TweaksActionScreen.xaml
    /// </summary>
    public partial class TweaksActionScreen : System.Windows.Controls.UserControl
    {
        private BackgroundWorker applyBackgroundWorker = new();
        bool IsCanceled = false;

        public TweaksActionScreen()
        {
            InitializeComponent();

            if (TweaksContent.ContentWindow != null)
            {
                TweaksContent.ContentWindow.NextBtn.IsEnabled = false;
                TweaksContent.ContentWindow.BackBtn.IsEnabled = false;
                TweaksContent.ContentWindow.CancelBtn.IsEnabled = false;
            }

            // Set active Image to card
            ImageName.Text = $"Disk {Common.Tweaks.DiskIndex}";
            
            // ImageFile.Text = Common.ApplyDetails.FileName;
            var img = new ImageSourceConverter();
            try
            {
                ImageIcon.Source = (ImageSource)img.ConvertFromString("null");
            }
            catch
            {
                // ignored
            }

            // Background worker for deployment
            applyBackgroundWorker.WorkerReportsProgress = true;
            applyBackgroundWorker.WorkerSupportsCancellation = true;
            applyBackgroundWorker.DoWork += MigrateWindows;
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
                    //ProgrText.Text = $"Injecting drivers ({currentDriver} of {driverCount}) ...";
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

        private static void MigrateWindows(object? sender, DoWorkEventArgs e)
        {
            #region Environment definition

            var worker = sender as BackgroundWorker;

            #endregion

            // Validate Windows Disk
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskPartition WHERE DiskIndex = " + Common.Tweaks.DiskIndex);

            foreach (ManagementObject partition in searcher.Get())
            {
                Console.WriteLine("Type: " + partition["Type"]);
                Console.WriteLine("Size: " + partition["Size"] + " bytes");
                Console.WriteLine("Name: " + partition["Name"]);
                Console.WriteLine("DeviceID: " + partition["DeviceID"]);
            }

            // Identify installed Windows Version
            const string ntKernel = "E:\\Windows\\System32\\ntoskrnl.exe";
            var kernelVersionInfo = FileVersionInfo.GetVersionInfo(ntKernel);
            var windowsVersion = GetWindowsName(kernelVersionInfo.ToString());

            if (windowsVersion is not WindowsVersion.Windows10 or WindowsVersion.Windows81 or WindowsVersion.Windows8)
            {
                // Failed -> System does not support UEFI
            }
        }

        private enum WindowsVersion
        {
            Windows2000,
            WindowsXP,
            WindowsVista,
            Windows7,
            Windows8,
            Windows81,
            Windows10,
            Unknown
        }

        private static WindowsVersion GetWindowsName(string productVersion)
        {
            var versionParts = productVersion.Split('.');

            if (versionParts[0] == "10")
            {
                return WindowsVersion.Windows10;
            }
            else
            {
                switch (versionParts[0])
                {
                    case "6":
                        switch (versionParts[1])
                        {
                            case "0":
                                return WindowsVersion.WindowsVista;
                            case "1":
                                return WindowsVersion.Windows7;
                            case "2":
                                return WindowsVersion.Windows8;
                            case "3":
                                return WindowsVersion.Windows81;
                        }
                        break;
                    case "5":
                        switch (versionParts[1])
                        {
                            case "0":
                                return WindowsVersion.Windows2000;
                            case "1":
                            case "2":
                                return WindowsVersion.WindowsXP;
                        }
                        break;
                    default:
                        return WindowsVersion.Unknown;
                }
            }
            return WindowsVersion.Unknown;
        }
    }
}
