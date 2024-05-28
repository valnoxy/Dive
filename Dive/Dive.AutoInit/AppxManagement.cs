using System.Diagnostics;
using System.Net;

namespace Dive.AutoInit
{
    public class AppxManagement
    {
        /// <summary>
        /// Removes the App from the system.
        /// </summary>
        /// <remarks>
        /// Return code: 0 = success, 1 = failure
        /// </remarks>
        /// <param name="appId">Application ID</param>
        /// <returns>Status code</returns>
        public static int RemoveAppx(string appId)
        {
            var psi = new ProcessStartInfo
            {
                UseShellExecute = true,
                CreateNoWindow = false,
                Arguments = $"Get-AppxPackage '{appId}' | Remove-AppxPackage",
                WindowStyle = ProcessWindowStyle.Hidden,
                FileName = "powershell.exe"
            };
            var proc = Process.Start(psi);

            proc?.WaitForExit();
            return proc!.ExitCode;
        }

        /// <summary>
        /// Removes the App from the system.
        /// </summary>
        /// <remarks>
        /// Return code: 0 = success, 1 = failure
        /// </remarks>
        /// <param name="appId">Application ID</param>
        /// <returns>Status code</returns>
        public static int RemoveAppXProvisionedPackage(string appId)
        {
            var psi = new ProcessStartInfo
            {
                UseShellExecute = true,
                CreateNoWindow = false,
                Arguments = $"$App = Get-AppXProvisionedPackage -Online | Where {{$_.DisplayName -eq '{appId}' }}; Remove-AppXProvisionedPackage -Online -PackageName $App.PackageName",
                WindowStyle = ProcessWindowStyle.Hidden,
                FileName = "powershell.exe"
            };
            var proc = Process.Start(psi);

            proc!.WaitForExit();
            return proc.ExitCode;
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
        /// <param name="packageName"></param>
        /// <param name="scope"></param>
        /// <returns>Status code</returns>
        public static int InstallApp(string packageName, string? scope = null)
        {
            var scopeArg = string.Empty;
            if (!string.IsNullOrEmpty(scope))
                scopeArg = $"--scope {scope}";

            var p = new Process();
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.Arguments = $"/c winget install --id {packageName} {scopeArg} --accept-source-agreements --accept-package-agreements";
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
        public static int DownloadFile(string downloadUrl, string saveTo)
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