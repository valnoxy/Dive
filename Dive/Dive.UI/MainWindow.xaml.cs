﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Security.Principal;
using System.Windows;
using System.Windows.Threading;
using Wpf.Ui.Controls;

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
            DebugString.Visibility = Visibility.Hidden;
            //DebugString.Text = "Pre-Release 2";
            Branch.Text = "Pre-Release 2";
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
            RootNavigation.Navigate(typeof(Pages.Dashboard));
        }

        private void UIElement_OnDrop(object sender, DragEventArgs e)
        {
            Common.Debug.WriteLine("Drop");
        }

        private void UIElement_OnDragEnter(object sender, DragEventArgs e)
        {
            Common.Debug.WriteLine("DragEnter");
        }

        private void ThemeSwitch_Click(object sender, RoutedEventArgs e)
        {
            WindowBackdropType = WindowBackdropType == WindowBackdropType.Mica ? WindowBackdropType.Tabbed : WindowBackdropType.Mica;
        }
    }
}
