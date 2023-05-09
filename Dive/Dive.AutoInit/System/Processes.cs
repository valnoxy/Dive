using System.Diagnostics;

namespace Dive.AutoInit.System
{
    internal class Processes
    {
        internal static int RunProcess(string program, string argument)
        {
            var psi = new ProcessStartInfo
            {
                UseShellExecute = true,
                CreateNoWindow = false,
                Arguments = argument,
                WindowStyle = ProcessWindowStyle.Hidden,
                FileName = program
            };
            var proc = Process.Start(psi);

            proc!.WaitForExit();
            return proc.ExitCode;
        }
    }
}
