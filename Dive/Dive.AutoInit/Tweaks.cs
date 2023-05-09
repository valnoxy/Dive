﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dive.AutoInit.Resources;

namespace Dive.AutoInit
{
    public class Tweaks
    {
        public enum MemoryDumpOption:int
        {
            None = 0,
            Complete = 1,
            Kernel = 2,
            Small = 3,
            Automatic = 7
        }

        public static bool DisableAutoRebootOnBSOD()
        {
            return System.Registry.SetKey("SYSTEM\\CurrentControlSet\\Control\\CrashControl", "AutoReboot", "0");
        }

        public static bool SetMaxMemDump(MemoryDumpOption memoryDumpOption)
        {
            var status = true;
            var a = System.Processes.RunProcess("wmic.exe", $"recoveros set DebugInfoType = {(int)memoryDumpOption}");
            if (a != 0) status = false;

            return status;
        }

        public static bool ShadowStorage(int size)
        {
            var status = true;
            var a = System.Processes.RunProcess("vssadmin.exe", $"resize shadowstorage /for=C: /on=C: /maxsize={size}%");
            if (a != 0) status = false;
            
            return status;
        }

        public static bool SetMaxPerformance()
        {
            var status = true;
            var a = System.Processes.RunProcess("powercfg.exe", "/setactive 8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c");
            if (a != 0) status = false;
            return status;
        }

        public static bool DisableFastBoot()
        {
            var status = true;
            var a = System.Processes.RunProcess("powercfg.exe", "/hibernate off");
            if (a != 0) status = false;
            return status;
        }

        public static void CleanupStartPins()
        {
            var build = Environment.OSVersion.Version.Build;
            if (build >= 22000) // Windows 11+
            {
                var root = Path.Combine(Path.GetPathRoot(Environment.SystemDirectory), "Users");
                var subdirectoriesEntries = Directory.GetDirectories(root);
                foreach (var subdirectory in subdirectoriesEntries)
                {
                    var startmenu = Path.Combine(subdirectory,
                        "AppData", "Local", "Packages", "Microsoft.Windows.StartMenuExperienceHost_cw5n1h2txyewy", "LocalState");
                    if (!Directory.Exists(startmenu)) continue;
                    try
                    {
                        File.WriteAllBytes(Path.Combine(startmenu, "start2.bin"), StartMenu.start2_W11);
                    }
                    catch (Exception ex)
                    {
                        // log error
                    }
                }
            }
        }
    }
}
