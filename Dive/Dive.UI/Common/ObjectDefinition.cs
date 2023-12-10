using System.Collections.Generic;
using System.ComponentModel;

namespace Dive.UI.Common
{
    public class ApplyDetails : INotifyPropertyChanged
    {
        private static ApplyDetails _instance;
        public static ApplyDetails Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ApplyDetails();
                }
                return _instance;
            }
        }

        private string _name;
        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        private string _fileName;
        public string FileName
        {
            get => _fileName;
            set
            {
                if (_fileName != value)
                {
                    _fileName = value;
                    OnPropertyChanged(nameof(FileName));
                }
            }
        }

        private string _iconPath;
        public string IconPath
        {
            get => _iconPath;
            set
            {
                if (_iconPath != value)
                {
                    _iconPath = value;
                    OnPropertyChanged(nameof(IconPath));
                }
            }
        }

        private int _index;
        public int Index
        {
            get => _index;
            set
            {
                if (_index != value)
                {
                    _index = value;
                    OnPropertyChanged(nameof(Index));
                }
            }
        }

        private int _diskIndex;
        public int DiskIndex
        {
            get => _diskIndex;
            set
            {
                if (_diskIndex != value)
                {
                    _diskIndex = value;
                    OnPropertyChanged(nameof(DiskIndex));
                }
            }
        }

        private bool _isDriveRemovable;
        public bool IsDriveRemovable
        {
            get => _isDriveRemovable;
            set
            {
                if (_isDriveRemovable != value)
                {
                    _isDriveRemovable = value;
                    OnPropertyChanged(nameof(IsDriveRemovable));
                }
            }
        }

        private bool _useEFI;
        public bool UseEFI
        {
            get => _useEFI;
            set
            {
                if (_useEFI != value)
                {
                    _useEFI = value;
                    OnPropertyChanged(nameof(UseEFI));
                }
            }
        }

        private bool _useNTLDR;
        public bool UseNTLDR
        {
            get => _useNTLDR;
            set
            {
                if (_useNTLDR != value)
                {
                    _useNTLDR = value;
                    OnPropertyChanged(nameof(UseNTLDR));
                }
            }
        }

        private bool _useRecovery;
        public bool UseRecovery
        {
            get => _useRecovery;
            set
            {
                if (_useRecovery != value)
                {
                    _useRecovery = value;
                    OnPropertyChanged(nameof(UseRecovery));
                }
            }
        }

        private string _ntVersion;
        public string NTVersion
        {
            get => _ntVersion;
            set
            {
                if (_ntVersion != value)
                {
                    _ntVersion = value;
                    OnPropertyChanged(nameof(NTVersion));
                }
            }
        }

        private int _build;
        public int Build
        {
            get => _build;
            set
            {
                if (_build != value)
                {
                    _build = value;
                    OnPropertyChanged(nameof(Build));
                }
            }
        }

        private List<string> _driverList;
        public List<string> DriverList
        {
            get => _driverList;
            set
            {
                if (_driverList != value)
                {
                    _driverList = value;
                    OnPropertyChanged(nameof(DriverList));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class DeploymentInfo : INotifyPropertyChanged
    {
        private static DeploymentInfo _instance;
        public static DeploymentInfo Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new DeploymentInfo();
                }
                return _instance;
            }
        }

        private bool _useUserInfo;
        public bool UseUserInfo
        {
            get => _useUserInfo;
            set
            {
                if (_useUserInfo != value)
                {
                    _useUserInfo = value;
                    OnPropertyChanged(nameof(UseUserInfo));
                }
            }
        }

        private string _username;
        public string Username
        {
            get => _username;
            set
            {
                if (_username != value)
                {
                    _username = value;
                    OnPropertyChanged(nameof(Username));
                }
            }
        }

        private string _password;
        public string Password
        {
            get => _password;
            set
            {
                if (_password != value)
                {
                    _password = value;
                    OnPropertyChanged(nameof(Password));
                }
            }
        }

        private string _customFilePath;
        public string CustomFilePath
        {
            get => _customFilePath;
            set
            {
                if (_customFilePath != value)
                {
                    _customFilePath = value;
                    OnPropertyChanged(nameof(CustomFilePath));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public static class CloudDetails
    {
        public static string Hostname { get; set; }
        public static string Username { get; set; }
        public static string Password { get; set; }
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

    public static class DeviceInfo
    {
        public static bool UseDeviceInfo { get; set; }
        public static string DeviceName { get; set; }
        public static string ProductKey { get; set; }
        public static string RegisteredOwner { get; set; }
        public static string RegisteredOrganization { get; set; }
        public static string TimeZone { get; set; }
        public static int TimeZoneId { get; set; }
    }

    public static class DomainInfo
    {
        public static bool UseDomainInfo { get; set; }
        public static string UserName { get; set; }
        public static string Password { get; set; }
        public static string Domain { get; set; }
    }

    public static class OutOfBoxExperienceInfo
    {
        public static bool UseOOBEInfo { get; set; }
        public static bool HideEULAPage { get; set; }
        public static bool HideOEMRegistrationScreen { get; set; }
        public static bool HideOnlineAccountScreens { get; set; }
        public static bool HideWirelessSetupInOOBE { get; set; }
        public static string NetworkLocation { get; set; }
        public static bool SkipMachineOOBE { get; set; }
        public static bool SkipUserOOBE { get; set; }
        public static bool HideLocalAccountScreen { get; set; }
    }

    public static class DeploymentOption
    {
        public static bool UseSMode { get; set; }
        public static bool UseCopyProfile { get; set; }
        public static bool AddDiveToWinRE { get; set; }
    }

    public static class Tweaks
    {
        public static TweakMode CurrentMode { get; set; }
        public static int DiskIndex { get; set; }
    }

    public static class WindowsModification
    {
        public static bool InstallUefiSeven { get; set; }
        public static bool UsToggleLog { get; set; }
        public static bool UsToggleVerbose { get; set; }
        public static bool UsToggleFakeVesa { get; set; }
        public static bool UsToggleSkipErros { get; set; }
    }
    
    public enum TweakMode
    {
        AutoInit,
        Migrate,
        RepairBootloader
    }
}
