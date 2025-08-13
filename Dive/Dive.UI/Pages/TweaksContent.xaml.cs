using System;
using System.Windows;
using System.Windows.Controls;
using Dive.UI.Common;
using Dive.UI.Pages.TweaksPages;
using Dive.UI.Pages.TweaksPages.PlayBook;
using Dive.UI.Pages.TweaksPages.USMT;

namespace Dive.UI.Pages
{
    /// <summary>
    /// Interaktionslogik für TweaksContent.xaml
    /// </summary>
    public partial class TweaksContent : UserControl
    {
        public static TweaksContent? ContentWindow;

        readonly TweaksDashboard _td = new();
        readonly PlayBookLoad _pb = new();
        //DeploymentSettingsStep deploymentSettingsStep = new DeploymentSettingsStep();
        private static readonly Tweaks TweaksInstance = Tweaks.Instance;

        public TweaksContent()
        {
            InitializeComponent();

            NextBtn.IsEnabled = false;
            BackBtn.IsEnabled = false;
            FrameWindow.Content = _td;
            ContentWindow = this;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            switch (TweaksInstance.CurrentMode)
            {
                case TweakMode.AutoInit:
                    switch (FrameWindow.Content)
                    {
                        case PlayBookLoad:
                            //_pb.DragEnter += UserControlDragEnter;
                            //_pb.Drop += _pb_Drop;
                            FrameWindow.Content = _pb;
                            NextBtn.IsEnabled = false;
                            BackBtn.IsEnabled = false;
                            break;
                        default:
                            break;
                    }
                    break;
                case TweakMode.Migrate:
                    BackBtn.IsEnabled = true;

                    switch (FrameWindow.Content)
                    {
                        case MigrateSettings:
                            FrameWindow.Content = _td;
                            NextBtn.IsEnabled = false;
                            BackBtn.IsEnabled = true;
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
                            FrameWindow.Content = _td;
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
            switch (TweaksInstance.CurrentMode)
            {
                case TweakMode.AutoInit:
                    switch (FrameWindow.Content)
                    {
                        case PlayBookLoad:
                            FrameWindow.Content = _td;
                            NextBtn.IsEnabled = false;
                            BackBtn.IsEnabled = false;
                            break;
                        default:
                            break;
                    }
                    break;
                case TweakMode.Migrate:
                    switch (FrameWindow.Content)
                    {
                        case MigrateSettings:
                            FrameWindow.Content = _td;
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
                            FrameWindow.Content = _td;
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
            FrameWindow.Content = _td;
            NextBtn.IsEnabled = false;
            NextBtn.Visibility = Visibility.Visible;
        }
    }
}
