namespace Dive.Core.Common
{
    public class Entities
    {
        public enum Firmware
        {
            BIOS,
            EFI,
        }

        public enum Bootloader
        {
            BOOTMGR,
            NTLDR,
        }

        public enum PartitionStyle
        {
            Single,
            SeparateBoot,
            Full,
            LegacyEfi,
            LegacyEfiWithRecovery
        }

        public class CanarySettings
        {
            public static readonly bool UseNewDiskOperation = true;
        }
    }
}
