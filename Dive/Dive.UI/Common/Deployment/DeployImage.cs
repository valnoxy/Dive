using System;
using System.ComponentModel;
using System.IO;
using Dive.Core;
using Dive.Core.Common;
using Newtonsoft.Json;
using static Dive.Core.Common.Entities;

namespace Dive.UI.Common.Deployment
{
    internal class DeployImage
    {
        private static readonly ApplyDetails ApplyDetailsInstance = ApplyDetails.Instance;
        private static readonly DeploymentInfo DeploymentInfoInstance = DeploymentInfo.Instance;

        internal class Configuration
        {
            internal static bool IsCanceled = false;
            internal static int DriverCount;
            internal static Firmware Firmware {get; set;}
            internal static Bootloader BootLoader { get; set;}
            internal static PartitionStyle PartitionStyle { get; set;}
            internal static string? WindowsDrive { get; set; }
            internal static string? BootDrive { get; set; }
            internal static string? RecoveryDrive { get; set; }
        }

        internal static void ApplyWindowsImage(DoWorkEventArgs e, BackgroundWorker worker)
        {
            Debug.WriteLine("Using new deployment class 'DeployImage v2'", ConsoleColor.DarkGreen);

            // Initialization
            InitializeWorkingEnvironment();
            Debug.Write("Selected Partition Style: ");
            Debug.Write($"{Configuration.PartitionStyle}\n", true, ConsoleColor.DarkYellow);

            Debug.Write("The Windows partition will use the following drive letter: ");
            Debug.Write($"{Configuration.WindowsDrive}\n", true, ConsoleColor.DarkYellow);

            Debug.Write("The Boot partition will use the following drive letter: ");
            Debug.Write($"{Configuration.BootDrive}\n", true, ConsoleColor.DarkYellow);

            Debug.Write("The Recovery partition will use the following drive letter: ");
            Debug.Write($"{Configuration.RecoveryDrive}\n", true, ConsoleColor.DarkYellow);

            // Partitioning the disk
            Actions.PrepareDisk(
                Configuration.Firmware,
                Configuration.BootLoader,
                ApplyDetailsInstance.DiskIndex,
                ApplyDetailsInstance.IsDriveRemovable,
                Configuration.PartitionStyle,
                ApplyDetailsInstance.UseRecovery,
                Configuration.WindowsDrive,
                Configuration.BootDrive,
                Configuration.RecoveryDrive,
                worker);
            if (Configuration.IsCanceled)
            {
                e.Cancel = true;
                return;
            }
            Debug.WriteLine("Disk Preparation done.");

            // Applying image to Windows Partition
            Actions.ApplyWim(Configuration.WindowsDrive, ApplyDetailsInstance.FileName, ApplyDetailsInstance.Index, worker);
            if (Configuration.IsCanceled)
            {
                e.Cancel = true;
                return;
            }
            Debug.WriteLine("Windows Image applied to partition.");

            // Writing BootLoader to disk
            switch (Configuration.PartitionStyle)
            {
                case PartitionStyle.SeparateBoot:
                case PartitionStyle.Full:
                    Actions.InstallBootloader(
                        Configuration.Firmware,
                        Configuration.BootLoader,
                        Configuration.WindowsDrive,
                        Configuration.BootDrive,
                        worker);
                    break;
                case PartitionStyle.Single:
                    Actions.InstallBootloader(
                        Configuration.Firmware,
                        Configuration.BootLoader,
                        Configuration.WindowsDrive,
                        Configuration.WindowsDrive,
                        worker);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (Configuration.IsCanceled)
            {
                e.Cancel = true;
                return;
            }
            Debug.WriteLine("Bootloader written to disk.");


            // Registering Recovery Image (only for Vista and higher)
            if (Configuration.BootLoader == Bootloader.BOOTMGR && ApplyDetailsInstance.UseRecovery)
            {
                Actions.InstallRecovery(
                    $"{Configuration.WindowsDrive}Windows",
                    Configuration.RecoveryDrive,
                    DeploymentOption.AddDiveToWinRE,
                    worker);

                if (Configuration.IsCanceled)
                {
                    e.Cancel = true;
                    return;
                }
                Debug.WriteLine("Recovery Image registered.");
            }

            // Install unattended file (only for Vista and higher)
            if (DeploymentInfoInstance.UseUserInfo || OemInfo.UseOemInfo)
            {
                Debug.WriteLine("Building config...");
                var config = File.Exists(DeploymentInfoInstance.CustomFilePath) ? File.ReadAllText(DeploymentInfoInstance.CustomFilePath) : Common.UnattendBuilder.Build();

                if (string.IsNullOrEmpty(config))
                    throw new Exception("Could not build or read unattended configuration file.");

                Actions.InstallUnattend($"{Configuration.WindowsDrive}Windows", config, OemInfo.LogoPath, DeploymentOption.UseSMode, worker);
                Debug.Write("Configuration file written to ");
                Debug.Write($"{Configuration.WindowsDrive}Windows\\Panther\\unattend.xml\n", true, ConsoleColor.DarkYellow);

                if (Configuration.IsCanceled)
                {
                    Debug.WriteLine("Cancelling process...");
                    e.Cancel = true;
                    return;
                }
            }
            else
            {
                Debug.WriteLine("No OEM Information entered. Skipping ...");
            }

            // Install Drivers (only for Vista and higher)
            if (ApplyDetailsInstance.DriverList != null && ApplyDetailsInstance.DriverList.Count > 0)
            {
                Configuration.DriverCount = ApplyDetailsInstance.DriverList.Count;
                Actions.InstallDriver(Configuration.WindowsDrive, ApplyDetailsInstance.DriverList, worker);

                if (Configuration.IsCanceled)
                {
                    e.Cancel = true;
                    return;
                }
            }
            else
            {
                Debug.WriteLine("No Drivers selected. Skipping ...");
            }

            // Install UefiSeven (only for Vista and 7 with EFI)
            if (WindowsModification.InstallUefiSeven)
            {
                Core.Action.UefiSeven.InstallUefiSeven(
                    Configuration.BootDrive,
                    WindowsModification.UsToggleSkipErros,
                    WindowsModification.UsToggleFakeVesa,
                    WindowsModification.UsToggleVerbose,
                    WindowsModification.UsToggleLog,
                    worker);

                if (Configuration.IsCanceled)
                {
                    e.Cancel = true;
                    return;
                }
            }
            else
            {
                Debug.WriteLine("UefiSeven disabled or not necessary. Skipping ...");
            }

            // Installation complete
            worker.ReportProgress(100, JsonConvert.SerializeObject(new ActionWorker
            {
                IsDebug = true,
                Message = "Job Done."
            }));
        }

        /// <summary>
        /// Initialize the Working Environment. Defines the Firmware, BootLoader and Drive letters.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private static void InitializeWorkingEnvironment()
        {
            // BootLoader and Firmware Definition
            Configuration.Firmware = ApplyDetailsInstance.UseEFI switch
            {
                true => Firmware.EFI,
                false => Firmware.BIOS,
            };
            Configuration.BootLoader = ApplyDetailsInstance.UseNTLDR switch
            {
                true => Bootloader.NTLDR,
                false => Bootloader.BOOTMGR,
            };

            // Partition Definition
            char[] letters;

            // Letters definition:
            // [0] = Windows drive
            // [1] = Boot drive
            // [2] = Recovery
            switch (ApplyDetailsInstance.UseNTLDR)
            {
                // pre-Vista
                case true:
                    letters = Actions.GetSystemLetters(PartitionStyle.Single);
                    Configuration.PartitionStyle = PartitionStyle.Single;
                    break;

                case false:
                    switch (ApplyDetailsInstance.UseRecovery)
                    {
                        case true:
                            letters = Actions.GetSystemLetters(PartitionStyle.Full);
                            Configuration.PartitionStyle = PartitionStyle.Full;
                            break;

                        case false:
                            // If Vista is used, we need to use the Single partition layout
                            if (ApplyDetailsInstance.NTVersion == "6.0") // Windows Vista NT Version
                            {
                                // Except if EFI is used, then we need to use the SeparateBoot partition layout
                                if (ApplyDetailsInstance.UseEFI)
                                {
                                    letters = Actions.GetSystemLetters(PartitionStyle.SeparateBoot);
                                    Configuration.PartitionStyle = PartitionStyle.SeparateBoot;
                                }
                                else
                                {
                                    letters = Actions.GetSystemLetters(PartitionStyle.Single);
                                    Configuration.PartitionStyle = PartitionStyle.Single;
                                }
                            }
                            else
                            {
                                letters = Actions.GetSystemLetters(PartitionStyle.SeparateBoot);
                                Configuration.PartitionStyle = PartitionStyle.SeparateBoot;
                            }
                            break;
                    }
                    break;
            }

            switch (Configuration.PartitionStyle)
            {
                case PartitionStyle.Single:
                    Configuration.WindowsDrive = $"{letters[0]}:\\";
                    break;
                case PartitionStyle.SeparateBoot:
                    Configuration.WindowsDrive = $"{letters[0]}:\\";
                    Configuration.BootDrive = $"{letters[1]}:\\";
                    break;
                case PartitionStyle.Full:
                    Configuration.WindowsDrive = $"{letters[0]}:\\";
                    Configuration.BootDrive = $"{letters[1]}:\\";
                    Configuration.RecoveryDrive = $"{letters[2]}:\\";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
