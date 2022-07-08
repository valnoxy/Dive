using deploya_core;
using System;
using System.ComponentModel;
using System.Threading;
using System.Windows.Controls;

namespace deploya.Pages.ApplyPages
{
    /// <summary>
    /// Interaktionslogik für ApplySelectStep.xaml
    /// </summary>
    public partial class ApplySelectStep : UserControl
    {
        private BackgroundWorker applyBackgroundWorker;

        public ApplySelectStep()
        {
            InitializeComponent();

            ApplyContent.ContentWindow.NextBtn.IsEnabled = false;
            ApplyContent.ContentWindow.BackBtn.IsEnabled = false;
            ApplyContent.ContentWindow.CancelBtn.IsEnabled = false;

            if (DiskSelectStep.ContentWindow.IsNTLDRChecked())
                Common.ApplyDetails.UseNTLDR = true;
            else 
                Common.ApplyDetails.UseNTLDR = false;

            applyBackgroundWorker = new BackgroundWorker();
            applyBackgroundWorker.WorkerReportsProgress = true;
            applyBackgroundWorker.WorkerSupportsCancellation = true;
            applyBackgroundWorker.DoWork += ApplyWim;
            applyBackgroundWorker.RunWorkerCompleted += applyBackgroundWorker_RunWorkerCompleted;
            applyBackgroundWorker.ProgressChanged += applyBackgroundWorker_ProgressChanged;
            applyBackgroundWorker.RunWorkerAsync();
        }

        private void applyBackgroundWorker_ProgressChanged(object? sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage == 101)        // 101: Progressbar is not Indeterminate
                ProgrBar.IsIndeterminate = false;
            if (e.ProgressPercentage == 102)        // 102: Progressbar is Indeterminate
                ProgrBar.IsIndeterminate = true;
            if (e.ProgressPercentage == 201)        // 201: ProgText -> Prepare Disk
                ProgrText.Text = "Preparing disk ...";
            if (e.ProgressPercentage == 202)        // 202: ProgText -> Applying WIM
                ProgrText.Text = $"Applying Image to disk ({ProgrBar.Value}%) ...";
            if (e.ProgressPercentage == 203)        // 203: ProgText -> Installing Bootloader
                ProgrText.Text = "Installing bootloader to disk ...";
            if (e.ProgressPercentage == 304)        // 304: Installation complete          
            {
                ProgrText.Text = "Installation completed. Press 'Next' to restart your computer.";
                ProgrBar.Value = 100;
                ApplyContent.ContentWindow.NextBtn.IsEnabled = true;
            }
            if (e.ProgressPercentage < 100)
                this.ProgrBar.Value = e.ProgressPercentage;
        }

        private void applyBackgroundWorker_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
        {
            Console.WriteLine("OK");
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

            // Apply image
            worker.ReportProgress(202, "");     // Applying Image Text
            Actions.ApplyWIM(ui, "W:\\", Common.ApplyDetails.FileName, Common.ApplyDetails.Index, worker);

            // Install Bootloader
            worker.ReportProgress(203, "");     // Installing Bootloader Text
            if (bootloader == Entities.Bootloader.BOOTMGR)
                Actions.InstallBootloader(firmware, bootloader, ui, "W:\\Windows", "S:\\", worker);

            if (bootloader == Entities.Bootloader.NTLDR)
                Actions.InstallBootloader(firmware, bootloader, ui, "W:\\", "W:\\", worker);
        }
    }
}
