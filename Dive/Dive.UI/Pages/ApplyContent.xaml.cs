using Dive.UI.Pages.ApplyPages;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Dive.UI.Pages
{
    /// <summary>
    /// Interaktionslogik für ApplyContent.xaml
    /// </summary>
    public partial class ApplyContent : UserControl
    {
        public static ApplyContent? ContentWindow;

        SKUSelectStep SKUSS = new SKUSelectStep();
        DeploymentSettingsStep deploymentSettingsStep = new DeploymentSettingsStep();
        
        public ApplyContent()
        {
            InitializeComponent();

            NextBtn.IsEnabled = false;
            BackBtn.IsEnabled = false;
            FrameWindow.Content = SKUSS;
            ContentWindow = this;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            switch (FrameWindow.Content)
            {
                case SKUSelectStep:                    
                    FrameWindow.Content = deploymentSettingsStep;
                    BackBtn.IsEnabled = true;
                    break;
                case DeploymentSettingsStep:
                    DiskSelectStep DiskSS = new DiskSelectStep();
                    FrameWindow.Content = DiskSS;
                    break;
                case DiskSelectStep:
                    if ((Common.ApplyDetails.Name.ToLower().Contains("windows 7") || Common.ApplyDetails.Name.ToLower().Contains("vista")) && Common.ApplyDetails.UseEFI)
                    {
                        Common.Debug.WriteLine("Detected Windows Vista / 7 with EFI - Showing UefiSeven Installation prompt ...");

                        var message = "It looks like you're attempting to install Windows Vista/7 with EFI support. Normally Vista/7 does not natively support EFI, however an EFI module called UefiSeven can be installed to allow Vista/7 to boot on EFI machines.\n\nDo you want to install UefiSeven?";
                        var title = "EFI Patch for Windows Vista / 7";
                        var btn1 = "No";
                        var btn2 = "Yes";

                        var w = new MessageUI(title, message, btn1, btn2, true);
                        if (w.ShowDialog() == false)
                        {
                            var summary = w.Summary;
                            if (summary == "Btn2")
                            {
                                Common.Debug.WriteLine("Using UefiSeven for EFI boot loader");
                                Common.WindowsModification.InstallUefiSeven = true;
                                var uefiSevenSettings = new Extras.UefiSevenSettings();
                                uefiSevenSettings.ShowDialog();
                            }
                        }
                    }
                    ApplySelectStep ApplySS = new ApplySelectStep();
                    FrameWindow.Content = ApplySS;
                    break;
                case ApplySelectStep:
                    if (System.IO.File.Exists("X:\\Windows\\System32\\wpeutil.exe"))
                        System.Diagnostics.Process.Start("wpeutil.exe", "reboot");
                    else
                        Environment.Exit(0);
                    break;
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            switch (FrameWindow.Content)
            {
                case DeploymentSettingsStep:
                    FrameWindow.Content = SKUSS;
                    NextBtn.IsEnabled = true;
                    BackBtn.IsEnabled = false;
                    break;
                case DiskSelectStep:
                    FrameWindow.Content = deploymentSettingsStep;
                    NextBtn.IsEnabled = true;
                    BackBtn.IsEnabled = true;
                    break;
                default:
                    break;
            }
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            FrameWindow.Content = SKUSS;
            NextBtn.IsEnabled = false;
            NextBtn.Visibility = Visibility.Visible;
        }
    }
}
