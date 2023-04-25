using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dive.Core.Common
{
    public class ActionWorker
    {
        public Progress Action { get; set; }
        public bool IsError { get; set; }
        public bool IsWarning { get; set; }
        public string Message { get; set; }
        public bool IsIndeterminate { get; set; }
        public bool IsDebug { get; set; }
    }

    public enum Progress
    {
        PrepareDisk,
        ApplyImage,
        InstallBootloader,
        InstallRecovery,
        InstallUnattend,
        InstallDrivers,
        InstallUefiSeven,
        CaptureDisk
    }
}
