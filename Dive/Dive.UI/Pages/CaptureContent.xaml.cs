using System;
using Dive.UI.Pages.ApplyPages;
using Dive.UI.Pages.CapturePages;
using System.Windows;
using System.Windows.Controls;

namespace Dive.UI.Pages
{
    /// <summary>
    /// Interaktionslogik für CaptureContent.xaml
    /// </summary>
    public partial class CaptureContent : System.Windows.Controls.UserControl
    {
        public static CaptureContent? ContentWindow;

        SettingsStep settingsStep = new SettingsStep();
        
        public CaptureContent()
        {
            InitializeComponent();

            NextBtn.IsEnabled = false;
            BackBtn.IsEnabled = false;
            FrameWindow.Content = settingsStep;
            ContentWindow = this;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            switch (FrameWindow.Content)
            {
                case SettingsStep:
                    FrameWindow.Content = new SaveToStep();
                    BackBtn.IsEnabled = true;
                    NextBtn.IsEnabled = false;
                    break;
                case SaveToStep:
                    FrameWindow.Content = new CaptureStep();
                    break;
                case CaptureStep:
                    Environment.Exit(0);
                    break;
                default:
                    break;
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            switch (FrameWindow.Content)
            {
                case SaveToStep:
                    FrameWindow.Content = settingsStep;
                    BackBtn.IsEnabled = false;
                    NextBtn.IsEnabled = true;
                    break;
                default:
                    break;
            }
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            FrameWindow.Content = settingsStep;
            NextBtn.IsEnabled = false;
            NextBtn.Visibility = Visibility.Visible;
        }
    }
}
