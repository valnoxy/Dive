/* 
 * deploya - Fast and Easy way to deploy Windows
 * Copyright (c) 2018 - 2022 Exploitox.
 * 
 * deploya is licensed under MIT License (https://github.com/valnoxy/deploya/blob/main/LICENSE). 
 * So you are allowed to use freely and modify the application. 
 * I will not be responsible for any outcome. 
 * Proceed with any action at your own risk.
 * 
 * Source code: https://github.com/valnoxy/deploya
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Diagnostics;
using System.Runtime.InteropServices;
using CommandLine;
using CommandLine.Text;
using System.Reflection;
using System.IO;
using deploya_core;

// Debug
using System.Xml;
// End Debug

namespace deploya
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static string ver = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        public static string codename = "deploya";
        public static string copyright = "Copyright (c) 2018 - 2022 Exploitox. All rights reserved.";

        #region Parser options
        class Options
        {
            [Option('w', "wim", Required = true, HelpText = "Input WIM-file to be processed.")]
            public string wimfile { get; set; }

            [Option(Default = false, Hidden = true, HelpText = "Used for deploya GUI Installation")]
            public bool uimode { get; set; }

            [Option('i', "index", Required = true, HelpText = "Index ID of the selected Windows-Installation.")]
            public int index { get; set; }

            [Option('d', "driveid", Required = true, HelpText = "Hard Drive ID of the destination hard drive.")]
            public int driveid { get; set; }

            [Option('e', "efi", Default = false, HelpText = "Use EFI for installation.")]
            public bool efi { get; set; }

            [Option('n', "ntldr", Default = false, HelpText = "Use NTLDR bootloader for XP and below.")]
            public bool ntldr { get; set; }


            [Usage(ApplicationAlias = "deploya")]
            public static IEnumerable<Example> Examples
            {
                get
                {
                    return new List<Example>() {
                         new Example("\nApply WIM file on a EFI system", new Options { driveid = 0, wimfile = "file.wim", index = 1, efi = true, ntldr = false }),
                         new Example("\nApply WIM file on a Legacy system", new Options { driveid = 0, wimfile = "file.wim", index = 1, efi = false, ntldr = false }),
                         new Example("\nApply XP-based image on a Legacy system", new Options { driveid = 0, wimfile = "file.wim", index = 1, efi = false, ntldr = true }),
                    };
                }
            }

        }

        static void DisplayHelp<T>(ParserResult<T> result, IEnumerable<Error> errs)
        {
            var helpText = HelpText.AutoBuild(result, h =>
            {
                h.AdditionalNewLineAfterOption = false; // Remove the extra newline between options
                h.Heading = $"{codename} [Version: {ver}]"; // Header
                h.Copyright = App.copyright; // Copyright text
                return HelpText.DefaultParsingErrorsHandler(result, h);
            }, e => e);
            Console.WriteLine(helpText);
        }

        static void ShowGUI()
        {
            MainWindow wnd = new MainWindow();
            wnd.ShowDialog();
            Environment.Exit(0);
        }
        #endregion

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;
        // const int SW_SHOW = 5;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            string[] args = Environment.GetCommandLineArgs();
            var handle = GetConsoleWindow();
            ShowWindow(handle, SW_HIDE);

            if (args.Length == 1) 
            {                
                ShowGUI();
            }

            var parser = new CommandLine.Parser(with => with.HelpWriter = null);
            var parserResult = parser.ParseArguments<Options>(args);
            parserResult
             .WithParsed<Options>(options => Run(options))
             .WithNotParsed(errs => DisplayHelp(parserResult, errs));
            Environment.Exit(0);
        }

        private static void Run(Options options)
        {
            Entities.Firmware firmware = new Entities.Firmware();
            Entities.Bootloader bootloader = new Entities.Bootloader();
            Entities.UI ui = new Entities.UI();

            // Firmware definition
            if (options.efi) { firmware = Entities.Firmware.EFI; }
            if (!options.efi) { firmware = Entities.Firmware.BIOS; }

            // Bootloader definition
            if (options.ntldr) { bootloader = Entities.Bootloader.NTLDR; }
            if (!options.ntldr) { bootloader = Entities.Bootloader.BOOTMGR; }

            // UI definition
            if (options.uimode) { ui = Entities.UI.Graphical; }
            if (!options.uimode) { ui = Entities.UI.Command; }

            // CLI verify
            string image = options.wimfile.ToString();
            string Index = options.index.ToString();
            string diskId = options.driveid.ToString();

            if (diskId.Contains("\\\\.\\PHYSICALDRIVE"))
                diskId = new string(Enumerable.ToArray<char>(Enumerable.Where<char>((IEnumerable<char>)diskId, new Func<char, bool>(char.IsDigit))));

            #region Check options

            #region WIM-File
            Console.ForegroundColor = ConsoleColor.Magenta;
            if (!File.Exists(image))
            {
                Console.WriteLine("[i] Image not exist.");
                Console.ForegroundColor = (ConsoleColor)15;
                Environment.Exit(1);
            }
            Console.WriteLine("[i] Image     = " + image);
            #endregion

            #region Target
            if (App.GetDiskIndex(diskId) > 0U)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[!] Target not exist. ID: " + App.GetDiskIndex(diskId).ToString());
                Console.ResetColor();
                Environment.Exit(1);
            }
            Console.WriteLine("[i] Target    = disk" + diskId);
            #endregion

            #region BIOS type & Bootloader

            if (options.efi && options.ntldr)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[!] You cannot use EFI with a legacy bootloader. Aborting ...");
                Console.ResetColor();
                Environment.Exit(1);
            }

            if (options.efi)
            {
                Console.WriteLine("[i] Firmware  = EFI");
                Console.WriteLine("[i] Legacy    = false");
            }
            else if (options.ntldr)
            {
                Console.WriteLine("[i] Firmware  = BIOS");
                Console.WriteLine("[i] Legacy    = true");
            }
            else
            {
                Console.WriteLine("[i] Firmware  = BIOS");
                Console.WriteLine("[i] Legacy    = false");
            }

            #endregion

            #endregion

            Actions.PrepareDisk(firmware, bootloader, ui, options.driveid);
            Actions.ApplyWIM(ui, "W:\\", options.wimfile, options.index);

            if (bootloader == Entities.Bootloader.BOOTMGR)
                Actions.InstallBootloader(firmware, bootloader, ui, "W:\\", "S:\\");

            if (bootloader == Entities.Bootloader.NTLDR)
                Actions.InstallBootloader(firmware, bootloader, ui, "W:\\", "W:\\");
        }

        #region Get Disk index
        public static int GetDiskIndex(string diskId)
        {
            // string tempPath = Path.GetTempPath();
            // File.WriteAllText(tempPath + "getdiskindex.cmd", "@wmic diskdrive get index | more +1");
            Process process = new Process();
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.Arguments = "/c \"@wmic diskdrive get index | more +1\"";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.Start();
            string end = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            // try { File.Delete(Path.Combine(tempPath, "getdiskindex.cmd")); } catch { }
            return end.Contains(diskId) ? 0 : -1;
        }
        #endregion
    }
}
