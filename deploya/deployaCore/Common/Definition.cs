namespace deployaCore.Common
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

        public enum UI
        {
            Graphical,
            Command,
        }

        public enum PartitionStyle
        {
            Single,
            SeparateBoot,
            Full
        }
    }
}
