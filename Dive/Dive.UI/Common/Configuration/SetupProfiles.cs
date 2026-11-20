using Dive.UI.Common.UserInterface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

namespace Dive.UI.Common.Configuration
{
    /// <summary>A saved setup profile (an XML file in a "Configs" folder).</summary>
    public sealed record SetupProfile(string Name, string FilePath)
    {
        public override string ToString() => Name;
    }

    /// <summary>
    /// Loads and saves <see cref="SetupConfiguration"/> profiles. Profiles are stored in
    /// "Configs" folders: next to Dive and in the root of every drive (e.g. the Dive USB stick).
    /// </summary>
    public static class SetupProfiles
    {
        private const string FolderName = "Configs";

        /// <summary>Folder used by default when a new profile is saved.</summary>
        public static string DefaultFolder
        {
            get
            {
                var existing = GetFolders().FirstOrDefault(Directory.Exists);
                return existing ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, FolderName);
            }
        }

        private static IEnumerable<string> GetFolders()
        {
            yield return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, FolderName);
            foreach (var drive in DriveInfo.GetDrives())
            {
                bool ready;
                try { ready = drive.IsReady; } catch { ready = false; }
                if (ready) yield return Path.Combine(drive.RootDirectory.FullName, FolderName);
            }
        }

        /// <summary>Finds all profiles in the known "Configs" folders.</summary>
        public static List<SetupProfile> FindProfiles()
        {
            var profiles = new List<SetupProfile>();
            foreach (var folder in GetFolders().Distinct(StringComparer.OrdinalIgnoreCase))
            {
                try
                {
                    if (!Directory.Exists(folder)) continue;
                    foreach (var file in Directory.GetFiles(folder, "*.xml"))
                    {
                        if (IsProfile(file))
                            profiles.Add(new SetupProfile(Path.GetFileNameWithoutExtension(file), file));
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Could not read profiles from {folder}: {ex.Message}", ConsoleColor.Yellow);
                }
            }
            return profiles.OrderBy(p => p.Name, StringComparer.CurrentCultureIgnoreCase).ToList();
        }

        private static bool IsProfile(string file)
        {
            try
            {
                using var reader = XmlReader.Create(file);
                reader.MoveToContent();
                return reader.LocalName is "SetupConfiguration" or "settings";
            }
            catch
            {
                return false;
            }
        }

        public static SetupConfiguration Load(string file)
        {
            var document = new XmlDocument();
            document.Load(file);
            var directory = Path.GetDirectoryName(file)!;

            var configuration = document.DocumentElement?.LocalName switch
            {
                "SetupConfiguration" => LoadCurrentFormat(file),
                "settings" => LoadLegacyFormat(document),
                _ => throw new InvalidDataException("The file is not a Dive configuration.")
            };

            // Logo and custom answer file are stored relative to the profile
            configuration.OemLogoPath = ResolvePath(directory, configuration.OemLogoPath);
            configuration.CustomFilePath = ResolvePath(directory, configuration.CustomFilePath);

            Debug.WriteLine($"Loaded setup profile '{file}'.");
            return configuration;
        }

        public static void Save(SetupConfiguration configuration, string file)
        {
            var directory = Path.GetDirectoryName(file)!;
            Directory.CreateDirectory(directory);

            // Copy referenced files next to the profile, so it can be moved as a whole
            var copy = configuration.Clone();
            copy.OemLogoPath = CopyNextTo(directory, configuration.OemLogoPath);
            copy.CustomFilePath = CopyNextTo(directory, configuration.CustomFilePath);

            var serializer = new XmlSerializer(typeof(SetupConfiguration));
            var namespaces = new XmlSerializerNamespaces();
            namespaces.Add("", "");
            using var writer = XmlWriter.Create(file, new XmlWriterSettings { Indent = true });
            serializer.Serialize(writer, copy, namespaces);

            Debug.WriteLine($"Saved setup profile '{file}'.");
        }

        private static SetupConfiguration LoadCurrentFormat(string file)
        {
            var serializer = new XmlSerializer(typeof(SetupConfiguration));
            using var stream = File.OpenRead(file);
            return (SetupConfiguration)serializer.Deserialize(stream)!;
        }

        /// <summary>Imports the export format of older Dive versions ("settings" root).</summary>
        private static SetupConfiguration LoadLegacyFormat(XmlDocument document)
        {
            string Get(string path) => document.SelectSingleNode(path)?.InnerText ?? string.Empty;
            bool GetBool(string path) => string.Equals(Get(path), "true", StringComparison.OrdinalIgnoreCase);

            var c = new SetupConfiguration
            {
                Mode = SetupMode.Advanced,
                UserName = Get("/settings/AdministratorAccount/Username"),
                Password = Get("/settings/AdministratorAccount/Password"),
                OemLogoPath = Get("/settings/OEMSupport/OEMLogo"),
                OemManufacturer = Get("/settings/OEMSupport/Manufacturer"),
                OemModel = Get("/settings/OEMSupport/Model"),
                OemSupportHours = Get("/settings/OEMSupport/SupportHours"),
                OemSupportPhone = Get("/settings/OEMSupport/SupportNo"),
                OemSupportUrl = Get("/settings/OEMSupport/Homepage"),
                ComputerName = Get("/settings/DeviceInfo/DeviceName"),
                RegisteredOwner = Get("/settings/DeviceInfo/RegisteredOwner"),
                RegisteredOrganization = Get("/settings/DeviceInfo/RegisteredOrganization"),
                ProductKey = Get("/settings/DeviceInfo/ProductKey"),
                TimeZone = Get("/settings/DeviceInfo/Timezone"),
                DomainUser = Get("/settings/DomainInfo/UserName"),
                DomainPassword = Get("/settings/DomainInfo/Password"),
                Domain = Get("/settings/DomainInfo/Domain"),
                HideEula = GetBool("/settings/OutOfBoxExperience/HideEULAPage"),
                HideOemRegistration = GetBool("/settings/OutOfBoxExperience/HideOEMRegistrationScreen"),
                HideOnlineAccountScreens = GetBool("/settings/OutOfBoxExperience/HideOnlineAccountScreens"),
                HideWirelessSetup = GetBool("/settings/OutOfBoxExperience/HideWirelessSetupInOOBE"),
            };

            var networkLocation = Get("/settings/OutOfBoxExperience/NetworkLocation");
            if (!string.IsNullOrWhiteSpace(networkLocation)) c.NetworkLocation = networkLocation;

            c.AccountMode = string.IsNullOrWhiteSpace(c.UserName) ? AccountMode.AskDuringSetup
                : string.Equals(c.UserName, "Administrator", StringComparison.OrdinalIgnoreCase) ? AccountMode.BuiltInAdministrator
                : AccountMode.LocalAdministrator;
            return c;
        }

        private static string ResolvePath(string directory, string path)
        {
            if (string.IsNullOrWhiteSpace(path) || Path.IsPathRooted(path)) return path;
            return Path.Combine(directory, path);
        }

        private static string CopyNextTo(string directory, string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) return path;

            var fileName = Path.GetFileName(path);
            var target = Path.Combine(directory, fileName);
            try
            {
                if (!string.Equals(Path.GetFullPath(path), Path.GetFullPath(target), StringComparison.OrdinalIgnoreCase))
                    File.Copy(path, target, true);
                return fileName;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Could not copy '{path}' to the profile folder: {ex.Message}", ConsoleColor.Yellow);
                return path;
            }
        }
    }
}
