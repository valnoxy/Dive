using Dive.Core.Common;
using Newtonsoft.Json;
using System.ComponentModel;
using System.IO;

namespace Dive.Core.Action.Deployment
{
    public class CSMWrap
    {
        public static void Install(string bootLetter,  BackgroundWorker worker = null)
        {
            worker?.ReportProgress(0, JsonConvert.SerializeObject(new ActionWorker
            {
                Action = Progress.InstallCSMWrap,
                IsError = false,
                IsIndeterminate = true,
                Message = "Installing CSMWrap ..."
            }));

            try
            {
                var targetPath = Path.Combine(bootLetter, "EFI", "Boot");
                var bootX64 = Assets.CSMWrap.csmwrapx64;
                var bootIA32 = Assets.CSMWrap.csmwrapia32;
                Directory.CreateDirectory(targetPath);
                File.WriteAllBytes(Path.Combine(targetPath, "bootia32.efi"), bootIA32);
                File.WriteAllBytes(Path.Combine(targetPath, "bootx64.efi"), bootX64);
            }
            catch
            {
                worker?.ReportProgress(0, JsonConvert.SerializeObject(new ActionWorker
                {
                    Action = Progress.InstallCSMWrap,
                    IsError = true,
                    IsIndeterminate = false,
                    Message = "Failed to install CSMWrap."
                }));
                return;
            }

            worker?.ReportProgress(100, JsonConvert.SerializeObject(new ActionWorker
            {
                Action = Progress.InstallCSMWrap,
                IsError = false,
                IsIndeterminate = false,
                Message = "Done."
            }));
        }
    }
}
