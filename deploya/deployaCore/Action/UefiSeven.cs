using System;
using System.ComponentModel;
using System.IO;
using deployaCore.Assets;
using deployaCore.Common;

namespace deployaCore.Action
{
    public class UefiSeven
    {
        /// <summary>
        /// Install UefiSeven on the target system
        /// </summary>
        /// <param name="ui">User Interface type</param>
        /// <param name="bootLetter">Drive letter of the recovery partition</param>
        /// <param name="logToFile">Log to UefiSeven.log file</param>
        /// <param name="worker">Background worker for Graphical user interface</param>
        /// <param name="skipErrors">Skip warnings and prompts</param>
        /// <param name="fakeVesaForce">Overwrite Int10h handler with fakevesa even when the native handler is present</param>
        /// <param name="verboseBoot">Enable verbose mode</param>
        public static void InstallUefiSeven(Entities.UI ui, string bootLetter, bool skipErrors, bool fakeVesaForce, bool verboseBoot, bool logToFile, BackgroundWorker worker = null)
        {
            Output.Write("Installing UefiSeven ...      ");
            ConsoleUtility.WriteProgressBar(0);
            if (ui == Entities.UI.Graphical) { worker.ReportProgress(102, ""); worker.ReportProgress(0, ""); }

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
                        var config = new deployaCore.Common.IniFile(uefiSevenConfig);
                        config.Write("skiperrors", skipErrors ? "1" : "0", "config");
                        config.Write("force_fakevesa", fakeVesaForce ? "1" : "0", "config");
                        config.Write("verbose", verboseBoot ? "1" : "0", "config");
                        config.Write("logfile", logToFile ? "1" : "0", "config");
                    }
                    catch
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("");
                        Console.WriteLine("   An Error has occurred.");
                        Console.WriteLine("   Error: Cannot extract UefiSeven to disk.");
                        if (ui == Entities.UI.Command)
                            Console.WriteLine(); // Only write new line if ui mode is disabled, so that the ui can read the error code above.
                        Console.ResetColor();
                        switch (ui)
                        {
                            case Entities.UI.Graphical:
                                worker.ReportProgress(307, "");
                                break;
                            case Entities.UI.Command:
                                Environment.Exit(1);
                                break;
                        }
                    }
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("");
                    Console.WriteLine("   An Error has occurred.");
                    Console.WriteLine("   Error: The file 'bootmgfw.efi' is missing.");
                    if (ui == Entities.UI.Command)
                        Console.WriteLine(); // Only write new line if ui mode is disabled, so that the ui can read the error code above.
                    Console.ResetColor();
                    switch (ui)
                    {
                        case Entities.UI.Graphical:
                            worker.ReportProgress(307, "");
                            break;
                        case Entities.UI.Command:
                            Environment.Exit(1);
                            break;
                    }
                }
            }
            catch
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("");
                Console.WriteLine("   An Error has occurred.");
                Console.WriteLine("   Error: Cannot create a copy of the efi bootloader.");
                if (ui == Entities.UI.Command)
                    Console.WriteLine(); // Only write new line if ui mode is disabled, so that the ui can read the error code above.
                Console.ResetColor();
                switch (ui)
                {
                    case Entities.UI.Graphical:
                        worker.ReportProgress(307, "");
                        break;
                    case Entities.UI.Command:
                        Environment.Exit(1);
                        break;
                }
            }

            ConsoleUtility.WriteProgressBar(100, true);
            Console.WriteLine();
            if (ui == Entities.UI.Graphical) { worker.ReportProgress(101, ""); worker.ReportProgress(100, ""); }
        }
    }
}
