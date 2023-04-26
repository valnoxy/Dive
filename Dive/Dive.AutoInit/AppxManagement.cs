using System.Diagnostics;
using System.Net;

namespace Dive.AutoInit
{
    public class AppxManagement
    {
        // public list of apps to remove
        public class App
        {
            public string Name { get; set; }
            public string ID { get; set; }
        }
        public static List<App> apps;

        /// <summary>
        /// Removes the App from the system.
        /// </summary>
        /// <remarks>
        /// Return code: 0 = success, 1 = failure
        /// </remarks>
        /// <param name="appID">Application ID</param>
        /// <returns>Status code</returns>
        public static int RemoveAppx(string appID)
        {
            var psi = new ProcessStartInfo
            {
                UseShellExecute = true,
                CreateNoWindow = false,
                Arguments = $"Get-AppxPackage '{appID}' | Remove-AppxPackage",
                WindowStyle = ProcessWindowStyle.Hidden,
                FileName = "powershell.exe"
            };
            var proc = Process.Start(psi);

            proc?.WaitForExit();
            return proc!.ExitCode;
        }

        /// <summary>
        /// Checks if WinGet is installed on the system.
        /// </summary>
        /// <returns>Summary as bool</returns>
        public static bool IsWinGetInstalled()
        {
            var p = new Process();
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.Arguments = "/c winget";
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;
            p.Start();
            p.WaitForExit();

            return p.ExitCode == 0;
        }

        /// <summary>
        /// Installs the app to the system via WinGet.
        /// </summary>
        /// <param name="PackageName"></param>
        /// <returns>Status code</returns>
        public static int InstallApp(string PackageName)
        {
            var p = new Process();
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.Arguments = $"/c winget install --id {PackageName} --accept-source-agreements --accept-package-agreements";
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;
            p.Start();
            p.WaitForExit();

            return p.ExitCode;
        }

        /// <summary>
        /// Download the desired file from the internet.
        /// </summary>
        /// <param name="downloadUrl"></param>
        /// <param name="saveTo"></param>
        /// <returns>Status code</returns>
        public static int InstallRemoteManagement(string downloadUrl, string saveTo)
        {
            try
            {
                WebClient rms = new();
                rms.DownloadFile(downloadUrl, saveTo);
                return 0;
            }
            catch
            {
                return 1;
            }
        }

        /// <summary>
        /// Installs the desired feature on the system.
        /// </summary>
        /// <param name="featureName"></param>
        /// <returns></returns>
        public static int InstallFeature(string featureName)
        {
            var p = new Process();
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.Arguments = $"/c \"dism /Online /Enable-Feature /All /FeatureName:{featureName} /NoRestart\"";
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;
            p.Start();
            p.WaitForExit();

            return p.ExitCode;
        }
    }
}