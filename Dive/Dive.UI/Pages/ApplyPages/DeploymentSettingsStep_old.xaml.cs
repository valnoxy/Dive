using Dive.UI.Common;
using Dive.UI.Pages.Extras;
using Microsoft.Win32;
using Path = System.IO.Path;
using System;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

namespace Dive.UI.Pages.ApplyPages
{
    /// <summary>
    /// Interaktionslogik für DeploymentSettingsStep_old.xaml
    /// </summary>
    public partial class DeploymentSettingsStepOld : UserControl
    {
        public DeploymentSettingsStepOld()
        {
            InitializeComponent();
            //DeploymentInfo.UseUserInfo = false;
            OemInfo.UseOemInfo = false;
        }

        private void MoreSettings_OnClicked(object sender, RoutedEventArgs e)
        {
            var ucWindow = new UnattendConfiguration();
            ucWindow.ShowDialog();
        }
    }
}
