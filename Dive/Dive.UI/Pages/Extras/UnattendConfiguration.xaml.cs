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

        private void NavView_OnLoaded(object sender, RoutedEventArgs e)
        {
            var navItems = new List<INavigationViewItem>
            {
                new NavigationViewItem
                {
                    Content = Common.LocalizationManager.LocalizeValue("Deployment"),
                    Icon = new SymbolIcon
                    {
                        Symbol = SymbolRegular.HardDrive20
                    },
                    TargetPageTag = "deployment",
                    TargetPageType = new TypeDelegator(typeof(Extras.UnattendConfigurationPages.DeploymentConfigurationPage))
                },
                new NavigationViewItem
                {
                    Content = Common.LocalizationManager.LocalizeValue("OOBE"),
                    Icon = new SymbolIcon
                    {
                        Symbol = SymbolRegular.CheckboxPerson20
                    },
                    TargetPageTag = "oobe",
                    TargetPageType = new TypeDelegator(typeof(Extras.UnattendConfigurationPages.OOBEConfigurationPage))
                },
                new NavigationViewItem
                {
                    Content = Common.LocalizationManager.LocalizeValue("Application"),
                    Icon = new SymbolIcon
                    {
                        Symbol = SymbolRegular.AppGeneric20
                    },
                    TargetPageTag = "application",
                    TargetPageType = new TypeDelegator(typeof(Extras.UnattendConfigurationPages.ApplicationConfigurationPage))
                }
            };

            NavView.MenuItems = navItems;
            NavView.PaneDisplayMode = NavigationViewPaneDisplayMode.Top;
            NavView.IsBackButtonVisible = NavigationViewBackButtonVisible.Collapsed;
            NavView.Navigate(typeof(Extras.UnattendConfigurationPages.DeploymentConfigurationPage));
        }

        private void UnattendConfiguration_OnContentRendered(object? sender, EventArgs e)
        {
            NavView.Navigate(typeof(Extras.UnattendConfigurationPages.DeploymentConfigurationPage)); // Workaround
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
