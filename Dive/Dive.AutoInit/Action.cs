using Dive.AutoInit.Common;
using Dive.PluginContext;

namespace Dive.AutoInit
{
    internal class Action
    {
        public static void PerformTelemetry(Configuration config)
        {
            if (config.Telemetry!.DisableCei)
                if (!Telemetry.DisableCEI())
                    CurrentAction.ErrorCounter++;

            if (config.Telemetry.DisableAit)
                if (!Telemetry.DisableAit())
                    CurrentAction.ErrorCounter++;

            if (config.Telemetry.DisableCeip)
                if (!Telemetry.DisableCeip())
                    CurrentAction.ErrorCounter++;

            if (config.Telemetry.DisableDcp)
                if (!Telemetry.DisableDcp())
                    CurrentAction.ErrorCounter++;

            if (config.Telemetry.DisableLicenseTelemetry)
                if (!Telemetry.DisableLicenseTel())
                    CurrentAction.ErrorCounter++;

            if (config.Telemetry.DisableDeviceCensus)
                if (!Telemetry.DisableDeviceCensus())
                    CurrentAction.ErrorCounter++;
        }

        public static void PerformTweaks(Configuration config)
        {
            if (config.Tweaks!.CleanupStartPins)
                if (!Tweaks.CleanupStartPins())
                    CurrentAction.ErrorCounter++;

            if (config.Tweaks.DisableAutoRebootOnBsod)
                if (!Tweaks.DisableAutoRebootOnBSOD())
                    CurrentAction.ErrorCounter++;

            if (config.Tweaks.SetMaxMemoryDumpSize != null)
                if (!Tweaks.SetMaxMemDump(config.Tweaks.SetMaxMemoryDumpSize))
                    CurrentAction.ErrorCounter++;

            if (config.Tweaks.ShadowStorage != 0)
            {
                if (!Tweaks.ShadowStorage(config.Tweaks.ShadowStorage))
                    CurrentAction.ErrorCounter++;
                if (!Tweaks.EnableSystemProtection())
                    CurrentAction.ErrorCounter++;
            }

            if (config.Tweaks.SetMaxPerformance)
                if (!Tweaks.SetMaxPerformance())
                    CurrentAction.ErrorCounter++;

            if (config.Tweaks.DisableFastBoot)
                if (!Tweaks.DisableFastBoot())
                    CurrentAction.ErrorCounter++;
        }

        public static void PerformRemoval(Configuration config)
        {
            foreach (var app in config.Removal!)
            {
                var statusAppx = AppxManagement.RemoveAppx(app.PackageId!);
                if (statusAppx != 0)
                {
                    DebugConsole.WriteLine($"Failed to remove appx package \"{app.PackageId}\": Exited with code {statusAppx}");
                    CurrentAction.ErrorCounter++;
                }

                var statusProvisioning = AppxManagement.RemoveAppXProvisionedPackage(app.PackageId!);
                if (statusProvisioning != 0)
                {
                    DebugConsole.WriteLine($"Failed to remove provisioned appx package \"{app.PackageId}\": Exited with code {statusProvisioning}");
                    CurrentAction.ErrorCounter++;
                }
            }
        }

        public static void PerformInstallation(Configuration config)
        {
            foreach (var app in config.Applications!)
            {
                if (string.IsNullOrEmpty(app.PathToExe))
                {
                    // Winget app
                    var status = AppxManagement.InstallApp(app.PackageId!, app.Scope);
                    if (status != 0)
                    {
                        DebugConsole.WriteLine($"Failed to install appx package \"{app.Name}\": Exited with code {status}");
                        CurrentAction.ErrorCounter++;
                    }
                }
                else
                {
                    // App from the internet
                    var status = AppxManagement.DownloadFile(app.PackageId!, app.PathToExe);
                    if (status != 0)
                    {
                        DebugConsole.WriteLine($"Failed to install appx package \"{app.Name}\": Exited with code {status}");
                        CurrentAction.ErrorCounter++;
                    }
                }
            }
        }

        public static void Perform(Configuration config)
        {
            DebugConsole.WriteLine("Performing AutoInit actions ...");
            DebugConsole.WriteLine("Configuring Telemetry ...");
            PerformTelemetry(config);

            DebugConsole.WriteLine("Configuring Tweaks ...");
            PerformTweaks(config);

            DebugConsole.WriteLine("Removing unwanted applications ...");
            PerformRemoval(config);

            DebugConsole.WriteLine("Installing desired applications ...");
            PerformInstallation(config);

            DebugConsole.WriteLine("AutoInit actions completed.");
            DebugConsole.WriteLine($"{CurrentAction.ErrorCounter} error(s) occurred.");
        }
    }
}
