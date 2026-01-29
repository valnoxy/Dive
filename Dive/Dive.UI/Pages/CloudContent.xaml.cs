using Dive.UI.Common;
using Dive.UI.Common.UserInterface;
using Dive.UI.Pages.ApplyPages;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Dive.UI.Pages
{
    /// <summary>
    /// Interaktionslogik für CloudContent.xaml
    /// </summary>
    public partial class CloudContent : UserControl
    {
        public static CloudContent? ContentWindow;

        private readonly CloudSelectStep _css = new();
        private readonly DeploymentSettingsStep _deploymentSettingsStep = new();
        private static readonly ApplyDetails ApplyDetailsInstance = ApplyDetails.Instance;

        public CloudContent()
        {
            InitializeComponent();

            NextBtn.IsEnabled = false;
            BackBtn.IsEnabled = false;
            FrameWindow.Content = _css;
            ContentWindow = this;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            switch (FrameWindow.Content)
            {
                case CloudSelectStep:
                    FrameWindow.Content = _deploymentSettingsStep;
                    BackBtn.IsEnabled = true;
                    break;
                case DeploymentSettingsStep:
                    var diskStep = new DiskSelectStep();
                    FrameWindow.Content = diskStep;
                    break;
                case DiskSelectStep:
                    if ((ApplyDetailsInstance.Name.ToLower().Contains("windows 7") || ApplyDetailsInstance.Name.ToLower().Contains("vista")) && ApplyDetailsInstance.UseEFI)
                    {
                        Debug.WriteLine("Detected Windows Vista / 7 with EFI - Showing UefiSeven Installation prompt ...");

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
                                Debug.WriteLine("Using UefiSeven for EFI boot loader");
                                Common.WindowsModification.InstallUefiSeven = true;
                                var uefiSevenSettings = new Extras.UefiSevenSettings();
                                uefiSevenSettings.ShowDialog();
                            }
                        }
                    }

                    //var applyPage = new ApplySelectStep(); // Old
                    var applyPage = new ApplyImageStep(); // New
                    FrameWindow.Content = applyPage;
                    break;
                case ApplySelectStep:
                case ApplyImageStep:
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
                    FrameWindow.Content = _css;
                    NextBtn.IsEnabled = true;
                    BackBtn.IsEnabled = false;
                    break;
                case DiskSelectStep:
                    FrameWindow.Content = _deploymentSettingsStep;
                    NextBtn.IsEnabled = true;
                    BackBtn.IsEnabled = true;
                    break;
                default:
                    break;
            }
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            FrameWindow.Content = _css;
            NextBtn.IsEnabled = false;
            NextBtn.Visibility = Visibility.Visible;
        }
    }
}
