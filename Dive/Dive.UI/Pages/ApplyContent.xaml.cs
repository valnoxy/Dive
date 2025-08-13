using Dive.UI.Common;
using Dive.UI.Pages.ApplyPages;
using System;
using System.Windows;
using System.Windows.Controls;
using Dive.UI.Common.UserInterface;

namespace Dive.UI.Pages
{
    /// <summary>
    /// Interaktionslogik für ApplyContent.xaml
    /// </summary>
    public partial class ApplyContent : UserControl
    {
        public static ApplyContent? ContentWindow;
        private static readonly ApplyDetails ApplyDetailsInstance = ApplyDetails.Instance;

        SKUSelectStep SKUSS = new();
        DeploymentSettingsStep deploymentSettingsStep = new();
        
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
                    var diskStep = new DiskSelectStep();
                    FrameWindow.Content = diskStep;
                    break;
                case DiskSelectStep:
                    if (ApplyDetailsInstance is { Build: < 7849, UseEFI: true }) // Windows 8 Build 7849 and lower 
                    {
                        Debug.WriteLine("Detected legacy Windows with EFI!");

                        var message = "You are trying to install a legacy Windows version with EFI bootloader. The Windows version you have selected does not support UEFI, but a patch can be applied. Should all available patches be shown?";
                        var title = "EFI Patch for legacy Windows Versions";
                        var btn1 = "No";
                        var btn2 = "Yes";

                        var w = new MessageUI(title, message, btn1, btn2, true);
                        if (w.ShowDialog() == false)
                        {
                            var summary = w.Summary;
                            if (summary == "Btn2")
                            {
                                var efiSettings = new Extras.LegacyEFI();
                                efiSettings.ShowDialog();
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
