using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;
using Wpf.Ui.Common;
using Wpf.Ui.Controls.Navigation;
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
            NavView.Navigate("application");
        }

        private void UnattendConfiguration_OnContentRendered(object? sender, EventArgs e)
        {
            NavView.Navigate("deployment"); // Workaround
        }
    }
}
