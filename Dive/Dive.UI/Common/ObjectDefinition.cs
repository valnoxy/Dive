using Dive.UI.Common.Plugins;
using System;
using System.Collections.Generic;

namespace Dive.UI.Common
{
    public class Plugin
    {
        public static PluginManager PluginManager { get; } = new();
    }

    public class SettingChangedEventArgs(string propertyName, object newValue) : EventArgs
    {
        public string PropertyName { get; } = propertyName;
        public object NewValue { get; } = newValue;
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

        private string _architecture;
        /// <summary>Architecture of the selected image as shown in the image list (e.g. "NT 10.0.22621.525 - x64").</summary>
        public string Architecture
        {
            get => _architecture;
            set
            {
                if (_architecture != value)
                {
                    _architecture = value;
                    OnSettingChanged(nameof(Architecture), value);
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
        // UefiSeven
        public static bool InstallUefiSeven { get; set; }
        public static bool UsToggleLog { get; set; }
        public static bool UsToggleVerbose { get; set; }
        public static bool UsToggleFakeVesa { get; set; }
        public static bool UsToggleSkipErros { get; set; }

        // CsmWrap
        public static bool InstallCsmWrap { get; set; }
    }

    public enum TweakMode
    {
        AutoInit,
        Migrate,
        MigrateUser,
        RepairBootloader
    }
}
