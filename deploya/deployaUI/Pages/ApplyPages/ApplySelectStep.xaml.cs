using deploya_core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Xml;

namespace deploya.Pages.ApplyPages
{
    /// <summary>
    /// Interaktionslogik für ApplySelectStep.xaml
    /// </summary>
    public partial class ApplySelectStep : UserControl
    {

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

            Task.Factory.StartNew(() => ApplyWim());
        }

        private void ApplyWim()
        {
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

            ProgrBar.IsIndeterminate = true;
            ProgrBar.Value = 0;

            // Prepare disk
            Dispatcher.BeginInvoke(new Action(() =>
            {
                ProgrText.Text = "Partitioning disk ...";
                Actions.PrepareDisk(firmware, bootloader, ui, Common.ApplyDetails.DiskIndex, ProgrBar);
            }), DispatcherPriority.Background);


            // Apply image
            Dispatcher.BeginInvoke(new Action(() =>
            {
                ProgrText.Text = "Applying image to disk ...";
                Actions.ApplyWIM(ui, "W:\\", Common.ApplyDetails.FileName, Common.ApplyDetails.Index, ProgrBar);
            }), DispatcherPriority.Background);

            // Install Bootloader
            Dispatcher.BeginInvoke(new Action(() =>
            {
                ProgrText.Text = "Installing Bootloader to disk ...";
                if (bootloader == Entities.Bootloader.BOOTMGR)
                    Actions.InstallBootloader(firmware, bootloader, ui, "W:\\", "S:\\", ProgrBar);

                if (bootloader == Entities.Bootloader.NTLDR)
                    Actions.InstallBootloader(firmware, bootloader, ui, "W:\\", "W:\\", ProgrBar);
            }), DispatcherPriority.Background);
        }
    }
}
