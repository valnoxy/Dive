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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Threading;
using Action = System.Action;
using Debug = Dive.UI.Common.UserInterface.Debug;

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
            int loadedPlugins;

            if (args.Length == 1)
            {
#if DEBUG
                ShowWindow(handle, SwShow);
#else
                ShowWindow(handle, SwHide);
#endif
                // Initialize Console
                Core.Common.Logging.Initialize();
                Debug.InitializeConsole();

                // Load plugins
                Debug.WriteLine("Loading plugins ...");
                loadedPlugins = Plugin.PluginManager.LoadPlugins(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins"));
                Debug.WriteLine($"Loaded {loadedPlugins} plugin(s).");
                Plugin.PluginManager.InitPlugins();
                Debug.WriteLine("Initialized all plugins.");

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

            Console.Title = $"{VersionInfo.ProductName} - Debug Console";
            Console.WriteLine($"{VersionInfo.ProductName} [Version: {VersionInfo.ProductVersion}]"); // Header
            Console.WriteLine(VersionInfo.LegalCopyright + "\n"); // Copyright text

#if DEBUG
            // Debug switches
            if (args.Contains("--unattend-test"))
            {
                Debug.WriteLine("Debug console initialized.");
                Debug.WriteLine("Unit Test - Unattend Compiling\n", ConsoleColor.Magenta);

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

                Debug.WriteLine("Use User: " + deploymentInfoInstance.UseUserInfo);
                Debug.WriteLine("Username: " + deploymentInfoInstance.Username);
                Debug.WriteLine("Password: " + deploymentInfoInstance.Password);
                Debug.WriteLine("Use S Mode: " + deploymentOptionInstance.UseSMode);
                Debug.WriteLine("Use Copy Path: " + deploymentOptionInstance.UseCopyProfile);
                Debug.WriteLine("Use OEM: " + oemInfoInstance.UseOemInfo);
                Debug.WriteLine("Manufacturer: " + oemInfoInstance.Manufacturer);
                Debug.WriteLine("Model: " + oemInfoInstance.Model);
                Debug.WriteLine("Support Tel.: " + oemInfoInstance.SupportPhone);
                Debug.WriteLine("Support Hours: " + oemInfoInstance.SupportHours);
                Debug.WriteLine("Support URL: " + oemInfoInstance.SupportURL);

                Debug.WriteLine("Building unattend configuration ...", ConsoleColor.DarkYellow);
                config = UnattendBuilder.Build();
                Console.WriteLine(config);

                Environment.Exit(0);
            }
#endif

            // Load plugins
            Debug.WriteLine("Loading plugins ...");
            loadedPlugins = Plugin.PluginManager.LoadPlugins(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins"));
            Debug.WriteLine($"Loaded {loadedPlugins} plugin(s).");
            Plugin.PluginManager.InitPlugins();
            Debug.WriteLine("Initialized all plugins.");
            Plugin.PluginManager.RunStartup();

            if (args.Contains("--autoinit-boot"))
            {
                // Load test config
                var testConfigJson = Initialization.ConfigurationLoader.CreateConfiguration();
                var config = Initialization.ConfigurationLoader.LoadConfiguration(testConfigJson);

                //ShowAutoInitBoot(config);
            }

            if (args.Contains("--autodive"))
            {
                Debug.InitializeConsole();
                Debug.WriteLine("Loading AutoDive UI ...");
                var adUi = new AutoDiveUI();
                adUi.ShowDialog();
                Environment.Exit(0);
            }

            Environment.Exit(0);
        }
    }
}
