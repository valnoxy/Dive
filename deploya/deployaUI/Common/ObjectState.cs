using System;
using System.Windows.Documents;

namespace deployaUI.Common
{
    public static class ApplyDetails
    {
        public static string Name { get; set; }
        public static string FileName { get; set; }
        public static string IconPath { get; set; }
        public static int Index { get; set; }
        public static int DiskIndex { get; set; }
        public static bool UseEFI { get; set; }
        public static bool UseNTLDR { get; set; }
        public static bool UseRecovery { get; set; }
    }

    public static class DeploymentInfo
    {
        public static bool UseUserInfo { get; set; }
        public static string Username { get; set; }
        public static string Password { get; set; }
        public static string CustomFilePath { get; set; }
    }

    public static class OemInfo
    {
        public static bool UseOemInfo { get; set; }
        public static string LogoPath { get; set; }
        public static string Manufacturer { get; set; }
        public static string Model { get; set; }
        public static string SupportHours { get; set; }
        public static string SupportPhone { get; set; }
        public static string SupportURL { get; set; }
    }

    public enum UnattendMode
    {
        Admin,
        User,
        AdminWithoutOem,
        UserWithoutOem,
        AdminWithoutPassword,
        AdminWithoutPasswordAndOem,
        UserWithoutPassword,
        UserWithoutPasswordAndOem,
    }

    public static class Debug
    {
        public static void WriteLine(string message, ConsoleColor color = ConsoleColor.White)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("[");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("*");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("] ");

            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ResetColor();
        }
    }
}
