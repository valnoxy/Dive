using deployaUI.Pages.ApplyPages;
using System.Windows;
using System.Windows.Controls;

namespace deployaUI.Pages
{
    /// <summary>
    /// Interaktionslogik für CloudContent.xaml
    /// </summary>
    public partial class CloudContent : UserControl
    {
        public static CloudContent? ContentWindow;

        CloudSelectStep CSS = new CloudSelectStep();
        DeploymentSettingsStep deploymentSettingsStep = new DeploymentSettingsStep();

        public CloudContent()
        {
            InitializeComponent();

            NextBtn.IsEnabled = false;
            BackBtn.IsEnabled = false;
            FrameWindow.Content = CSS;
            ContentWindow = this;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            switch (FrameWindow.Content)
            {
                case CloudSelectStep:
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
                    else
                        System.Diagnostics.Process.Start("shutdown.exe", "-r -t 0");
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
                    FrameWindow.Content = CSS;
                    NextBtn.IsEnabled = true;
                    BackBtn.IsEnabled = false;
                    break;
                case DiskSelectStep:
                    FrameWindow.Content = deploymentSettingsStep;
                    NextBtn.IsEnabled = true;
                    BackBtn.IsEnabled = false;
                    break;
                default:
                    break;
            }
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            FrameWindow.Content = CSS;
            NextBtn.IsEnabled = false;
            NextBtn.Visibility = Visibility.Visible;
        }
    }
}
