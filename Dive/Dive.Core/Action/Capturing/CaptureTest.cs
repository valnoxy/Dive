using System;
using System.IO;

namespace Dive.Core.Action.Capturing
{
    public class CaptureTest
    {
        public static void TestBuildInfo()
        {
            var pathToKernel = Path.Combine("C:", "Windows", "System32", "ntoskrnl.exe");
            var a = Capture.BuildImageInfo("kaka", "arsch", pathToKernel);
            Console.WriteLine(a);
        }
    }
}
