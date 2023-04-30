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
        public Telemetry Telemetry { get; set; }
        public Tweaks Tweaks { get; set; }
    }

    public class Application
    {
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
        public MemoryDumpOption SetMaxMemoryDumpSize { get; set; }
        public int ShadowStorage { get; set; } // Ex. 20% of Volume Size
        public bool SetMaxPerformance { get; set; }
        public bool DisableFastBoot { get; set; }
    }

    public enum MemoryDumpOption
    {
        None = 0,
        Complete = 1,
        Kernel = 2,
        Small = 3,
        Automatic = 7
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
                    SetMaxMemoryDumpSize = MemoryDumpOption.Small,
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
                    }
                }
            };
            var json = JsonConvert.SerializeObject(config);
            Console.WriteLine(json);
            return json;
        }
    }
}
