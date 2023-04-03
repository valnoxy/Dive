using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;
using Wpf.Ui.Common;
using Wpf.Ui.Controls.Navigation;
using Wpf.Ui.Controls;

namespace deployaUI.Pages.Extras
{
    /// <summary>
    /// Interaktionslogik für UnattendConfiguration.xaml
    /// </summary>
    public partial class UnattendConfiguration
    {
        public UnattendConfiguration()
        {
            InitializeComponent();

            var navItems = new List<INavigationViewItem>
            {
                new NavigationViewItem
                {
                    Content = Common.LocalizationManager.LocalizeValue("Deployment"),
                    Icon = new SymbolIcon
                    {
                        Symbol = SymbolRegular.Home24
                    },
                    TargetPageTag = "deployment",
                    TargetPageType = new TypeDelegator(typeof(Extras.UnattendConfigurationPages.DeploymentConfigurationPage)),

                },
                new NavigationViewItem
                {
                    Content = Common.LocalizationManager.LocalizeValue("Out-of-Box-Experience"),
                    Icon = new SymbolIcon
                    {
                        Symbol = SymbolRegular.WindowApps24
                    },
                    TargetPageTag = "oobe"
                },
                new NavigationViewItem
                {
                    Content = Common.LocalizationManager.LocalizeValue("Application"),
                    Icon = new SymbolIcon
                    {
                        Symbol = SymbolRegular.AppGeneric32
                    },
                    TargetPageTag = "application"
                }
            };

            NavView.MenuItems = navItems;
            
            NavView.IsBackButtonVisible = NavigationViewBackButtonVisible.Collapsed;
            NavView.Navigate("about");
        }
    }
}
