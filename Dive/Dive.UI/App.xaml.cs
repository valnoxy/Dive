/*
 * Dive (formally deploya) - Deployment is very easy.
 * Copyright (c) 2018 - 2024 Exploitox.
 *
 * Dive is licensed under MIT License (https://github.com/valnoxy/dive/blob/main/LICENSE).
 * So you are allowed to use freely and modify the application.
 * I will not be responsible for any outcome.
 * Proceed with any action at your own risk.
 *
 * Source code: https://github.com/valnoxy/dive
 */

using Dive.UI.AutoDive;
using Dive.UI.Common;
using System;
using System.Linq;
using System.Windows;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Reflection;
using System.IO;
using System.Windows.Threading;
using Action = System.Action;

namespace Dive.UI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        public static readonly FileVersionInfo VersionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly()!.Location);
        public static readonly string Ver = Assembly.GetExecutingAssembly().GetName().Version!.ToString();

        private static void ShowGui()
        {
            var wnd = new MainWindow();
            var splash = new SplashScreen();
            splash.Show();
            while (splash.IsVisible)
            {
                Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(() => { }));
            }

            // Update language
            LocalizationManager.LoadLanguage(LocalizationManager.CurrentLanguage.Code);

            wnd.ShowDialog();
            Environment.Exit(0);
        }

        private static void ShowAutoDive()
        {
            //var wnd = new AutoDiveUi();
            //wnd.ShowDialog();
            Environment.Exit(0);
        }

        #region Console Window State

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        public const int SwHide = 0;
        public const int SwShow = 5;

        #endregion

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var args = Environment.GetCommandLineArgs();
            var handle = GetConsoleWindow();
            var loadedPlugins = 0;

            if (args.Length == 1)
            {
#if DEBUG
                ShowWindow(handle, SwShow);
#else
                ShowWindow(handle, SwHide);
#endif
                // Initialize Console
                Common.Debug.InitializeConsole();

                // Load plugins
                Common.Debug.WriteLine("Loading plugins ...");
                loadedPlugins = Plugin.PluginManager.LoadPlugins(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins"));
                Common.Debug.WriteLine($"Loaded {loadedPlugins} plugin(s).");
                Plugin.PluginManager.InitPlugins();
                Common.Debug.WriteLine("Initialized all plugins.");

                var allDrives = DriveInfo.GetDrives();
                foreach (var d in allDrives)
                {
                    if (File.Exists(Path.Combine(d.Name, ".diveconfig")))
                    {
#if RELEASE
                        if (!File.Exists("X:\\Windows\\System32\\wpeinit.exe")) ShowGui();
#endif
                        const string message = "Auto deployment config detected. Do you want to perform the deployment now?";
                        const string title = "AutoDive";
                        const string btn1 = "No";
                        const string btn2 = "Yes";

                        var w = new MessageUI(title, message, btn1, btn2, true, 5);
                        if (w.ShowDialog() != false) continue;
                        var summary = w.Summary;
                        if (summary == "Btn2")
                        {
                            ShowAutoDive();
                        }
                    }
                }

                ShowGui();
            }

#if DEBUG
            // Debug switches
            if (args.Contains("--unattend-test"))
            {
                Console.Title = $"{VersionInfo.ProductName} - Debug Console";
                Console.WriteLine($"{VersionInfo.ProductName} [Version: {VersionInfo.ProductVersion}]"); // Header
                Console.WriteLine(VersionInfo.LegalCopyright + "\n"); // Copyright text
                Common.Debug.WriteLine("Debug console initialized.");
                Common.Debug.WriteLine("Unit Test - Unattend Compiling\n", ConsoleColor.Magenta);

                var deploymentInfoInstance = DeploymentInfo.Instance;
                var deploymentOptionInstance = DeploymentOption.Instance;
                var oemInfoInstance = OemInfo.Instance;

                var config = "";
                deploymentInfoInstance.UseUserInfo = true;
                deploymentInfoInstance.Username = "User";
                deploymentInfoInstance.Password = "Pa$$w0rd";
                deploymentOptionInstance.UseCopyProfile = true;
                deploymentOptionInstance.UseSMode = true;
                oemInfoInstance.UseOemInfo = true;
                oemInfoInstance.Model = "Toaster";
                oemInfoInstance.Manufacturer = "Fabrikam";
                oemInfoInstance.SupportHours = "24/7";
                oemInfoInstance.SupportURL = "https://fabrikam.com";
                oemInfoInstance.SupportPhone = "+1 111 11111111";
                
                Common.Debug.WriteLine("Use User: " + deploymentInfoInstance.UseUserInfo);
                Common.Debug.WriteLine("Username: " + deploymentInfoInstance.Username);
                Common.Debug.WriteLine("Password: " + deploymentInfoInstance.Password);
                Common.Debug.WriteLine("Use S Mode: " + deploymentOptionInstance.UseSMode);
                Common.Debug.WriteLine("Use Copy Path: " + deploymentOptionInstance.UseCopyProfile);
                Common.Debug.WriteLine("Use OEM: " + oemInfoInstance.UseOemInfo);
                Common.Debug.WriteLine("Manufacturer: " + oemInfoInstance.Manufacturer);
                Common.Debug.WriteLine("Model: " + oemInfoInstance.Model);
                Common.Debug.WriteLine("Support Tel.: " + oemInfoInstance.SupportPhone);
                Common.Debug.WriteLine("Support Hours: " + oemInfoInstance.SupportHours);
                Common.Debug.WriteLine("Support URL: " + oemInfoInstance.SupportURL);

                Common.Debug.WriteLine("Building unattend configuration ...", ConsoleColor.DarkYellow);
                config = UnattendBuilder.Build();
                Console.WriteLine(config);

                Environment.Exit(0);
            }
#endif

            // Load plugins
            Common.Debug.WriteLine("Loading plugins ...");
            loadedPlugins = Plugin.PluginManager.LoadPlugins(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins"));
            Common.Debug.WriteLine($"Loaded {loadedPlugins} plugin(s).");
            Plugin.PluginManager.InitPlugins();
            Common.Debug.WriteLine("Initialized all plugins.");

            if (args.Contains("--autoinit-boot"))
            {
                Console.Title = $"{VersionInfo.ProductName} - Debug Console";
                Console.WriteLine($"{VersionInfo.ProductName} [Version: {VersionInfo.ProductVersion}]"); // Header
                Console.WriteLine(VersionInfo.LegalCopyright + "\n"); // Copyright text
                Console.WriteLine("Please wait ...");

                // Load test config
                var testConfigJson = Initialization.ConfigurationLoader.CreateConfiguration();
                var config = Initialization.ConfigurationLoader.LoadConfiguration(testConfigJson);

                //ShowAutoInitBoot(config);
            }

            if (args.Contains("--autodive"))
            {
                Common.Debug.InitializeConsole();
                Common.Debug.WriteLine("Loading AutoDive UI ...");
                var adUi = new AutoDiveUI();
                adUi.ShowDialog();
                Environment.Exit(0);
            }

            Environment.Exit(0);
        }
    }
}
