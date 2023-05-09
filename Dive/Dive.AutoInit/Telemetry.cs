/*
 * Tweaks from privacy.sexy
 */

namespace Dive.AutoInit
{
    public class Telemetry
    {
        /// <summary>
        /// Disable Customer Experience Improvement (CEIP/SQM)
        /// </summary>
        /// <returns>Status of action</returns>
        public static bool DisableCEI()
        {
            return System.Registry.SetKey("Software\\Policies\\Microsoft\\SQMClient\\Windows", "CEIPEnable", "0");
        }

        /// <summary>
        /// Disable Application Impact Telemetry (AIT)
        /// </summary>
        /// <returns>Status of action</returns>
        public static bool DisableAit()
        {
            return System.Registry.SetKey("Software\\Policies\\Microsoft\\Windows\\AppCompat", "AITEnable", "0");
        }

        /// <summary>
        /// Disable Customer Experience Improvement Program
        /// </summary>
        /// <returns>Status of action</returns>
        public static bool DisableCeip()
        {
            var ceip = true;
            var a = System.Processes.RunProcess("schtasks.exe", "/change /TN \"\\Microsoft\\Windows\\Customer Experience Improvement Program\\Consolidator\" /DISABLE");
            if (a != 0) ceip = false;

            var b = System.Processes.RunProcess("schtasks.exe", "/change /TN \"\\Microsoft\\Windows\\Customer Experience Improvement Program\\KernelCeipTask\" /DISABLE");
            if (b != 0) ceip = false;

            var c = System.Processes.RunProcess("schtasks.exe", "/change /TN \"\\Microsoft\\Windows\\Customer Experience Improvement Program\\UsbCeip\" /DISABLE");
            if (c != 0) ceip = false;

            return ceip;
        }

        /// <summary>
        /// Disable telemetry in data collection policy
        /// </summary>
        /// <returns>Status of action</returns>
        public static bool DisableDcp()
        {
            var dcp = true;
            var a = System.Registry.SetKey("SOFTWARE\\Wow6432Node\\Microsoft\\Windows\\CurrentVersion\\Policies\\DataCollection", "AllowTelemetry", "0");
            if (a == false) dcp = false;

            var b = System.Registry.SetKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Policies\\DataCollection", "AllowTelemetry", "0");
            if (b == false) dcp = false;

            var c = System.Registry.SetKey("SOFTWARE\\Policies\\Microsoft\\Windows\\DataCollection", "AllowTelemetry", "0");
            if (c == false) dcp = false;

            var d = System.Registry.SetKey("SOFTWARE\\Policies\\Microsoft\\Windows\\DataCollection", "LimitEnhancedDiagnosticDataWindowsAnalytics", "0");
            if (d == false) dcp = false;

            var e = System.Registry.SetKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Policies\\DataCollection", "AllowTelemetry", "0");
            if (e == false) dcp = false;

            return dcp;
        }

        /// <summary>
        /// Disable license telemetry
        /// </summary>
        /// <returns>Status of action</returns>
        public static bool DisableLicenseTel()
        {
            return System.Registry.SetKey("Software\\Policies\\Microsoft\\Windows NT\\CurrentVersion\\Software Protection Platform", "NoGenTicket", "1");
        }

        /// <summary>
        /// Disable devicecensus.exe
        /// </summary>
        /// <returns>Status of action</returns>
        public static bool DisableDeviceCensus()
        {
            var status = true;
            var a = System.Processes.RunProcess("schtasks.exe", "/change /TN \"Microsoft\\Windows\\Device Information\\Device\" /disable");
            if (a != 0) status = false;

            return status;
        }
    }
}
