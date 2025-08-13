using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Principal;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Dive.Core.Common;
using Dive.UI.Pages.Extras.FunPages;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;
using Debug = Dive.UI.Common.UserInterface.Debug;

namespace Dive.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private bool _displayDebugConsole;
        private string _currentTheme = "Mica";
        private readonly DispatcherTimer _timer = new()
        {
            Interval = TimeSpan.FromSeconds(1)
        };

        public MainWindow()
        {
            InitializeComponent();

#if DEBUG
            _displayDebugConsole = true; 
            Branch.Text = "Development";
            CloudPage.Visibility = Visibility.Visible;
            TweaksPage.Visibility = Visibility.Visible;
#else
            _displayDebugConsole = true;
            Branch.Text = "Pre-Release 2";
            CloudPage.Visibility = Visibility.Collapsed;
            TweaksPage.Visibility = Visibility.Collapsed;
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

            // Check for valid license
            if (File.Exists("Dive.lic"))
            {
                var licenseData = File.ReadAllText("Dive.lic");
                Common.Licensing.Validation.Validate(licenseData);
            }
            else
            {
                var allDrives = DriveInfo.GetDrives();
                foreach (var d in allDrives)
                {
                    if (File.Exists($"{d.Name}Dive.lic"))
                    {
                        var licenseData = File.ReadAllText($"{d.Name}Dive.lic");
                        Common.Licensing.Validation.Validate(licenseData);
                        break;
                    }
                    if (File.Exists($"{d.Name}Dive\\Dive.lic"))
                    {
                        var licenseData = File.ReadAllText($"{d.Name}Dive\\Dive.lic");
                        Common.Licensing.Validation.Validate(licenseData);
                        break;
                    }
                }
            }
            if (Common.Licensing.Validation.Info.Valid)
            {
                FoundersBanner.Visibility = Visibility.Visible;
                Debug.WriteLine("Founders License found!", ConsoleColor.Green);
                Logging.Log("Founders License found!");
            }
            else if (Common.Licensing.Validation.Info.ValidationFailed)
            {
                Debug.WriteLine("License validation failed! Invalid license detected.", ConsoleColor.Red);
                Logging.Log("License validation failed! Invalid license detected.", Logging.LogLevel.ERROR);
            }
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
            // run plugin startup
            Common.Plugin.PluginManager.RunStartup();

            RootNavigation.Navigate(typeof(Pages.Home));
        }

        private void UIElement_OnDrop(object sender, DragEventArgs e)
        {
            Debug.WriteLine("Drop");
        }

        private void UIElement_OnDragEnter(object sender, DragEventArgs e)
        {
            Debug.WriteLine("DragEnter");
        }

        private void ThemeSwitch_Click(object sender, RoutedEventArgs e)
        {
            //WindowBackdropType = WindowBackdropType == WindowBackdropType.Mica ? WindowBackdropType.Tabbed : WindowBackdropType.Mica;
            switch (_currentTheme)
            {
                case "Mica":
                    ApplicationThemeManager.Apply(
                        ApplicationTheme.Dark,
                        WindowBackdropType.Tabbed);
                    _currentTheme = "Tabbed";
                    break;
                case "Tabbed":
                    //WindowBackdropType = WindowBackdropType.Mica;
                    _currentTheme = "Light";
                    ApplicationThemeManager.Apply(
                        ApplicationTheme.Light,
                        WindowBackdropType.Mica);
                    break;
                case "Light":
                    _currentTheme = "Mica";
                    ApplicationThemeManager.Apply(
                        ApplicationTheme.Dark,
                        WindowBackdropType.Mica);
                    break;
            }
        }

        private void FunIcon_OnClick(object sender, RoutedEventArgs e)
        {
            var w = new FunSettings();
            w.ShowDialog();
        }

        private void MainWindow_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                FunIcon.Visibility = Visibility.Visible;
            }
        }
    }
}
