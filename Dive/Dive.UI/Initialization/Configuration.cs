using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Dive.UI.Initialization
{
    public class Configuration
    {
        public string Name { get; set; }
        public double Version { get; set; }
        public string Description { get; set; }
        public string Author { get; set; }
        public List<Application> Applications { get; set; }
        public List<Removal> Removal { get; set; }
        public Telemetry Telemetry { get; set; }
        public Tweaks Tweaks { get; set; }
    }

    public class Application
    {
        public string PackageId { get; set; }
        public string PathToExe { get; set; }
    }

    public class Removal
    {
        public string Name { get; set; }
        public string PackageId { get; set; }
    }

    public class Telemetry
    {
        public bool DisableCei { get; set; }
        public bool DisableCeip { get; set; }
        public bool DisableAit { get; set; }
        public bool DisableDcp { get; set; }
        public bool DisableLicenseTelemetry { get; set; }
        public bool DisableDeviceCensus { get; set; }
    }

    public class Tweaks
    {
        public bool CleanupStartPins { get; set; }
        public bool DisableAutoRebootOnBsod { get; set; }
        public AutoInit.Tweaks.MemoryDumpOption SetMaxMemoryDumpSize { get; set; }
        public int ShadowStorage { get; set; } // Ex. 20% of Volume Size
        public bool SetMaxPerformance { get; set; }
        public bool DisableFastBoot { get; set; }
    }

    public class CurrentAction
    {
        public static int ErrorCounter = 0;
    }

    internal class ConfigurationLoader
    {
        public static Configuration? LoadConfiguration(string jsonConfig)
        {
            try
            {
                var config = JsonConvert.DeserializeObject<Configuration>(jsonConfig);
                return config ?? null;
            }
            catch
            {
                return null;
            }
        }

        public static string CreateConfiguration()
        {
            var config = new Configuration
            {
                Name = "Exploitox Default Configuration",
                Version = 1.0,
                Description = "Default configuration for Exploitox",
                Author = "Exploitox",
                Telemetry = new Telemetry
                {
                    DisableAit = true,
                    DisableCei = true,
                    DisableCeip = true,
                    DisableDcp = true,
                    DisableDeviceCensus = true,
                    DisableLicenseTelemetry = true
                },
                Tweaks = new Tweaks
                {
                    CleanupStartPins = true,
                    DisableAutoRebootOnBsod = true,
                    SetMaxMemoryDumpSize = AutoInit.Tweaks.MemoryDumpOption.Small,
                    ShadowStorage = 20,
                    SetMaxPerformance = true,
                    DisableFastBoot = true
                },
                Applications = new List<Application>
                {
                    new Application
                    {
                        PackageId = "Mozilla.Firefox"
                    },
                    new Application
                    {
                        PackageId = "Adobe.Acrobat.Reader.64-bit"
                    },
                    new Application
                    {
                        PackageId = "https://www.wolkenhof.com/download/Fernwartung_Wolkenhof.exe",
                        PathToExe = "C:\\Users\\Public\\Desktop\\Fernwartung Wolkenhof.exe"
                    }
                },
                Removal = new List<Removal>
                {
                    new Removal { Name = "3D Builder", PackageId = "Microsoft.3DBuilder" },
                    new Removal { Name = "3D Viewer", PackageId = "Microsoft.Microsoft3DViewer" },
                    new Removal { Name = "App Connector", PackageId = "Microsoft.Appconnector" },
                    new Removal { Name = "Candy Crush Saga", PackageId = "king.com.CandyCrushSaga" },
                    new Removal { Name = "Candy Crush Soda Saga", PackageId = "king.com.CandyCrushSodaSaga" },
                    new Removal { Name = "Code Writer", PackageId = "ActiproSoftwareLLC.562882FEEB491" },
                    new Removal { Name = "Communications - Phone app", PackageId = "Microsoft.CommsPhone" },
                    new Removal { Name = "Cortana", PackageId = "Microsoft.549981C3F5F10" },
                    new Removal { Name = "Duolingo", PackageId = "D5EA27B7.Duolingo-LearnLanguagesforFree" },
                    new Removal { Name = "Eclipse Manager", PackageId = "46928bounde.EclipseManager" },
                    new Removal { Name = "Flipboard", PackageId = "Flipboard.Flipboard" },
                    new Removal { Name = "Get Help", PackageId = "Microsoft.GetHelp" },
                    new Removal { Name = "GroupMe", PackageId = "Microsoft.GroupMe10" },
                    new Removal { Name = "iHeartRadio", PackageId = "ClearChannelRadioDigital.iHeartRadio" },
                    new Removal { Name = "Messaging", PackageId = "Microsoft.Messaging" },
                    new Removal { Name = "Microsoft Office OneNote", PackageId = "Microsoft.Office.OneNote" },
                    new Removal { Name = "Microsoft Solitaire Collection", PackageId = "Microsoft.MicrosoftSolitaireCollection" },
                    new Removal { Name = "Minecraft for Windows 10 Edition", PackageId = "Microsoft.MinecraftUWP" },
                    new Removal { Name = "Mixed Reality Portal", PackageId = "Microsoft.MixedReality.Portal" },
                    new Removal { Name = "Mobile Plans", PackageId = "Microsoft.OneConnect" },
                    new Removal { Name = "MSN Finance", PackageId = "Microsoft.BingFinance" },
                    new Removal { Name = "MSN News", PackageId = "Microsoft.BingNews" },
                    new Removal { Name = "MSN Sports", PackageId = "Microsoft.BingSports" },
                    new Removal { Name = "MSN Weather", PackageId = "Microsoft.BingWeather" },
                    new Removal { Name = "My Office", PackageId = "Microsoft.MicrosoftOfficeHub" },
                    new Removal { Name = "Network Speedtest", PackageId = "Microsoft.NetworkSpeedTest" },
                    new Removal { Name = "Pandora", PackageId = "PandoraMediaInc.29680B314EFC2" },
                    new Removal { Name = "People", PackageId = "Microsoft.People" },
                    new Removal { Name = "Photoshop Express", PackageId = "AdobeSystemIncorporated.AdobePhotoshop" },
                    new Removal { Name = "Print3D", PackageId = "Microsoft.Print3D" },
                    new Removal { Name = "Remote Desktop app", PackageId = "Microsoft.RemoteDesktop" },
                    new Removal { Name = "Shazam", PackageId = "ShazamEntertainmentLtd.Shazam" },
                    new Removal { Name = "Spotify", PackageId = "SpotifyAB.SpotifyMusic" },
                    new Removal { Name = "Sticky Notes", PackageId = "Microsoft.MicrosoftStickyNotes" },
                    new Removal { Name = "Sway", PackageId = "Microsoft.Office.Sway" },
                    new Removal { Name = "Tips app", PackageId = "Microsoft.Getstarted" },
                    new Removal { Name = "To Do app", PackageId = "Microsoft.Todos" },
                    new Removal { Name = "Twitter", PackageId = "9E2F88E3.Twitter" },
                    new Removal { Name = "Voice Recorder", PackageId = "Microsoft.WindowsSoundRecorder" },
                    new Removal { Name = "Windows Feedback Hub", PackageId = "Microsoft.WindowsFeedbackHub" },
                    new Removal { Name = "Windows Alarms", PackageId = "Microsoft.WindowsAlarms" },
                    new Removal { Name = "Windows Camera", PackageId = "Microsoft.WindowsCamera" },
                    new Removal { Name = "Windows Maps", PackageId = "Microsoft.WindowsMaps" },
                    new Removal { Name = "Xbox App", PackageId = "Microsoft.XboxApp" },
                    new Removal { Name = "Xbox Live in-game experience", PackageId = "Microsoft.Xbox.TCUI" },
                    new Removal { Name = "Xbox Game Bar", PackageId = "Microsoft.XboxGamingOverlay" },
                    new Removal { Name = "Xbox Game Bar Plugin", PackageId = "Microsoft.XboxGameOverlay" },
                    new Removal { Name = "Xbox Identity Provider", PackageId = "Microsoft.XboxIdentityProvider" },
                    new Removal { Name = "Xbox Speech to Text Overlay", PackageId = "Microsoft.XboxSpeechToTextOverlay" },
                    new Removal { Name = "Your Phone Companion #1", PackageId = "Microsoft.WindowsPhone" },
                    new Removal { Name = "Your Phone Companion #2", PackageId = "Microsoft.Windows.Phon" },
                    new Removal { Name = "Your Phone", PackageId = "Microsoft.YourPhone" }
                }
            };
            var json = JsonConvert.SerializeObject(config);
            Console.WriteLine(json);
            return json;
        }
    }
}
