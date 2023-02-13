using deployaUI.Pages.ApplyPages;
using System.Windows;
using System.Windows.Controls;

namespace deployaUI.Pages
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
                    ApplySelectStep ApplySS = new ApplySelectStep();
                    FrameWindow.Content = ApplySS;
                    break;
                case ApplySelectStep:
                    if (System.IO.File.Exists("X:\\Windows\\System32\\wpeutil.exe"))
                        System.Diagnostics.Process.Start("wpeutil.exe", "reboot");
                    //else
                    //    System.Diagnostics.Process.Start("shutdown.exe", "-r -t 0");
                    break;
                default:
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
