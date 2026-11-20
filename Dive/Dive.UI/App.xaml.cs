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
using Dive.UI.Common.Configuration;
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

        /// <summary>
        /// Shows the splash screen, which initializes Dive. Returns false if the initialization failed.
        /// </summary>
        private static bool RunSplashScreen()
        {
            var splash = new SplashScreen();
            splash.ShowDialog();
            return splash.Succeeded;
        }

        private static void ShowGui()
        {
            var wnd = new MainWindow();
            // The splash screen was the first window, so it would otherwise stay Application.MainWindow.
            Current.MainWindow = wnd;
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

                // Windows are opened one after another (splash -> main window), so closing the
                // splash screen must not end the application.
                Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;

                // Splash screen: language, plugins, core, license and hardware check
                if (!RunSplashScreen())
                    Environment.Exit(1);

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

            Core.Common.Logging.Initialize();
            Debug.InitializeConsole();

#if DEBUG
            // Debug switches
            if (args.Contains("--unattend-test"))
            {
                Debug.WriteLine("Debug console initialized.");
                Debug.WriteLine("Unit Test - Unattend Compiling\n", ConsoleColor.Magenta);

                var setup = new SetupConfiguration
                {
                    Mode = SetupMode.Advanced,
                    UserName = "User",
                    Password = "Pa$$w0rd",
                    ComputerName = "TEST-PC",
                    Locale = "de-DE",
                    TimeZone = "W. Europe Standard Time",
                    CopyProfile = true,
                    SMode = true,
                    OemManufacturer = "Fabrikam",
                    OemModel = "Toaster",
                    OemSupportHours = "24/7",
                    OemSupportUrl = "https://fabrikam.com",
                    OemSupportPhone = "+1 111 11111111"
                };
                var image = ApplyDetails.Instance;
                image.NTVersion = "10.0";
                image.Architecture = "NT 10.0.22621.525 - x64";

                foreach (var item in setup.GetSummary(isModernWindows: true))
                    Debug.WriteLine($"[{item.Kind}] {item.Text}");

                Debug.WriteLine("Building unattend configuration ...", ConsoleColor.DarkYellow);
                Console.WriteLine(UnattendBuilder.Build(setup, image));

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
