﻿/* 
 * Dive (formally deploya) - Fast and Easy way to deploy Windows
 * Copyright (c) 2018 - 2022 Exploitox.
 * 
 * Dive is licensed under MIT License (https://github.com/valnoxy/dive/blob/main/LICENSE). 
 * So you are allowed to use freely and modify the application. 
 * I will not be responsible for any outcome. 
 * Proceed with any action at your own risk.
 * 
 * Source code: https://github.com/valnoxy/dive
 */

using CommandLine;
using CommandLine.Text;
using Dive.UI.AutoDive;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Reflection;
using System.IO;
using System.Windows.Threading;
using Dive.UI.Common;
using Dive.UI.Initialization;
using Action = System.Action;
using Application = System.Windows.Application;
using Dive.Core.Action.Capturing;

namespace Dive.UI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        public static readonly FileVersionInfo VersionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly()!.Location);
        public static readonly string Ver = Assembly.GetExecutingAssembly().GetName().Version!.ToString();

        #region Parser options
        [Verb("Apply", HelpText = "Add file contents to the index.")]
        class ApplyOptions
        {
            [Option('w', "wim", Required = true, HelpText = "Input WIM-file to be processed.")]
            public string wimfile { get; set; }

            [Option('i', "index", Required = true, HelpText = "Index ID of the selected Windows-Installation.")]
            public int index { get; set; }

            [Option('d', "driveid", Required = true, HelpText = "Hard Drive ID of the destination hard drive.")]
            public int driveid { get; set; }

            [Option('e', "efi", Default = false, HelpText = "Use EFI for installation.")]
            public bool efi { get; set; }

            [Option('n', "ntldr", Default = false, HelpText = "Use NTLDR bootloader for XP and below.")]
            public bool ntldr { get; set; }


            [Usage(ApplicationAlias = "Dive")]
            public static IEnumerable<Example> Examples
            {
                get
                {
                    return new List<Example>() {
                         new Example("\nApply WIM file on a EFI system", new ApplyOptions { driveid = 0, wimfile = "file.wim", index = 1, efi = true, ntldr = false }),
                         new Example("\nApply WIM file on a Legacy system", new ApplyOptions { driveid = 0, wimfile = "file.wim", index = 1, efi = false, ntldr = false }),
                         new Example("\nApply XP-based image on a Legacy system", new ApplyOptions { driveid = 0, wimfile = "file.wim", index = 1, efi = false, ntldr = true }),
                    };
                }
            }

        }

        [Verb("Capture", HelpText = "Captures a image")]
        private class CaptureOptions
        {
            [Option('c', "cap", Required = true, HelpText = "Input WIM-file to be processed.")]
            public string capturedir { get; set; }

            [Option(Default = false, Hidden = true, HelpText = "Used for deploya GUI Installation")]
            public bool uimode { get; set; }

            [Option('i', "index", Required = true, HelpText = "Index ID of the selected Windows-Installation.")]
            public int index { get; set; }

        }

        private static void DisplayHelp<T>(ParserResult<T> result, IEnumerable<Error> errs)
        {
            var helpText = HelpText.AutoBuild(result, h =>
            {
                h.AdditionalNewLineAfterOption = false; // Remove the extra newline between options
                h.Heading = $"{VersionInfo.ProductName} [Version: {Ver}]"; // Header
                h.Copyright = VersionInfo.LegalCopyright; // Copyright text
                return HelpText.DefaultParsingErrorsHandler(result, h);
            }, e => e);
            Console.WriteLine(helpText);
        }

        private static void ShowGui()
        {
            var wnd = new MainWindow();
            var splash = new SplashScreen();
            splash.Show();
            while (splash.IsVisible)
            {
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(() => { }));
            }

            // Update language
            Common.LocalizationManager.LoadLanguage(Common.LocalizationManager.CurrentLanguage.Code);

            wnd.ShowDialog();
            Environment.Exit(0);
        }

        private static void ShowAutoDive()
        {
            //var wnd = new AutoDiveUi();
            //wnd.ShowDialog();
            Environment.Exit(0);
        }

        private static void ShowAutoInitBoot(Configuration? config)
        {
            var wnd = new Initialization.BootWindow(config);
            wnd.ShowDialog();
            Environment.Exit(0);
        }

        #endregion

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

            if (args.Length == 1)
            {
#if DEBUG
                ShowWindow(handle, SwShow);
#else
                ShowWindow(handle, SwHide);
#endif
                // Initialize Console
                Common.Debug.InitializeConsole();                
                //Common.Debug.WriteLine("Debug console initialized.", ConsoleColor.White);

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
            if (args.Contains("--unattend-test"))
            {
                Console.Title = $"{VersionInfo.ProductName} - Debug Console";
                Console.WriteLine($"{VersionInfo.ProductName} [Version: {VersionInfo.ProductVersion}]"); // Header
                Console.WriteLine(VersionInfo.LegalCopyright + "\n"); // Copyright text
                Common.Debug.WriteLine("Debug console initialized.", ConsoleColor.White);
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
                config = Common.UnattendBuilder.Build();
                Console.WriteLine(config);

                Environment.Exit(0);
            }
#endif

            if (args.Contains("--autoinit-boot"))
            {
                Console.Title = $"{VersionInfo.ProductName} - Debug Console";
                Console.WriteLine($"{VersionInfo.ProductName} [Version: {VersionInfo.ProductVersion}]"); // Header
                Console.WriteLine(VersionInfo.LegalCopyright + "\n"); // Copyright text
                Console.WriteLine("Please wait ...");

                // Load test config
                var testConfigJson = Initialization.ConfigurationLoader.CreateConfiguration();
                var config = Initialization.ConfigurationLoader.LoadConfiguration(testConfigJson);

                ShowAutoInitBoot(config);
            }

            if (args.Contains("--autodive"))
            {
                Common.Debug.InitializeConsole();
                Common.Debug.WriteLine("Loading AutoDive UI ...");
                var adUi = new AutoDiveUI();
                adUi.ShowDialog();
                Environment.Exit(0);
            }

            if (args.Contains("--fuck"))
            {
                CaptureTest.TestBuildInfo();
                Environment.Exit(0);
            }

            var parser = new Parser(with => with.HelpWriter = null);
            var parserResult = parser.ParseArguments<ApplyOptions, CaptureOptions>(args);
            parserResult
                .WithParsed(options => RunA())
                .WithNotParsed(errs => DisplayHelp(parserResult, errs));
            Environment.Exit(0);
        }

        private static void RunA()
        {
            throw new NotImplementedException("The CLI version is obsolete and will be replaced in future versions. Please use the GUI version of Dive for deployment.");
        }
    }
}
