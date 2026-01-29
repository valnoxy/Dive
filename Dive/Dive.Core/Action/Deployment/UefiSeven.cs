using Dive.Core.Assets;
using Dive.Core.Common;
using System.ComponentModel;
using System.IO;

namespace Dive.Core.Action.Deployment
{
    public class UefiSeven
    {
        /// <summary>
        /// Install UefiSeven on the target system
        /// </summary>
        /// <param name="bootLetter">Drive letter of the recovery partition</param>
        /// <param name="logToFile">Log to UefiSeven.log file</param>
        /// <param name="worker">Background worker for Graphical user interface</param>
        /// <param name="skipErrors">Skip warnings and prompts</param>
        /// <param name="fakeVesaForce">Overwrite Int10h handler with fakevesa even when the native handler is present</param>
        /// <param name="verboseBoot">Enable verbose mode</param>
        public static void InstallUefiSeven(string bootLetter, bool skipErrors, bool fakeVesaForce, bool verboseBoot, bool logToFile, BackgroundWorker worker = null)
        {
            worker?.ReportProgress(0, new ActionWorker
            {
                Action = Progress.InstallUefiSeven,
                IsError = false,
                IsIndeterminate = true,
                Message = "Installing UefiSeven ..."
            });

            // Find bootmgfw.efi and rename it to bootmgfw.original.efi
            try
            {
                var bootmgfwBinary = Path.Combine(bootLetter, "EFI", "Microsoft", "Boot", "bootmgfw.efi");
                var bootmgfwBinaryOriginal = Path.Combine(bootLetter, "EFI", "Microsoft", "Boot", "bootmgfw.original.efi");
                var uefiSevenConfig = Path.Combine(bootLetter, "EFI", "Microsoft", "Boot", "UefiSeven.ini");

                if (File.Exists(bootmgfwBinary))
                {
                    File.Move(bootmgfwBinary, bootmgfwBinaryOriginal, true);

                    // Copy UefiSeven to bootmgfw.efi
                    try
                    {
                        var uefiSevenBinary = UefiSevenResources.bootx64;
                        File.WriteAllBytes(bootmgfwBinary, uefiSevenBinary);

                        // Create UefiSeven.ini
                        var config = new IniFile(uefiSevenConfig);
                        config.Write("skiperrors", skipErrors ? "1" : "0", "config");
                        config.Write("force_fakevesa", fakeVesaForce ? "1" : "0", "config");
                        config.Write("verbose", verboseBoot ? "1" : "0", "config");
                        config.Write("logfile", logToFile ? "1" : "0", "config");
                    }
                    catch
                    {
                        worker?.ReportProgress(0, new ActionWorker
                        {
                            Action = Progress.InstallUefiSeven,
                            IsError = true,
                            IsIndeterminate = false,
                            Message = "Failed to install UefiSeven."
                        });
                        return;
                    }
                }
                else
                {
                    worker?.ReportProgress(0, new ActionWorker
                    {
                        Action = Progress.InstallUefiSeven,
                        IsError = true,
                        IsIndeterminate = false,
                        Message = "File 'bootmgfw.efi' is missing."
                    });
                    return;
                }
            }
            catch
            {
                worker?.ReportProgress(0, new ActionWorker
                {
                    Action = Progress.InstallUefiSeven,
                    IsError = true,
                    IsIndeterminate = false,
                    Message = "Cannot create a copy of the EFI bootloader."
                });
            }

            worker?.ReportProgress(100, new ActionWorker
            {
                Action = Progress.InstallUefiSeven,
                IsError = false,
                IsIndeterminate = false,
                Message = "Done."
            });
        }
    }
}
