using System;
using deployaUI.Pages.ApplyPages;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using deployaUI.Common;
using deployaUI.Pages.TweaksPages;

namespace deployaUI.Pages
{
    /// <summary>
    /// Interaktionslogik für TweaksContent.xaml
    /// </summary>
    public partial class TweaksContent : UserControl
    {
        public static TweaksContent? ContentWindow;

        TweaksDashboard TD = new TweaksDashboard();
        //DeploymentSettingsStep deploymentSettingsStep = new DeploymentSettingsStep();

        public TweaksContent()
        {
            InitializeComponent();

            NextBtn.IsEnabled = false;
            BackBtn.IsEnabled = false;
            FrameWindow.Content = TD;
            ContentWindow = this;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            switch (Common.Tweaks.CurrentMode)
            {
                case TweakMode.Migrate:
                    switch (FrameWindow.Content)
                    {
                        case MigrateSettings:
                            FrameWindow.Content = TD;
                            NextBtn.IsEnabled = false;
                            BackBtn.IsEnabled = false;
                            break;
                        default:
                            break;
                    }
                    break;
                case TweakMode.RepairBootloader:
                    // WIP
                    switch (FrameWindow.Content)
                    {
                        case MigrateSettings:
                            FrameWindow.Content = TD;
                            NextBtn.IsEnabled = false;
                            BackBtn.IsEnabled = false;
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            switch (Common.Tweaks.CurrentMode)
            {
                case TweakMode.Migrate:
                    switch (FrameWindow.Content)
                    {
                        case MigrateSettings:
                            FrameWindow.Content = TD;
                            NextBtn.IsEnabled = false;
                            BackBtn.IsEnabled = false;
                            break;
                        default:
                            break;
                    }
                    break;
                case TweakMode.RepairBootloader:
                    // WIP
                    switch (FrameWindow.Content)
                    {
                        case MigrateSettings:
                            FrameWindow.Content = TD;
                            NextBtn.IsEnabled = false;
                            BackBtn.IsEnabled = false;
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            FrameWindow.Content = TD;
            NextBtn.IsEnabled = false;
            NextBtn.Visibility = Visibility.Visible;
        }
    }
}
