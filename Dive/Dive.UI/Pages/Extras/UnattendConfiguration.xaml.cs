using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using Dive.UI.Common;
using Wpf.Ui.Controls;

namespace Dive.UI.Pages.Extras
{
    /// <summary>
    /// Interaktionslogik für UnattendConfiguration.xaml
    /// </summary>
    public partial class UnattendConfiguration
    {
        public UnattendConfiguration()
        {
            InitializeComponent();
        }

        private void UnattendConfiguration_OnLoaded(object sender, RoutedEventArgs e)
        {
            NavView.Navigate(typeof(UnattendConfigurationPages.DeploymentConfigurationPage));
        }

        private void UnattendConfiguration_OnClosing(object? sender, CancelEventArgs e)
        {
            // Define state of DeviceInfo
            if (!string.IsNullOrEmpty(DeviceInfo.RegisteredOwner) ||
                !string.IsNullOrEmpty(DeviceInfo.RegisteredOrganization) ||
                !string.IsNullOrEmpty(DeviceInfo.ProductKey) || 
                !string.IsNullOrEmpty(DeviceInfo.DeviceName))
            {
                DeviceInfo.UseDeviceInfo = true;
                Debug.Write("Use DeviceInfo in Unattend: ");
                Debug.Write("Enabled\n", true, ConsoleColor.DarkYellow);
            }
            else
            {
                DeviceInfo.UseDeviceInfo = false;
                Debug.Write("Use DeviceInfo in Unattend: ");
                Debug.Write("Disabled\n", true, ConsoleColor.DarkYellow);
            }
        }
    }
}
