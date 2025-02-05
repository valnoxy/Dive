using System;
using System.ComponentModel;
using System.Windows;
using Dive.UI.Common;
using Dive.UI.Common.UserInterface;

namespace Dive.UI.Pages.Extras
{
    /// <summary>
    /// Interaktionslogik für UnattendConfiguration.xaml
    /// </summary>
    public partial class UnattendConfiguration
    {
        private static readonly DeviceInfo DeviceInfoInstance = DeviceInfo.Instance;

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
            if (!string.IsNullOrEmpty(DeviceInfoInstance.RegisteredOwner) ||
                !string.IsNullOrEmpty(DeviceInfoInstance.RegisteredOrganization) ||
                !string.IsNullOrEmpty(DeviceInfoInstance.ProductKey) || 
                !string.IsNullOrEmpty(DeviceInfoInstance.DeviceName))
            {
                DeviceInfoInstance.UseDeviceInfo = true;
                Debug.Write("Use DeviceInfo in Unattend: ");
                Debug.Write("Enabled\n", true, ConsoleColor.DarkYellow);
            }
            else
            {
                DeviceInfoInstance.UseDeviceInfo = false;
                Debug.Write("Use DeviceInfo in Unattend: ");
                Debug.Write("Disabled\n", true, ConsoleColor.DarkYellow);
            }
        }
    }
}
