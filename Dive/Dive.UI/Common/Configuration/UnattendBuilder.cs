using Dive.UI.Common.Configuration;
using Dive.UI.Common.UserInterface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace Dive.UI.Common
{
    /// <summary>
    /// Generates the unattend.xml for the selected image from a <see cref="SetupConfiguration"/>.
    /// </summary>
    public static class UnattendBuilder
    {
        private static readonly XNamespace Ns = "urn:schemas-microsoft-com:unattend";
        private static readonly XNamespace Wcm = "http://schemas.microsoft.com/WMIConfig/2002/State";

        /// <summary>
        /// Builds the answer file for the current configuration and image.
        /// Returns null if no answer file is required (standard setup without options).
        /// </summary>
        public static string? Build() => Build(SetupConfiguration.Current, ApplyDetails.Instance);

        public static string? Build(SetupConfiguration configuration, ApplyDetails image)
        {
            Debug.WriteLine("[UnattendBuilder v3] Building answer file ...");

            if (configuration.Mode == SetupMode.CustomFile)
                return File.ReadAllText(configuration.CustomFilePath);

            var c = configuration.GetEffective();
            var target = new TargetInfo(image);
            var settings = new List<XElement>();

            AddIfNotEmpty(settings, BuildOfflineServicing(c, target));
            AddIfNotEmpty(settings, BuildSpecialize(c, target));
            AddIfNotEmpty(settings, BuildOobeSystem(c, target));

            if (settings.Count == 0)
            {
                Debug.WriteLine("[UnattendBuilder v3] Nothing to configure, no answer file required.");
                return null;
            }

            var document = new XDocument(
                new XDeclaration("1.0", "utf-8", null),
                new XElement(Ns + "unattend",
                    new XAttribute(XNamespace.Xmlns + "wcm", Wcm),
                    settings));

            Debug.WriteLine($"[UnattendBuilder v3] Done ({settings.Count} pass(es), architecture {target.Architecture}).");
            return ToXml(document);
        }

        #region Passes

        private static XElement? BuildOfflineServicing(SetupConfiguration c, TargetInfo t)
        {
            if (!c.SMode) return null;
            return Pass("offlineServicing",
                Component("Microsoft-Windows-CodeIntegrity", t,
                    new XElement(Ns + "SkuPolicyRequired", 1)));
        }

        private static XElement? BuildSpecialize(SetupConfiguration c, TargetInfo t)
        {
            var components = new List<XElement>();
            var answers = c.UsesAnswerFileSettings;

            // Shell-Setup (child elements in schema order)
            var shell = new List<XElement>();
            if (answers) AddValue(shell, "ComputerName", c.ComputerName);
            if (c.CopyProfile) shell.Add(new XElement(Ns + "CopyProfile", true));
            if (answers) AddValue(shell, "ProductKey", c.ProductKey.Trim().ToUpperInvariant());
            if (answers) AddValue(shell, "TimeZone", c.TimeZone);
            if (shell.Count > 0)
                components.Add(Component("Microsoft-Windows-Shell-Setup", t, shell));

            // Domain join
            if (answers && c.HasDomain)
            {
                components.Add(Component("Microsoft-Windows-UnattendedJoin", t,
                    new XElement(Ns + "Identification",
                        new XElement(Ns + "Credentials",
                            new XElement(Ns + "Domain", c.Domain),
                            new XElement(Ns + "Password", c.DomainPassword),
                            new XElement(Ns + "Username", c.DomainUser)),
                        new XElement(Ns + "JoinDomain", c.Domain))));
            }

            // Windows 11: allow setup without internet connection
            if (answers && c.BypassNetworkRequirement && t.IsWindows10OrNewer)
            {
                components.Add(Component("Microsoft-Windows-Deployment", t,
                    new XElement(Ns + "RunSynchronous",
                        SynchronousCommand("RunSynchronousCommand", 1,
                            @"reg add HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\OOBE /v BypassNRO /t REG_DWORD /d 1 /f",
                            "Allow setup without internet connection"))));
            }

            return components.Count == 0 ? null : Pass("specialize", components);
        }

        private static XElement? BuildOobeSystem(SetupConfiguration c, TargetInfo t)
        {
            if (!c.UsesAnswerFileSettings) return null;
            var components = new List<XElement>();

            // Region and keyboard
            if (!string.IsNullOrWhiteSpace(c.Locale) || !string.IsNullOrWhiteSpace(c.KeyboardLocale))
            {
                var keyboard = string.IsNullOrWhiteSpace(c.KeyboardLocale) ? c.Locale : c.KeyboardLocale;
                var international = new List<XElement>();
                AddValue(international, "InputLocale", keyboard);
                AddValue(international, "SystemLocale", c.Locale);
                AddValue(international, "UserLocale", c.Locale);
                components.Add(Component("Microsoft-Windows-International-Core", t, international));
            }

            var shell = new List<XElement>();
            var hasAccount = c.HasAccount && !string.IsNullOrWhiteSpace(c.EffectiveUserName);

            // AutoLogon
            if (hasAccount && c.AutoLogon)
            {
                shell.Add(new XElement(Ns + "AutoLogon",
                    Password(c.Password),
                    new XElement(Ns + "Enabled", true),
                    new XElement(Ns + "LogonCount", 1),
                    new XElement(Ns + "Username", c.EffectiveUserName)));
            }

            // Password never expires (local account only)
            if (hasAccount && c.PasswordNeverExpires && c.AccountMode == AccountMode.LocalAdministrator)
            {
                var command = t.IsWindows10OrNewer
                    ? $"powershell.exe -NoProfile -Command \"Set-LocalUser -Name '{c.EffectiveUserName.Replace("'", "''")}' -PasswordNeverExpires $true\""
                    : $"cmd /C wmic useraccount where name=\"{c.EffectiveUserName}\" set PasswordExpires=false";
                shell.Add(new XElement(Ns + "FirstLogonCommands",
                    SynchronousCommand("SynchronousCommand", 1, command, "Password never expires")));
            }

            // OEM information
            if (c.HasOemInfo)
            {
                var oem = new List<XElement>();
                if (!string.IsNullOrWhiteSpace(c.OemLogoPath))
                    oem.Add(new XElement(Ns + "Logo", @"%WINDIR%\System32\logo.bmp"));
                AddValue(oem, "Manufacturer", c.OemManufacturer);
                AddValue(oem, "Model", c.OemModel);
                AddValue(oem, "SupportHours", c.OemSupportHours);
                AddValue(oem, "SupportPhone", c.OemSupportPhone);
                AddValue(oem, "SupportURL", c.OemSupportUrl);
                shell.Add(new XElement(Ns + "OEMInformation", oem));
            }

            // OOBE screens
            var oobe = new List<XElement>();
            if (c.HideEula) oobe.Add(new XElement(Ns + "HideEULAPage", true));
            if (t.IsWindows8OrNewer && hasAccount) oobe.Add(new XElement(Ns + "HideLocalAccountScreen", true));
            if (t.IsWindows8OrNewer && c.HideOemRegistration) oobe.Add(new XElement(Ns + "HideOEMRegistrationScreen", true));
            if (t.IsWindows8OrNewer && c.HideOnlineAccountScreens) oobe.Add(new XElement(Ns + "HideOnlineAccountScreens", true));
            if (c.HideWirelessSetup) oobe.Add(new XElement(Ns + "HideWirelessSetupInOOBE", true));
            if (!t.IsWindows8OrNewer) AddValue(oobe, "NetworkLocation", c.NetworkLocation);
            if (c.SkipPrivacySettings) oobe.Add(new XElement(Ns + "ProtectYourPC", 3));
            if (oobe.Count > 0) shell.Add(new XElement(Ns + "OOBE", oobe));

            AddValue(shell, "RegisteredOrganization", c.RegisteredOrganization);
            AddValue(shell, "RegisteredOwner", c.RegisteredOwner);

            // User accounts
            if (hasAccount)
            {
                if (c.AccountMode == AccountMode.BuiltInAdministrator)
                {
                    shell.Add(new XElement(Ns + "UserAccounts",
                        new XElement(Ns + "AdministratorPassword",
                            new XElement(Ns + "Value", c.Password),
                            new XElement(Ns + "PlainText", true))));
                }
                else
                {
                    shell.Add(new XElement(Ns + "UserAccounts",
                        new XElement(Ns + "LocalAccounts",
                            new XElement(Ns + "LocalAccount",
                                new XAttribute(Wcm + "action", "add"),
                                Password(c.Password),
                                new XElement(Ns + "DisplayName", c.EffectiveUserName),
                                new XElement(Ns + "Group", "Administrators"),
                                new XElement(Ns + "Name", c.EffectiveUserName)))));
                }
            }

            if (shell.Count > 0)
                components.Add(Component("Microsoft-Windows-Shell-Setup", t, shell));

            return components.Count == 0 ? null : Pass("oobeSystem", components);
        }

        #endregion

        #region Helpers

        /// <summary>Information about the image the answer file is built for.</summary>
        private sealed class TargetInfo
        {
            public string Architecture { get; }
            public bool IsWindows8OrNewer { get; }
            public bool IsWindows10OrNewer { get; }

            public TargetInfo(ApplyDetails image)
            {
                var arch = image.Architecture ?? string.Empty;
                Architecture = arch.EndsWith("arm64", StringComparison.OrdinalIgnoreCase) ? "arm64"
                    : arch.EndsWith("x86", StringComparison.OrdinalIgnoreCase) ? "x86"
                    : "amd64";

                var version = image.NTVersion ?? string.Empty;
                IsWindows10OrNewer = version.StartsWith("10.", StringComparison.Ordinal);
                IsWindows8OrNewer = IsWindows10OrNewer || version is "6.2" or "6.3";
            }
        }

        private static XElement Pass(string name, params object[] components) =>
            new(Ns + "settings", new XAttribute("pass", name), components);

        private static XElement Component(string name, TargetInfo t, params object[] content) =>
            new(Ns + "component",
                new XAttribute("name", name),
                new XAttribute("processorArchitecture", t.Architecture),
                new XAttribute("publicKeyToken", "31bf3856ad364e35"),
                new XAttribute("language", "neutral"),
                new XAttribute("versionScope", "nonSxS"),
                content);

        private static XElement Password(string password) =>
            new(Ns + "Password",
                new XElement(Ns + "Value", password),
                new XElement(Ns + "PlainText", true));

        private static XElement SynchronousCommand(string elementName, int order, string commandLine, string description) =>
            new(Ns + elementName,
                new XAttribute(Wcm + "action", "add"),
                new XElement(Ns + "CommandLine", commandLine),
                new XElement(Ns + "Description", description),
                new XElement(Ns + "Order", order));

        private static void AddValue(List<XElement> target, string name, string? value)
        {
            if (!string.IsNullOrWhiteSpace(value))
                target.Add(new XElement(Ns + name, value.Trim()));
        }

        private static void AddIfNotEmpty(List<XElement> target, XElement? element)
        {
            if (element != null) target.Add(element);
        }

        private static string ToXml(XDocument document)
        {
            var builder = new StringBuilder();
            using (var writer = XmlWriter.Create(new Utf8StringWriter(builder), new XmlWriterSettings { Indent = true, Encoding = Encoding.UTF8 }))
                document.Save(writer);
            return builder.ToString();
        }

        private sealed class Utf8StringWriter(StringBuilder builder) : StringWriter(builder)
        {
            public override Encoding Encoding => Encoding.UTF8;
        }

        #endregion
    }
}
