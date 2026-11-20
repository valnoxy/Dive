using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace Dive.UI.Common.Configuration
{
    /// <summary>How Windows is set up on first start.</summary>
    public enum SetupMode
    {
        /// <summary>No answer file: the regular Windows setup (OOBE) runs.</summary>
        Standard,
        /// <summary>A few questions, everything else is skipped automatically.</summary>
        Express,
        /// <summary>All options, grouped by topic.</summary>
        Advanced,
        /// <summary>A user-provided unattend.xml is used as is.</summary>
        CustomFile
    }

    public enum AccountMode
    {
        /// <summary>Windows asks for an account during setup.</summary>
        AskDuringSetup,
        /// <summary>A new local account in the Administrators group.</summary>
        LocalAdministrator,
        /// <summary>The built-in "Administrator" account is enabled.</summary>
        BuiltInAdministrator
    }

    public enum SummaryKind { Info, Skipped, Warning, Error }

    /// <summary>One line of the "On first start" summary.</summary>
    public sealed record SummaryItem(SummaryKind Kind, string Text);

    /// <summary>
    /// All settings of the "Set up Windows" step. Single source for the UI, the profiles
    /// and the unattend generator.
    /// </summary>
    [XmlRoot("SetupConfiguration")]
    public sealed class SetupConfiguration : INotifyPropertyChanged
    {
        private static SetupConfiguration? _current;

        /// <summary>The configuration used for the current deployment.</summary>
        [XmlIgnore]
        public static SetupConfiguration Current
        {
            get => _current ??= new SetupConfiguration();
            set
            {
                _current = value;
                CurrentChanged?.Invoke(value);
            }
        }

        /// <summary>Raised when <see cref="Current"/> is replaced (e.g. a profile was loaded).</summary>
        public static event Action<SetupConfiguration>? CurrentChanged;

        public event PropertyChangedEventHandler? PropertyChanged;

        private bool Set<T>(ref T field, T value, [CallerMemberName] string? name = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            return true;
        }

        #region Mode
        private SetupMode _mode = SetupMode.Standard;
        public SetupMode Mode { get => _mode; set => Set(ref _mode, value); }

        private string _customFilePath = string.Empty;
        public string CustomFilePath { get => _customFilePath; set => Set(ref _customFilePath, value ?? string.Empty); }
        #endregion

        #region Account
        private AccountMode _accountMode = AccountMode.LocalAdministrator;
        public AccountMode AccountMode { get => _accountMode; set => Set(ref _accountMode, value); }

        private string _userName = string.Empty;
        public string UserName { get => _userName; set => Set(ref _userName, value ?? string.Empty); }

        private string _password = string.Empty;
        public string Password { get => _password; set => Set(ref _password, value ?? string.Empty); }

        private bool _autoLogon = true;
        /// <summary>Signs in once automatically after setup.</summary>
        public bool AutoLogon { get => _autoLogon; set => Set(ref _autoLogon, value); }

        private bool _passwordNeverExpires = true;
        public bool PasswordNeverExpires { get => _passwordNeverExpires; set => Set(ref _passwordNeverExpires, value); }
        #endregion

        #region Device
        private string _computerName = string.Empty;
        public string ComputerName { get => _computerName; set => Set(ref _computerName, value ?? string.Empty); }

        private string _productKey = string.Empty;
        public string ProductKey { get => _productKey; set => Set(ref _productKey, value ?? string.Empty); }

        private string _registeredOwner = string.Empty;
        public string RegisteredOwner { get => _registeredOwner; set => Set(ref _registeredOwner, value ?? string.Empty); }

        private string _registeredOrganization = string.Empty;
        public string RegisteredOrganization { get => _registeredOrganization; set => Set(ref _registeredOrganization, value ?? string.Empty); }
        #endregion

        #region Region
        private string _locale = string.Empty;
        /// <summary>Region and formats (e.g. "de-DE"). Empty = unchanged.</summary>
        public string Locale { get => _locale; set => Set(ref _locale, value ?? string.Empty); }

        private string _keyboardLocale = string.Empty;
        /// <summary>Keyboard layout as locale (e.g. "de-DE"). Empty = same as <see cref="Locale"/>.</summary>
        public string KeyboardLocale { get => _keyboardLocale; set => Set(ref _keyboardLocale, value ?? string.Empty); }

        private string _timeZone = string.Empty;
        /// <summary>Windows time zone id (e.g. "W. Europe Standard Time"). Empty = unchanged.</summary>
        public string TimeZone { get => _timeZone; set => Set(ref _timeZone, value ?? string.Empty); }
        #endregion

        #region Setup (OOBE)
        private bool _bypassNetworkRequirement = true;
        /// <summary>Allows Windows 11 to be set up without internet / Microsoft account.</summary>
        public bool BypassNetworkRequirement { get => _bypassNetworkRequirement; set => Set(ref _bypassNetworkRequirement, value); }

        private bool _hideEula = true;
        public bool HideEula { get => _hideEula; set => Set(ref _hideEula, value); }

        private bool _skipPrivacySettings = true;
        public bool SkipPrivacySettings { get => _skipPrivacySettings; set => Set(ref _skipPrivacySettings, value); }

        private bool _hideOnlineAccountScreens = true;
        public bool HideOnlineAccountScreens { get => _hideOnlineAccountScreens; set => Set(ref _hideOnlineAccountScreens, value); }

        private bool _hideWirelessSetup;
        public bool HideWirelessSetup { get => _hideWirelessSetup; set => Set(ref _hideWirelessSetup, value); }

        private bool _hideOemRegistration = true;
        public bool HideOemRegistration { get => _hideOemRegistration; set => Set(ref _hideOemRegistration, value); }

        private string _networkLocation = "Work";
        /// <summary>Network location for Windows Vista / 7 (Home, Work, Other).</summary>
        public string NetworkLocation { get => _networkLocation; set => Set(ref _networkLocation, value ?? "Work"); }
        #endregion

        #region Domain
        private string _domain = string.Empty;
        public string Domain { get => _domain; set => Set(ref _domain, value ?? string.Empty); }

        private string _domainUser = string.Empty;
        public string DomainUser { get => _domainUser; set => Set(ref _domainUser, value ?? string.Empty); }

        private string _domainPassword = string.Empty;
        public string DomainPassword { get => _domainPassword; set => Set(ref _domainPassword, value ?? string.Empty); }
        #endregion

        #region OEM
        private string _oemManufacturer = string.Empty;
        public string OemManufacturer { get => _oemManufacturer; set => Set(ref _oemManufacturer, value ?? string.Empty); }

        private string _oemModel = string.Empty;
        public string OemModel { get => _oemModel; set => Set(ref _oemModel, value ?? string.Empty); }

        private string _oemSupportHours = string.Empty;
        public string OemSupportHours { get => _oemSupportHours; set => Set(ref _oemSupportHours, value ?? string.Empty); }

        private string _oemSupportPhone = string.Empty;
        public string OemSupportPhone { get => _oemSupportPhone; set => Set(ref _oemSupportPhone, value ?? string.Empty); }

        private string _oemSupportUrl = string.Empty;
        public string OemSupportUrl { get => _oemSupportUrl; set => Set(ref _oemSupportUrl, value ?? string.Empty); }

        private string _oemLogoPath = string.Empty;
        /// <summary>BMP logo (120x120). Copied to %WINDIR%\System32\logo.bmp.</summary>
        public string OemLogoPath { get => _oemLogoPath; set => Set(ref _oemLogoPath, value ?? string.Empty); }
        #endregion

        #region Options
        private bool _sMode;
        public bool SMode { get => _sMode; set => Set(ref _sMode, value); }

        private bool _copyProfile;
        /// <summary>Copies the profile of a captured image to the default profile.</summary>
        public bool CopyProfile { get => _copyProfile; set => Set(ref _copyProfile, value); }

        private bool _addDiveToRecovery;
        /// <summary>Adds Dive as a tool to the Windows recovery environment (not part of the answer file).</summary>
        public bool AddDiveToRecovery { get => _addDiveToRecovery; set => Set(ref _addDiveToRecovery, value); }
        #endregion

        #region Derived state

        [XmlIgnore] public bool UsesAnswerFileSettings => Mode is SetupMode.Express or SetupMode.Advanced;
        [XmlIgnore] public bool HasAccount => UsesAnswerFileSettings && AccountMode != AccountMode.AskDuringSetup;
        [XmlIgnore] public bool HasDomain => Mode == SetupMode.Advanced && !string.IsNullOrWhiteSpace(Domain);
        [XmlIgnore] public bool HasOemInfo => Mode == SetupMode.Advanced &&
                                              new[] { OemManufacturer, OemModel, OemSupportHours, OemSupportPhone, OemSupportUrl, OemLogoPath }
                                                  .Any(v => !string.IsNullOrWhiteSpace(v));

        /// <summary>The user name that is actually created ("Administrator" for the built-in account).</summary>
        [XmlIgnore] public string EffectiveUserName => AccountMode == AccountMode.BuiltInAdministrator ? "Administrator" : UserName.Trim();

        /// <summary>
        /// Returns the settings that are actually applied. In express mode the setup options are
        /// fixed to sensible defaults; options that are hidden in the current mode are ignored.
        /// </summary>
        public SetupConfiguration Clone()
        {
            var copy = (SetupConfiguration)MemberwiseClone();
            copy.PropertyChanged = null;
            return copy;
        }

        public SetupConfiguration GetEffective()
        {
            var e = Clone();

            if (Mode == SetupMode.Express)
            {
                e.AccountMode = AccountMode.LocalAdministrator;
                e.AutoLogon = true;
                e.PasswordNeverExpires = true;
                e.KeyboardLocale = string.Empty;
                e.ProductKey = string.Empty;
                e.RegisteredOwner = string.Empty;
                e.RegisteredOrganization = string.Empty;
                e.HideEula = true;
                e.SkipPrivacySettings = true;
                e.HideOnlineAccountScreens = true;
                e.HideOemRegistration = true;
                e.HideWirelessSetup = BypassNetworkRequirement;
                e.Domain = e.DomainUser = e.DomainPassword = string.Empty;
                e.OemManufacturer = e.OemModel = e.OemSupportHours = e.OemSupportPhone = e.OemSupportUrl = e.OemLogoPath = string.Empty;
            }

            return e;
        }

        #endregion

        #region Validation and summary

        private static readonly Regex ProductKeyRegex = new(@"^[A-Z0-9]{5}(-[A-Z0-9]{5}){4}$", RegexOptions.IgnoreCase);
        private static readonly char[] InvalidUserNameChars = "\"/\\[]:;|=,+*?<>@".ToCharArray();
        private static readonly Regex ComputerNameRegex = new(@"^[A-Za-z0-9-]{1,15}$");

        private static string L(string key) => LocalizationManager.LocalizeValue(key);
        private static string L(string key, params object[] args) => string.Format(LocalizationManager.LocalizeValue(key), args);

        /// <summary>
        /// Describes what happens on first start, including warnings and blocking errors.
        /// </summary>
        public List<SummaryItem> GetSummary(bool isModernWindows)
        {
            var items = new List<SummaryItem>();
            var e = GetEffective();

            switch (Mode)
            {
                case SetupMode.Standard:
                    items.Add(new(SummaryKind.Info, L("SetupSumStandard")));
                    AddOptions(items, e);
                    return items;

                case SetupMode.CustomFile:
                    if (string.IsNullOrWhiteSpace(CustomFilePath))
                        items.Add(new(SummaryKind.Error, L("SetupErrNoFile")));
                    else if (!System.IO.File.Exists(CustomFilePath))
                        items.Add(new(SummaryKind.Error, L("SetupErrFileMissing")));
                    else
                        items.Add(new(SummaryKind.Info, L("SetupSumCustomFile", System.IO.Path.GetFileName(CustomFilePath))));
                    return items;
            }

            // Device
            if (string.IsNullOrWhiteSpace(e.ComputerName))
                items.Add(new(SummaryKind.Info, L("SetupSumRandomName")));
            else if (!ComputerNameRegex.IsMatch(e.ComputerName) || e.ComputerName.All(char.IsDigit))
                items.Add(new(SummaryKind.Error, L("SetupErrComputerName")));
            else
                items.Add(new(SummaryKind.Info, L("SetupSumComputerName", e.ComputerName)));

            // Account
            switch (e.AccountMode)
            {
                case AccountMode.AskDuringSetup:
                    items.Add(new(SummaryKind.Info, L("SetupSumAskAccount")));
                    break;
                default:
                    var user = e.EffectiveUserName;
                    if (string.IsNullOrWhiteSpace(user))
                        items.Add(new(SummaryKind.Error, L("SetupErrNoUser")));
                    else if (user.IndexOfAny(InvalidUserNameChars) >= 0 || user.Length > 20)
                        items.Add(new(SummaryKind.Error, L("SetupErrUserName")));
                    else if (string.Equals(user, e.ComputerName, StringComparison.OrdinalIgnoreCase))
                        items.Add(new(SummaryKind.Error, L("SetupErrUserEqualsComputer")));
                    else
                        items.Add(new(SummaryKind.Info, L(e.AutoLogon ? "SetupSumAccountAutoLogon" : "SetupSumAccount", user)));

                    if (!string.IsNullOrWhiteSpace(user) && string.IsNullOrEmpty(e.Password))
                        items.Add(new(SummaryKind.Warning, L("SetupWarnNoPassword")));
                    break;
            }

            // Region
            if (!string.IsNullOrWhiteSpace(e.Locale) || !string.IsNullOrWhiteSpace(e.TimeZone))
                items.Add(new(SummaryKind.Info, L("SetupSumRegion")));

            // Setup screens
            if (e.HideEula || e.SkipPrivacySettings || e.HideOnlineAccountScreens || e.HideWirelessSetup || e.HideOemRegistration)
                items.Add(new(SummaryKind.Skipped, L("SetupSumSkippedScreens")));
            if (e.BypassNetworkRequirement && isModernWindows)
                items.Add(new(SummaryKind.Skipped, L("SetupSumOffline")));

            // Domain
            if (e.HasDomain)
            {
                if (string.IsNullOrWhiteSpace(e.DomainUser) || string.IsNullOrEmpty(e.DomainPassword))
                    items.Add(new(SummaryKind.Error, L("SetupErrDomainCredentials")));
                else
                    items.Add(new(SummaryKind.Info, L("SetupSumDomain", e.Domain)));
            }

            // OEM
            if (e.HasOemInfo)
            {
                items.Add(new(SummaryKind.Info, L("SetupSumOem")));
                if (!string.IsNullOrWhiteSpace(e.OemLogoPath) && !System.IO.File.Exists(e.OemLogoPath))
                    items.Add(new(SummaryKind.Error, L("SetupErrLogoMissing")));
            }

            // Product key
            if (string.IsNullOrWhiteSpace(e.ProductKey))
            {
                if (Mode == SetupMode.Advanced)
                    items.Add(new(SummaryKind.Warning, L("SetupWarnNoProductKey")));
            }
            else if (!ProductKeyRegex.IsMatch(e.ProductKey.Trim()))
                items.Add(new(SummaryKind.Error, L("SetupErrProductKey")));

            AddOptions(items, e);
            return items;
        }

        private static void AddOptions(List<SummaryItem> items, SetupConfiguration e)
        {
            if (e.CopyProfile) items.Add(new(SummaryKind.Info, L("SetupSumCopyProfile")));
            if (e.SMode) items.Add(new(SummaryKind.Info, L("SetupSumSMode")));
            if (e.AddDiveToRecovery) items.Add(new(SummaryKind.Info, L("SetupSumDiveRecovery")));
        }

        #endregion
    }
}
