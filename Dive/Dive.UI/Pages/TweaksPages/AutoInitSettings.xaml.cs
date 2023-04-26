using System;
using System.Linq;
using System.Management;
using System.Text.RegularExpressions;
using System.Windows;
using Wpf.Ui.Controls;

namespace Dive.UI.Pages.TweaksPages
{
    /// <summary>
    /// Interaktionslogik für AutoInitSettings.xaml
    /// </summary>
    public partial class AutoInitSettings : System.Windows.Controls.UserControl
    {
        public AutoInitSettings()
        {
            InitializeComponent();

            if (TweaksContent.ContentWindow == null) return;
            TweaksContent.ContentWindow.NextBtn.IsEnabled = false;
            TweaksContent.ContentWindow.BackBtn.IsEnabled = true;
        }

        private void ShowFlyout(object sender, RoutedEventArgs e)
        {
            UiFlyout.IsOpen = true;
        }
    }
}
