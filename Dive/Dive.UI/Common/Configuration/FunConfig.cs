using Kevsoft.WLED;

namespace Dive.UI.Common.Configuration
{
    public class FunConfig
    {
        public static bool WledCommunication { get; set; }
        public static string? WledControllerIp { get; set; }
        public static int? AvailableLEDs { get; set; }
        public static WLedClient WledClient { get; set; }
    }
}
