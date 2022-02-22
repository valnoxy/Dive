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

using CommandLine;
using CommandLine.Text;
using Microsoft.Wim;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace deploya_core
{
    internal class Program
    {
        public static string ver = "12.0.0";
        public static string build = "401";
        public static string codename = "deploya Core";
        public static string copyright = "Copyright (c) 2018 - 2022 Exploitox. All rights reserved.";
        public static bool uimode = false;

        #region Parser options
        class Options
        {
            [Option('w', "wim", Required = true, HelpText = "Input WIM-file to be processed.")]
            public string wimfile { get; set; }


            [Option(Default = false, Hidden = true, HelpText = "Used for deploya UI - Network Installation")]
            public bool uimode { get; set; }


            [Option('i', "index", Required = true, HelpText = "Index ID of the selected Windows-Installation.")]
            public int index { get; set; }


            [Option('e', "efi", Default = false, HelpText = "Use EFI for installation.")]
            public bool efi { get; set; }


            [Option('d', "driveid", Required = true, HelpText = "Hard Drive ID of the destination hard drive.")]
            public int driveid { get; set; }


            [Option('l', "legacy", Default = false, HelpText = "Use legacy bootloader for XP and below.")]
            public bool legacy { get; set; }

            /*
            [Usage(ApplicationAlias = "deployaCLI")]
            public static IEnumerable<Example> Examples
            {
                get
                {
                    return new List<Example>() {
                         new Example("Apply WIM file on a EFI system", new Options { driveid = 0, wimfile = "file.wim", index = 1, efi = true, legacy = false })
                    };
                }
            }
            */
        }

        static void DisplayHelp<T>(ParserResult<T> result, IEnumerable<Error> errs)
        {
            var helpText = HelpText.AutoBuild(result, h =>
            {
                h.AdditionalNewLineAfterOption = false; // Remove the extra newline between options
                h.Heading = Program.codename + " [Version: " + Program.ver + "." + Program.build + "]"; // Change header
                h.Copyright = Program.copyright; // Change copyright text
                return HelpText.DefaultParsingErrorsHandler(result, h);
            }, e => e);
            Console.WriteLine(helpText);
        }
        #endregion

        static void Main(string[] args)
        {
            var parser = new CommandLine.Parser(with => with.HelpWriter = null);
            var parserResult = parser.ParseArguments<Options>(args);
            parserResult
             .WithParsed<Options>(options => Run(options))
             .WithNotParsed(errs => DisplayHelp(parserResult, errs));
        }

        private static void Run(Options options)
        {
            string image = options.wimfile.ToString();
            string Index = options.index.ToString();
            string diskId = options.driveid.ToString();
            uimode = options.uimode;

            if (diskId.Contains("\\\\.\\PHYSICALDRIVE"))
                diskId = new string(Enumerable.ToArray<char>(Enumerable.Where<char>((IEnumerable<char>)diskId, new Func<char, bool>(char.IsDigit))));

            // -----------------------------
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
            if (Program.GetDiskIndex(diskId) > 0U)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[!] Target not exist. ID: " + Program.GetDiskIndex(diskId).ToString());
                Console.ResetColor();
                Environment.Exit(1);
            }
            Console.WriteLine("[i] Target    = disk" + diskId);
            #endregion

            #region BIOS type & Bootloader

            if (options.efi && options.legacy)
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
            else if (options.legacy)
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
            // -----------------------------

            #region Partitioning destination
            Process partDest = new Process();
            partDest.StartInfo.FileName = "diskpart.exe";
            partDest.StartInfo.UseShellExecute = false;
            partDest.StartInfo.CreateNoWindow = true;
            partDest.StartInfo.RedirectStandardInput = true;
            partDest.StartInfo.RedirectStandardOutput = true;
            partDest.Start();
            if (!options.efi)
            {
                if (options.legacy)
                {
                    partDest.StandardInput.WriteLine("select disk " + diskId);
                    partDest.StandardInput.WriteLine("clean");
                    partDest.StandardInput.WriteLine("create partition primary");
                    partDest.StandardInput.WriteLine("format quick fs=ntfs label=Windows");
                    partDest.StandardInput.WriteLine("active");
                    partDest.StandardInput.WriteLine("assign letter=W");
                    partDest.StandardInput.WriteLine("exit");
                    partDest.WaitForExit();
                    if (!Program.uimode)
                        ConsoleUtility.WriteProgressBar(100, true);
                }
                if (!options.legacy)
                {
                    partDest.StandardInput.WriteLine("select disk " + diskId);
                    partDest.StandardInput.WriteLine("clean");
                    partDest.StandardInput.WriteLine("create partition primary size=100");
                    partDest.StandardInput.WriteLine("format quick fs=ntfs label=System");
                    partDest.StandardInput.WriteLine("assign letter=S");
                    partDest.StandardInput.WriteLine("active");
                    partDest.StandardInput.WriteLine("create partition primary");
                    partDest.StandardInput.WriteLine("shrink minimum=650");
                    partDest.StandardInput.WriteLine("format quick fs=ntfs label=Windows");
                    partDest.StandardInput.WriteLine("assign letter=W");
                    partDest.StandardInput.WriteLine("create partition primary");
                    partDest.StandardInput.WriteLine("format quick fs=ntfs label=Recovery");
                    partDest.StandardInput.WriteLine("assign letter=R");
                    partDest.StandardInput.WriteLine("set id=27");
                    partDest.StandardInput.WriteLine("exit");
                    partDest.WaitForExit();
                    if (!Program.uimode)
                        ConsoleUtility.WriteProgressBar(100, true);
                }
            }
            if (options.efi)
            {
                partDest.StandardInput.WriteLine("select disk " + diskId);
                partDest.StandardInput.WriteLine("clean");
                partDest.StandardInput.WriteLine("convert gpt");
                partDest.StandardInput.WriteLine("create partition efi size=100");
                partDest.StandardInput.WriteLine("format quick fs=fat32 label=System");
                partDest.StandardInput.WriteLine("assign letter=S");
                partDest.StandardInput.WriteLine("create partition msr size=16");
                partDest.StandardInput.WriteLine("create partition primary");
                partDest.StandardInput.WriteLine("shrink minimum=650");
                partDest.StandardInput.WriteLine("format quick fs=ntfs label=Windows");
                partDest.StandardInput.WriteLine("assign letter=W");
                partDest.StandardInput.WriteLine("create partition primary");
                partDest.StandardInput.WriteLine("format quick fs=ntfs label=Recovery");
                partDest.StandardInput.WriteLine("assign letter=R");
                partDest.StandardInput.WriteLine("set id=de94bba4-06d1-4d40-a16a-bfd50179d6ac");
                partDest.StandardInput.WriteLine("gpt attributes=0x8000000000000001");
                partDest.StandardInput.WriteLine("exit");
                partDest.WaitForExit();
                if (!Program.uimode)
                    ConsoleUtility.WriteProgressBar(100, true);
            }

            if (partDest.ExitCode != 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("");
                Console.WriteLine("   An Error has occurred.");
                Console.WriteLine("   Error: " + partDest.ExitCode.ToString());
                if (!Program.uimode)
                    Console.WriteLine(); // Only write new line if ui mode is disabled, so that the ui can read the error code above.
                Console.ResetColor();
                return;
            }
            #endregion

            #region Apply WIM file & Bootloader
            Program.applyWim(image, Index, "W:\\");

            if (!Program.uimode)
            {
                Console.Write("[*] Installing Bootloader ...     ");
                ConsoleUtility.WriteProgressBar(0);
            }
            if (Program.uimode) { Console.WriteLine("[A] Installing Bootloader ..."); }

            Process bootld = new Process();
            bootld.StartInfo.FileName = "cmd.exe";

            #region Legacy check
            if (options.legacy)
            {
                if (diskId.EndsWith("\\"))
                {
                    diskId = diskId.Remove(diskId.Length - 1);
                    bootld.StartInfo.Arguments = "/c \"bootsect /nt52 " + diskId + " /force /mbr\"";
                }
                else
                {
                    bootld.StartInfo.Arguments = "/c \"bootsect.exe /nt52 " + diskId + " /force /mbr > nul\"";
                }
            }
            #endregion

            #region BIOS / EFI check
            if (!options.legacy)
            {
                if (!options.efi) // BIOS
                    bootld.StartInfo.Arguments = "/c \"bcdboot.exe W:\\Windows /s S: /f BIOS >NUL\"";

                if (options.efi) // EFI
                    bootld.StartInfo.Arguments = "/c \"bcdboot.exe W:\\Windows /s S: /f UEFI >NUL\"";
            }
            #endregion

            bootld.Start();
            bootld.WaitForExit();

            if (bootld.ExitCode != 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("");
                Console.WriteLine("   An Error has occurred.");
                Console.WriteLine("   Error: " + bootld.ExitCode.ToString());
                if (!Program.uimode)
                    Console.WriteLine(); // Only write new line if ui mode is disabled, so that the ui can read the error code above.
                Console.ResetColor();
                Environment.Exit(bootld.ExitCode);
            }

            if (!Program.uimode)
            {
                ConsoleUtility.WriteProgressBar(100, true);
                Console.WriteLine("");
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("[*] Installation completed.");
            Console.ResetColor();
            Environment.Exit(0);
            #endregion
        }

        #region Modules

        #region Get Disk index
        public static int GetDiskIndex(string diskId)
        {
            string tempPath = Path.GetTempPath();
            File.WriteAllText(tempPath + "getdiskindex.cmd", "@wmic diskdrive get index | more +1");
            Process process = new Process();
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.FileName = tempPath + "getdiskindex.cmd";
            process.Start();
            string end = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            try { File.Delete(Path.Combine(tempPath, "getdiskindex.cmd")); } catch { }
            return end.Contains(diskId) ? 0 : -1;
        }
        #endregion

        #region Apply Wim file
        private static void applyWim(string ImagePath, string Index, string Drive)
        {
            Environment.GetCommandLineArgs();
            if (!Program.uimode) 
            { 
                Console.Write("[*] Applying Image ...            "); 
                ConsoleUtility.WriteProgressBar(0);
            }
            if (Program.uimode) { Console.WriteLine("[A] Applying Image ..."); }
            
            string path = Drive;
            using (WimHandle file = WimgApi.CreateFile(ImagePath, WimFileAccess.Read, WimCreationDisposition.OpenExisting, WimCreateFileOptions.None, WimCompressionType.None))
            {
                WimgApi.SetTemporaryPath(file, Environment.GetEnvironmentVariable("TEMP"));
                WimgApi.RegisterMessageCallback(file, new WimMessageCallback(Program.MyCallbackMethod));
                try
                {
                    using (WimHandle imageHandle = WimgApi.LoadImage(file, Convert.ToInt32(Index)))
                        WimgApi.ApplyImage(imageHandle, path, WimApplyImageOptions.None);
                }
                finally
                {
                    WimgApi.UnregisterMessageCallback(file, new WimMessageCallback(Program.MyCallbackMethod));
                }
            }
        }
        #endregion

        #region WimCallbackMessage
        private static WimMessageResult MyCallbackMethod(WimMessageType messageType, object message, object userData)
        {
            switch (messageType)
            {
                case WimMessageType.Progress:
                    WimMessageProgress wimMessageProgress = (WimMessageProgress)message;
                    if (!Program.uimode)
                        ConsoleUtility.WriteProgressBar(wimMessageProgress.PercentComplete, true);
                    if (Program.uimode)
                    {
                        Console.WriteLine(wimMessageProgress.PercentComplete);
                        break;
                    }
                    break;
                case WimMessageType.Error:
                    WimMessageError wimMessageError = (WimMessageError)message;
                    Console.WriteLine($"Error: {0} ({1})", (object)wimMessageError.Path, (object)wimMessageError.Win32ErrorCode);
                    break;
                case WimMessageType.Warning:
                    WimMessageWarning wimMessageWarning = (WimMessageWarning)message;
                    Console.WriteLine($"Warning: {0} ({1})", (object)wimMessageWarning.Path, (object)wimMessageWarning.Win32ErrorCode);
                    break;
            }
            return WimMessageResult.Success;
        }
        #endregion

        #endregion
    }
}
