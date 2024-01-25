using System;
using System.Collections.Generic;

namespace Dive.UI.Common
{
    public class SettingChangedEventArgs : EventArgs
    {
        public string PropertyName { get; }
        public object NewValue { get; }

        public SettingChangedEventArgs(string propertyName, object newValue)
        {
            PropertyName = propertyName;
            NewValue = newValue;
        }
    }

    public class ApplyDetails
    {
        private string _name;
        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnSettingChanged(nameof(Name), value);
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
                    OnSettingChanged(nameof(FileName), value);
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
                    OnSettingChanged(nameof(IconPath), value);
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
                    OnSettingChanged(nameof(Index), value);
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
                    OnSettingChanged(nameof(DiskIndex), value);
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
                    OnSettingChanged(nameof(IsDriveRemovable), value);
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
                    OnSettingChanged(nameof(UseEFI), value);
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
                    OnSettingChanged(nameof(UseNTLDR), value);
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
                    OnSettingChanged(nameof(UseRecovery), value);
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
                    OnSettingChanged(nameof(NTVersion), value);
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
                    OnSettingChanged(nameof(Build), value);
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
                    OnSettingChanged(nameof(DriverList), value);
                }
            }
        }

        // Event Handler
        public event EventHandler<SettingChangedEventArgs> SettingChanged;
        private void OnSettingChanged(string propertyName, object value)
        {
            SettingChanged?.Invoke(this, new SettingChangedEventArgs(propertyName, value));
        }

        // Singleton
        private static ApplyDetails? _instance;
        public static ApplyDetails Instance => _instance ??= new ApplyDetails();
    }

    public class DeploymentInfo
    {
        private bool _useUserInfo;
        public bool UseUserInfo
        {
            get => _useUserInfo;
            set
            {
                if (_useUserInfo != value)
                {
                    _useUserInfo = value;
                    OnSettingChanged(nameof(UseUserInfo), value);
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
                    OnSettingChanged(nameof(Username), value);
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
                    OnSettingChanged(nameof(Password), value);
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
                    OnSettingChanged(nameof(CustomFilePath), value);
                }
            }
        }

        // Event Handler
        public event EventHandler<SettingChangedEventArgs> SettingChanged;
        private void OnSettingChanged(string propertyName, object value)
        {
            SettingChanged?.Invoke(this, new SettingChangedEventArgs(propertyName, value));
        }

        // Singleton
        private static DeploymentInfo? _instance;
        public static DeploymentInfo Instance => _instance ??= new DeploymentInfo();
    }

    public class OemInfo
    {
        private bool _useOemInfo;
        public bool UseOemInfo
        {
            get => _useOemInfo;
            set
            {
                if (_useOemInfo != value)
                {
                    _useOemInfo = value;
                    OnSettingChanged(nameof(UseOemInfo), value);
                }
            }
        }

        private string _logoPath;
        public string LogoPath
        {
            get => _logoPath;
            set
            {
                if (_logoPath != value)
                {
                    _logoPath = value;
                    OnSettingChanged(nameof(LogoPath), value);
                }
            }
        }

        private string _manufacturer;
        public string Manufacturer
        {
            get => _manufacturer;
            set
            {
                if (_manufacturer != value)
                {
                    _manufacturer = value;
                    OnSettingChanged(nameof(Manufacturer), value);
                }
            }
        }

        private string _model;
        public string Model
        {
            get => _model;
            set
            {
                if (_model != value)
                {
                    _model = value;
                    OnSettingChanged(nameof(Model), value);
                }
            }
        }

        private string _supportHours;
        public string SupportHours
        {
            get => _supportHours;
            set
            {
                if (_supportHours != value)
                {
                    _supportHours = value;
                    OnSettingChanged(nameof(SupportHours), value);
                }
            }
        }

        private string _supportPhone;
        public string SupportPhone
        {
            get => _supportPhone;
            set
            {
                if (_supportPhone != value)
                {
                    _supportPhone = value;
                    OnSettingChanged(nameof(SupportPhone), value);
                }
            }
        }

        private string _supportURL;
        public string SupportURL
        {
            get => _supportURL;
            set
            {
                if (_supportURL != value)
                {
                    _supportURL = value;
                    OnSettingChanged(nameof(SupportURL), value);
                }
            }
        }

        // Event Handler
        public event EventHandler<SettingChangedEventArgs> SettingChanged;
        private void OnSettingChanged(string propertyName, object value)
        {
            SettingChanged?.Invoke(this, new SettingChangedEventArgs(propertyName, value));
        }

        // Singleton
        private static OemInfo? _instance;
        public static OemInfo Instance => _instance ??= new OemInfo();
    }

    public class DeviceInfo
    {
        private bool _useDeviceInfo;

        public bool UseDeviceInfo
        {
            get => _useDeviceInfo;
            set
            {
                if (_useDeviceInfo != value)
                {
                    _useDeviceInfo = value;
                    OnSettingChanged(nameof(UseDeviceInfo), value);
                }
            }
        }

        private string _deviceName;

        public string DeviceName
        {
            get => _deviceName;
            set
            {
                if (_deviceName != value)
                {
                    _deviceName = value;
                    OnSettingChanged(nameof(DeviceName), value);
                }
            }
        }

        private string _productKey;

        public string ProductKey
        {
            get => _productKey;
            set
            {
                if (_productKey != value)
                {
                    _productKey = value;
                    OnSettingChanged(nameof(ProductKey), value);
                }
            }
        }

        private string _registeredOwner;

        public string RegisteredOwner
        {
            get => _registeredOwner;
            set
            {
                if (_registeredOwner != value)
                {
                    _registeredOwner = value;
                    OnSettingChanged(nameof(RegisteredOwner), value);
                }
            }
        }

        private string _registeredOrganization;

        public string RegisteredOrganization
        {
            get => _registeredOrganization;
            set
            {
                if (_registeredOrganization != value)
                {
                    _registeredOrganization = value;
                    OnSettingChanged(nameof(RegisteredOrganization), value);
                }
            }
        }

        private string _timeZone;

        public string TimeZone
        {
            get => _timeZone;
            set
            {
                if (_timeZone != value)
                {
                    _timeZone = value;
                    OnSettingChanged(nameof(TimeZone), value);
                }
            }
        }

        private int _timeZoneId;

        public int TimeZoneId
        {
            get => _timeZoneId;
            set
            {
                if (_timeZoneId != value)
                {
                    _timeZoneId = value;
                    OnSettingChanged(nameof(TimeZoneId), value);
                }
            }
        }

        // Event Handler
        public event EventHandler<SettingChangedEventArgs> SettingChanged;
        private void OnSettingChanged(string propertyName, object value)
        {
            SettingChanged?.Invoke(this, new SettingChangedEventArgs(propertyName, value));
        }

        // Singleton
        private static DeviceInfo? _instance;
        public static DeviceInfo Instance => _instance ??= new DeviceInfo();
    }

    public class DomainInfo
    {
        private bool _useDomainInfo;
        public bool UseDomainInfo
        {
            get => _useDomainInfo;
            set
            {
                if (_useDomainInfo != value)
                {
                    _useDomainInfo = value;
                    OnSettingChanged(nameof(UseDomainInfo), value);
                }
            }
        }

        private string _userName;
        public string UserName
        {
            get => _userName;
            set
            {
                if (_userName != value)
                {
                    _userName = value;
                    OnSettingChanged(nameof(UserName), value);
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
                    OnSettingChanged(nameof(Password), value);
                }
            }
        }

        private string _domain;
        public string Domain
        {
            get => _domain;
            set
            {
                if (_domain != value)
                {
                    _domain = value;
                    OnSettingChanged(nameof(Domain), value);
                }
            }
        }

        // Event Handler
        public event EventHandler<SettingChangedEventArgs> SettingChanged;
        private void OnSettingChanged(string propertyName, object value)
        {
            SettingChanged?.Invoke(this, new SettingChangedEventArgs(propertyName, value));
        }

        // Singleton
        private static DomainInfo? _instance;
        public static DomainInfo Instance => _instance ??= new DomainInfo();
    }

    public class OutOfBoxExperienceInfo
    {
        private bool _useOOBEInfo;
        public bool UseOOBEInfo
        {
            get => _useOOBEInfo;
            set
            {
                if (_useOOBEInfo != value)
                {
                    _useOOBEInfo = value;
                    OnSettingChanged(nameof(UseOOBEInfo), value);
                }
            }
        }

        private bool _hideEULAPage;
        public bool HideEULAPage
        {
            get => _hideEULAPage;
            set
            {
                if (_hideEULAPage != value)
                {
                    _hideEULAPage = value;
                    OnSettingChanged(nameof(HideEULAPage), value);
                }
            }
        }

        private bool _hideOEMRegistrationScreen;
        public bool HideOEMRegistrationScreen
        {
            get => _hideOEMRegistrationScreen;
            set
            {
                if (_hideOEMRegistrationScreen != value)
                {
                    _hideOEMRegistrationScreen = value;
                    OnSettingChanged(nameof(HideOEMRegistrationScreen), value);
                }
            }
        }

        private bool _hideOnlineAccountScreens;
        public bool HideOnlineAccountScreens
        {
            get => _hideOnlineAccountScreens;
            set
            {
                if (_hideOnlineAccountScreens != value)
                {
                    _hideOnlineAccountScreens = value;
                    OnSettingChanged(nameof(HideOnlineAccountScreens), value);
                }
            }
        }

        private bool _hideWirelessSetupInOOBE;
        public bool HideWirelessSetupInOOBE
        {
            get => _hideWirelessSetupInOOBE;
            set
            {
                if (_hideWirelessSetupInOOBE != value)
                {
                    _hideWirelessSetupInOOBE = value;
                    OnSettingChanged(nameof(HideWirelessSetupInOOBE), value);
                }
            }
        }

        private string _networkLocation;
        public string NetworkLocation
        {
            get => _networkLocation;
            set
            {
                if (_networkLocation != value)
                {
                    _networkLocation = value;
                    OnSettingChanged(nameof(NetworkLocation), value);
                }
            }
        }

        private bool _skipMachineOOBE;
        public bool SkipMachineOOBE
        {
            get => _skipMachineOOBE;
            set
            {
                if (_skipMachineOOBE != value)
                {
                    _skipMachineOOBE = value;
                    OnSettingChanged(nameof(SkipMachineOOBE), value);
                }
            }
        }

        private bool _skipUserOOBE;
        public bool SkipUserOOBE
        {
            get => _skipUserOOBE;
            set
            {
                if (_skipUserOOBE != value)
                {
                    _skipUserOOBE = value;
                    OnSettingChanged(nameof(SkipUserOOBE), value);
                }
            }
        }

        private bool _hideLocalAccountScreen;
        public bool HideLocalAccountScreen
        {
            get => _hideLocalAccountScreen;
            set
            {
                if (_hideLocalAccountScreen != value)
                {
                    _hideLocalAccountScreen = value;
                    OnSettingChanged(nameof(HideLocalAccountScreen), value);
                }
            }
        }

        // Event Handler
        public event EventHandler<SettingChangedEventArgs> SettingChanged;
        private void OnSettingChanged(string propertyName, object value)
        {
            SettingChanged?.Invoke(this, new SettingChangedEventArgs(propertyName, value));
        }

        // Singleton
        private static OutOfBoxExperienceInfo _instance;
        public static OutOfBoxExperienceInfo Instance => _instance ??= new OutOfBoxExperienceInfo();
    }

    public class DeploymentOption
    {
        private bool _useSMode;
        public bool UseSMode
        {
            get => _useSMode;
            set
            {
                if (_useSMode != value)
                {
                    _useSMode = value;
                    OnSettingChanged(nameof(UseSMode), value);
                }
            }
        }

        private bool _useCopyProfile;
        public bool UseCopyProfile
        {
            get => _useCopyProfile;
            set
            {
                if (_useCopyProfile != value)
                {
                    _useCopyProfile = value;
                    OnSettingChanged(nameof(UseCopyProfile), value);
                }
            }
        }

        private bool _addDiveToWinRE;
        public bool AddDiveToWinRE
        {
            get => _addDiveToWinRE;
            set
            {
                if (_addDiveToWinRE != value)
                {
                    _addDiveToWinRE = value;
                    OnSettingChanged(nameof(AddDiveToWinRE), value);
                }
            }
        }

        // Event Handler
        public event EventHandler<SettingChangedEventArgs> SettingChanged;
        private void OnSettingChanged(string propertyName, object value)
        {
            SettingChanged?.Invoke(this, new SettingChangedEventArgs(propertyName, value));
        }

        // Singleton
        private static DeploymentOption _instance;
        public static DeploymentOption Instance => _instance ??= new DeploymentOption();
    }

    public class Tweaks
    {
        private TweakMode _currentMode;
        public TweakMode CurrentMode
        {
            get => _currentMode;
            set
            {
                if (_currentMode != value)
                {
                    _currentMode = value;
                    OnSettingChanged(nameof(CurrentMode), value);
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
                    OnSettingChanged(nameof(DiskIndex), value);
                }
            }
        }

        // Event Handler
        public event EventHandler<SettingChangedEventArgs> SettingChanged;
        private void OnSettingChanged(string propertyName, object value)
        {
            SettingChanged?.Invoke(this, new SettingChangedEventArgs(propertyName, value));
        }

        // Singleton
        private static Tweaks _instance;
        public static Tweaks Instance => _instance ??= new Tweaks();
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
        public static string PathToImage { get; set; }
        public static string ImageFileName { get; set; }
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
        MigrateUser,
        RepairBootloader
    }
}
