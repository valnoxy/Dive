using deploya_core;
using System;
using System.ComponentModel;
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

            // Set active Image to card
            ImageName.Text = Common.ApplyDetails.Name;
            ImageFile.Text = Common.ApplyDetails.FileName;
            ImageSourceConverter img = new ImageSourceConverter();
            ImageIcon.Source = (ImageSource)img.ConvertFromString(Common.ApplyDetails.IconPath);
            
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
            //   202: ProgText -> Applying WIM
            //   203: ProgText -> Installing Bootloader
            //   204: ProgText -> Installing recovery
            //   205: Installation complete
            //
            // Error message handling
            //   301: Failed at preparing disk
            //   302: Failed at applying WIM
            //   303: Failed at installing bootloader
            //   304: Failed at installing recovery
            //
            // Range 0-100 -> Progressbar percentage
            //

            // Progress bar handling
            if (e.ProgressPercentage == 101)        // 101: Progressbar is not Indeterminate
                ProgrBar.IsIndeterminate = false;
            if (e.ProgressPercentage == 102)        // 102: Progressbar is Indeterminate
                ProgrBar.IsIndeterminate = true;

            // Standard Message handling
            if (e.ProgressPercentage == 201)        // 201: ProgText -> Prepare Disk
                ProgrText.Text = "Preparing disk ...";
            if (e.ProgressPercentage == 202)        // 202: ProgText -> Applying WIM
                ProgrText.Text = $"Applying Image to disk ({ProgrBar.Value}%) ...";
            if (e.ProgressPercentage == 203)        // 203: ProgText -> Installing Bootloader
                ProgrText.Text = "Installing bootloader to disk ...";
            if (e.ProgressPercentage == 204)        // 204: ProgText -> Installing recovery
                ProgrText.Text = "Registering recovery partition to Windows ...";
            if (e.ProgressPercentage == 205)        // 205: Installation complete          
            {
                ProgrText.Text = "Installation completed. Press 'Next' to restart your computer.";
                ProgrBar.Value = 100;
                if (ApplyContent.ContentWindow != null)
                    ApplyContent.ContentWindow.NextBtn.IsEnabled = true;
                if (CloudContent.ContentWindow != null)
                    CloudContent.ContentWindow.NextBtn.IsEnabled = true;
            }

            // Error message handling
            if (e.ProgressPercentage == 301)        // 301: Failed at preparing disk
            {
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
            }
            if (e.ProgressPercentage == 302)        // 302: Failed at applying WIM
            {
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
            }
            if (e.ProgressPercentage == 303)        // 303: Failed at installing bootloader
            {
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
            }
            if (e.ProgressPercentage == 304)        // 303: Failed at installing recovery
            {
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
            }

            // Progressbar percentage
            if (e.ProgressPercentage <= 100)
                this.ProgrBar.Value = e.ProgressPercentage;
        }

        private void ApplyWim(object? sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            Entities.Firmware firmware = new Entities.Firmware();
            Entities.Bootloader bootloader = new Entities.Bootloader();
            Entities.UI ui = new Entities.UI();

            // UI definition
            ui = Entities.UI.Graphical;

            // Firmware definition
            if (Common.ApplyDetails.UseEFI) { firmware = Entities.Firmware.EFI; }
            if (!Common.ApplyDetails.UseEFI) { firmware = Entities.Firmware.BIOS; }

            // Bootloader definition
            if (Common.ApplyDetails.UseNTLDR) { bootloader = Entities.Bootloader.NTLDR; }
            if (!Common.ApplyDetails.UseNTLDR) { bootloader = Entities.Bootloader.BOOTMGR; }

            worker.ReportProgress(0, "");       // Value 0

            // Prepare disk
            worker.ReportProgress(201, "");     // Prepare Disk Text
            Actions.PrepareDisk(firmware, bootloader, ui, Common.ApplyDetails.DiskIndex, worker);
            if (IsCanceled)
            {
                e.Cancel = true;
                return;
            }

            // Apply image
            worker.ReportProgress(202, "");     // Applying Image Text
            worker.ReportProgress(0, "");       // Value 0
            Actions.ApplyWIM(ui, "W:\\", Common.ApplyDetails.FileName, Common.ApplyDetails.Index, worker);
            if (IsCanceled)
            {
                e.Cancel = true;
                return;
            }
            
            // Install Bootloader
            worker.ReportProgress(203, "");     // Installing Bootloader Text
            if (bootloader == Entities.Bootloader.BOOTMGR)
                Actions.InstallBootloader(firmware, bootloader, ui, "W:\\Windows", "S:\\", worker);

            if (bootloader == Entities.Bootloader.NTLDR)
                Actions.InstallBootloader(firmware, bootloader, ui, "W:\\", "W:\\", worker);

            if (IsCanceled)
            {
                e.Cancel = true;
                return;
            }

            // Install Recovery (only for Vista and higher)
            if (bootloader == Entities.Bootloader.BOOTMGR)
            {
                worker.ReportProgress(204, "");     // Installing Bootloader Text
                Actions.InstallRecovery(ui, "W:\\Windows", "R:\\", worker);
            
                if (IsCanceled)
                {
                    e.Cancel = true;
                    return;
                }
            }

            // Installation complete
            worker.ReportProgress(205, "");     // Installation complete Text
        }
    }
}
