using System;
using System.Collections.Generic;

namespace Dive.UI.Common
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
        public static string NTVersion { get; set; }
        public static int Build { get; set; }
        public static List<string> DriverList { get; set; }
    }

    public static class DeploymentInfo
    {
        public static bool UseUserInfo { get; set; }
        public static string Username { get; set; }
        public static string Password { get; set; }
        public static string CustomFilePath { get; set; }
    }

    public static class CaptureInfo
    {
        public static string Name { get; set; }
        public static string Description { get; set; }
        public static string PathToCapture { get; set; }
        public static string PathToImage { get; set;}
        public static string ImageFileName { get; set; }
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

    public static class DeploymentOption
    {
        public static bool UseSMode { get; set; }
        public static bool UseCopyProfile { get; set; }
        public static bool AddDiveToWinRE { get; set;}
    }

    public static class Tweaks
    {
        public static TweakMode CurrentMode { get; set; }
        public static int DiskIndex { get; set; }
        public static string ImageName { get; set; }
    }

    public static class WindowsModification
    {
        public static bool InstallUefiSeven { get; set; }
        public static bool UsToggleLog { get; set; }
        public static bool UsToggleVerbose { get; set; }
        public static bool UsToggleFakeVesa { get; set; }
        public static bool UsToggleSkipErros { get; set; }

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
        OnlyOem
    }

    public enum TweakMode
    {
        AutoInit,
        Migrate,
        RepairBootloader
    }
}
