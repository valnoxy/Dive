using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Security.Principal;
using System.Windows;
using System.Windows.Threading;
using Wpf.Ui.Common;
using Wpf.Ui.Controls;
using Wpf.Ui.Controls.Navigation;

namespace Dive.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private bool _displayDebugConsole;
        private readonly DispatcherTimer _timer = new()
        {
            Interval = TimeSpan.FromSeconds(1)
        };

        public MainWindow()
        {
            InitializeComponent();
            
#if DEBUG
            DebugString.Visibility = Visibility.Visible;
            DebugString.Text = "Debug build - This is not a production ready build.";
            _displayDebugConsole = true;
            Branch.Text = "Development";
#else
            DebugString.Visibility = Visibility.Visible;
            DebugString.Text = "Public Beta Build 7";
            Branch.Text = "Stable";
#endif
            // Get version
            var assembly = Assembly.GetExecutingAssembly();
            var fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            var version = fvi.FileVersion;
            Version.Text = $"Version {version}";

            // Get User
            var accountToken = WindowsIdentity.GetCurrent().Token;
            var windowsIdentity = new WindowsIdentity(accountToken);
            UserName.Text = windowsIdentity.Name;

            // Time & Date
            _timer.Tick += UpdateTimeAndDate_Tick!;
            _timer.Start();
        }

        private void UpdateTimeAndDate_Tick(object sender, EventArgs e)
        {
            Time.Text = DateTime.Now.ToShortTimeString();
            Date.Text = DateTime.Now.ToShortDateString();
        }

        private void RootNavigation_OnLoaded(object sender, RoutedEventArgs e)
        {
            // Build navigation menu
           
            var navItems = new List<INavigationViewItem>
            {
                new NavigationViewItem
                {
                    Content = Common.LocalizationManager.LocalizeValue("Home"),
                    Icon = new SymbolIcon
                    {
                        Symbol = SymbolRegular.Home24
                    },
                    TargetPageTag = "dashboard",
                    TargetPageType = new TypeDelegator(typeof(Pages.Dashboard)),

                },
                new NavigationViewItem
                {
                    Content = Common.LocalizationManager.LocalizeValue("Apply"),
                    Icon = new SymbolIcon
                    {
                        Symbol = SymbolRegular.WindowApps24
                    },
                    TargetPageTag = "apply",
                    TargetPageType = new TypeDelegator(typeof(Pages.ApplyContent))
                },
                new NavigationViewItem
                {
                    Content = Common.LocalizationManager.LocalizeValue("Capture"),
                    Icon = new SymbolIcon
                    {
                        Symbol = SymbolRegular.Copy24
                    },
                    TargetPageTag = "capture",
                    TargetPageType = new TypeDelegator(typeof(Pages.CaptureContent))
                },
                new NavigationViewItem
                {
                    Content = Common.LocalizationManager.LocalizeValue("Cloud"),
                    Icon = new SymbolIcon
                    {
                        Symbol = SymbolRegular.Cloud24
                    },
                    TargetPageTag = "cloud",
                    TargetPageType = new TypeDelegator(typeof(Pages.CloudContent))
                },
                new NavigationViewItem
                {
                    Content = Common.LocalizationManager.LocalizeValue("Tweaks"),
                    Icon = new SymbolIcon
                    {
                        Symbol = SymbolRegular.Toolbox24
                    },
                    TargetPageTag = "tweaks",
                    TargetPageType = new TypeDelegator(typeof(Pages.TweaksContent))
                }
            };
            var footerNavItems = new List<INavigationViewItem>();
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
            RootNavigation.Navigate(new TypeDelegator(typeof(Pages.AboutPage)));
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
            RootNavigation.Navigate("dashboard"); // Workaround
        }

        private void UIElement_OnDrop(object sender, DragEventArgs e)
        {
            Common.Debug.WriteLine("Drop");
        }

        private void UIElement_OnDragEnter(object sender, DragEventArgs e)
        {
            Common.Debug.WriteLine("DragEnter");
        }
    }
}
