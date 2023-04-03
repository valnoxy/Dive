using System;
using deployaUI.Common;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using deployaUI.Pages;
using Wpf.Ui.Appearance;
using Wpf.Ui.Common;
using Wpf.Ui.Controls;
using Wpf.Ui.Controls.Navigation;

namespace deployaUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private bool _displayDebugConsole;

        public MainWindow()
        {
            InitializeComponent();
            
#if DEBUG
            DebugString.Visibility = Visibility.Visible;
            DebugString.Text = "Debug build - This is not a production ready build.";
            _displayDebugConsole = true;
#else
            DebugString.Visibility = Visibility.Visible;
            DebugString.Text = "Beta build [B6 Public]";
#endif
            // Get version
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            var version = fvi.FileVersion;
            VersionString.Text = $"[{version}]";
        }

        private void RootNavigation_OnLoaded(object sender, RoutedEventArgs e)
        {
            // Build navigation menu
            var navItems = new List<INavigationViewItem>();
            var footerNavItems = new List<INavigationViewItem>();

            navItems.Add(new NavigationViewItem
            {
                Content = Common.LocalizationManager.LocalizeValue("Home"),
                Icon = new SymbolIcon
                {
                    Symbol = SymbolRegular.Home24
                },
                TargetPageTag = "dashboard",
                TargetPageType = new TypeDelegator(typeof(Pages.Dashboard)),

            });

            navItems.Add(new NavigationViewItem
            {
                Content = Common.LocalizationManager.LocalizeValue("Apply"),
                Icon = new SymbolIcon
                {
                    Symbol = SymbolRegular.WindowApps24
                },
                TargetPageTag = "apply",
                TargetPageType = new TypeDelegator(typeof(Pages.ApplyContent))
            });

            navItems.Add(new NavigationViewItem
            {
                Content = Common.LocalizationManager.LocalizeValue("Capture"),
                Icon = new SymbolIcon
                {
                    Symbol = SymbolRegular.Copy24
                },
                TargetPageTag = "capture",
                TargetPageType = new TypeDelegator(typeof(Pages.CaptureContent))
            });

            navItems.Add(new NavigationViewItem
            {
                Content = Common.LocalizationManager.LocalizeValue("Cloud"),
                Icon = new SymbolIcon
                {
                    Symbol = SymbolRegular.Cloud24
                },
                TargetPageTag = "cloud",
                TargetPageType = new TypeDelegator(typeof(Pages.CloudContent))
            });

            navItems.Add(new NavigationViewItem
            {
                Content = Common.LocalizationManager.LocalizeValue("Tweaks"),
                Icon = new SymbolIcon
                {
                    Symbol = SymbolRegular.Toolbox24
                },
                TargetPageTag = "tweaks",
                TargetPageType = new TypeDelegator(typeof(Pages.TweaksContent))
            });

            var consoleItem = new NavigationViewItem
            {
                Content = Common.LocalizationManager.LocalizeValue("Console"),
                Icon = new SymbolIcon
                {
                    Symbol = SymbolRegular.WindowConsole20
                },
                TargetPageTag = "console",
            };
            footerNavItems.Add(consoleItem);
            consoleItem.Click += CommandLine_Click;

            footerNavItems.Add(new NavigationViewItem
            {
                Content = Common.LocalizationManager.LocalizeValue("About"),
                Icon = new SymbolIcon
                {
                    Symbol = SymbolRegular.QuestionCircle24
                },
                TargetPageTag = "about",
                TargetPageType = new TypeDelegator(typeof(Pages.AboutPage))
            });

            RootNavigation.MenuItems = navItems;
            RootNavigation.FooterMenuItems = footerNavItems;

            RootNavigation.PaneDisplayMode = NavigationViewPaneDisplayMode.Top;
            RootNavigation.IsBackButtonVisible = NavigationViewBackButtonVisible.Collapsed;
            RootNavigation.Navigate("about");
        }

        private void CommandLine_Click(object sender, RoutedEventArgs e)
        {
            var handle = App.GetConsoleWindow();

            if (_displayDebugConsole)
            {
                App.ShowWindow(handle, App.SwHide);
                _displayDebugConsole = false;
            }
            else
            {
                App.ShowWindow(handle, App.SwShow);
                _displayDebugConsole = true;
            }
        }

        private void MainWindow_OnLoaded(object? sender, EventArgs eventArgs)
        {
            RootNavigation.Navigate("dashboard");
        }
    }
}
