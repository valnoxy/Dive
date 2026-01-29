using Dive.UI.Common;
using Dive.UI.Common.Configuration;
using Dive.UI.Common.UserInterface;
using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Xml;
using System.Xml.Serialization;

namespace Dive.UI.Pages.ApplyPages
{
    /// <summary>
    /// Interaktionslogik für DeploymentSettingsStep.xaml
    /// </summary>
    public partial class DeploymentSettingsStep
    {
        private static readonly ApplyDetails ApplyDetailsInstance = ApplyDetails.Instance;
        private static readonly DeploymentInfo DeploymentInfoInstance = DeploymentInfo.Instance;
        private static readonly DeviceInfo DeviceInfoInstance = DeviceInfo.Instance;
        private static readonly DomainInfo DomainInfoInstance = DomainInfo.Instance;
        private static readonly OemInfo OemInfoInstance = OemInfo.Instance;
        private static readonly OutOfBoxExperienceInfo OutOfBoxExperienceInfoInstance = OutOfBoxExperienceInfo.Instance;

        public DeploymentSettingsStep()
        {
            InitializeComponent();
            DeploymentInfoInstance.UseUserInfo = false;
            OemInfoInstance.UseOemInfo = false;

            DataContext = DeploymentInfo.Instance;
        }

        private void Import_OnClicked(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Dive Configuration (*.xml)|*.xml"
            };
            if (openFileDialog.ShowDialog() != true) return;

            var doc = new XmlDocument();
            doc.LoadXml(File.ReadAllText(openFileDialog.FileName));

            try
            {
                var imageNames = doc.DocumentElement!.SelectNodes("/settings/AdministratorAccount");

                foreach (XmlNode node in imageNames!)
                {
                    DeploymentInfoInstance.Username = node.SelectSingleNode("Username")!.InnerText;
                    DeploymentInfoInstance.Password = node.SelectSingleNode("Password")!.InnerText;
                }

                if (DeploymentInfoInstance.Username != "" || DeploymentInfoInstance.Password != "")
                {
                    DeploymentInfoInstance.UseUserInfo = true;
                }
                else
                {
                    DeploymentInfoInstance.UseUserInfo = false;
                }
            }
            catch (Exception ex) { Debug.WriteLine(ex.Message, ConsoleColor.Red); }

            try
            {
                var imageNames = doc.DocumentElement!.SelectNodes("/settings/OEMSupport");

                foreach (XmlNode node in imageNames!)
                {
                    var oemLogoFileName = node.SelectSingleNode("OEMLogo")!.InnerText;
                    var oemLogoPath = Path.Combine(Path.GetDirectoryName(openFileDialog.FileName)!, oemLogoFileName);
                    if (oemLogoFileName != "")
                    {
                        if (File.Exists(oemLogoPath))
                        {
                            OemInfoInstance.LogoPath = oemLogoPath;
                        }
                        else
                        {
                            // TODO: Add Error handling
                        }
                    }

                    OemInfoInstance.Manufacturer = node.SelectSingleNode("Manufacturer")!.InnerText;
                    OemInfoInstance.Model = node.SelectSingleNode("Model")!.InnerText;
                    OemInfoInstance.SupportHours = node.SelectSingleNode("SupportHours")!.InnerText;
                    OemInfoInstance.SupportPhone = node.SelectSingleNode("SupportNo")!.InnerText;
                    OemInfoInstance.SupportURL = node.SelectSingleNode("Homepage")!.InnerText;
                }

                OemInfoInstance.UseOemInfo = !string.IsNullOrEmpty(OemInfoInstance.LogoPath) ||
                                             !string.IsNullOrEmpty(OemInfoInstance.Manufacturer) ||
                                             !string.IsNullOrEmpty(OemInfoInstance.Model) ||
                                             !string.IsNullOrEmpty(OemInfoInstance.SupportHours) ||
                                             !string.IsNullOrEmpty(OemInfoInstance.SupportPhone) ||
                                             !string.IsNullOrEmpty(OemInfoInstance.SupportURL);

                //ToggleOem.IsChecked = OemInfoInstance.UseOemInfo;
                Debug.WriteLine("Successfully imported settings from file.");
            }
            catch (Exception ex) { Debug.WriteLine(ex.Message, ConsoleColor.DarkRed); }

            try
            {
                var imageNames = doc.DocumentElement!.SelectNodes("/settings/DeviceInfo");

                foreach (XmlNode node in imageNames!)
                {
                    DeviceInfoInstance.DeviceName = node.SelectSingleNode("DeviceName")!.InnerText;
                    DeviceInfoInstance.RegisteredOwner = node.SelectSingleNode("RegisteredOwner")!.InnerText;
                    DeviceInfoInstance.RegisteredOrganization = node.SelectSingleNode("RegisteredOrganization")!.InnerText;
                    DeviceInfoInstance.ProductKey = node.SelectSingleNode("ProductKey")!.InnerText;
                    DeviceInfoInstance.TimeZone = node.SelectSingleNode("Timezone")!.InnerText;
                }

                DeviceInfoInstance.UseDeviceInfo = !string.IsNullOrEmpty(DeviceInfoInstance.DeviceName) ||
                                                   !string.IsNullOrEmpty(DeviceInfoInstance.RegisteredOwner) ||
                                                   !string.IsNullOrEmpty(DeviceInfoInstance.RegisteredOrganization) ||
                                                   !string.IsNullOrEmpty(DeviceInfoInstance.ProductKey) ||
                                                   !string.IsNullOrEmpty(DeviceInfoInstance.TimeZone);
            }
            catch (Exception ex) { Debug.WriteLine(ex.Message, ConsoleColor.Red); }

            try
            {
                var imageNames = doc.DocumentElement!.SelectNodes("/settings/DomainInfo");

                foreach (XmlNode node in imageNames!)
                {
                    DomainInfoInstance.UserName = node.SelectSingleNode("UserName")!.InnerText;
                    DomainInfoInstance.Password = node.SelectSingleNode("Password")!.InnerText;
                    DomainInfoInstance.Domain = node.SelectSingleNode("Domain")!.InnerText;
                }

                DomainInfoInstance.UseDomainInfo = !string.IsNullOrEmpty(DomainInfoInstance.UserName) ||
                                                   !string.IsNullOrEmpty(DomainInfoInstance.Password) ||
                                                   !string.IsNullOrEmpty(DomainInfoInstance.Domain);
            }
            catch (Exception ex) { Debug.WriteLine(ex.Message, ConsoleColor.Red); }

            try
            {
                var imageNames = doc.DocumentElement!.SelectNodes("/settings/OutOfBoxExperience");

                foreach (XmlNode node in imageNames!)
                {
                    OutOfBoxExperienceInfoInstance.HideEULAPage = node.SelectSingleNode("HideEULAPage")!.InnerText == "true";
                    OutOfBoxExperienceInfoInstance.HideOEMRegistrationScreen = node.SelectSingleNode("HideOEMRegistrationScreen")!.InnerText == "true";
                    OutOfBoxExperienceInfoInstance.HideOnlineAccountScreens = node.SelectSingleNode("HideOnlineAccountScreens")!.InnerText == "true";
                    OutOfBoxExperienceInfoInstance.HideWirelessSetupInOOBE = node.SelectSingleNode("HideWirelessSetupInOOBE")!.InnerText == "true";
                    OutOfBoxExperienceInfoInstance.HideLocalAccountScreen = node.SelectSingleNode("HideLocalAccountScreen")!.InnerText == "true";
                    OutOfBoxExperienceInfoInstance.SkipMachineOOBE = node.SelectSingleNode("SkipMachineOOBE")!.InnerText == "true";
                    OutOfBoxExperienceInfoInstance.SkipUserOOBE = node.SelectSingleNode("SkipUserOOBE")!.InnerText == "true";
                    OutOfBoxExperienceInfoInstance.NetworkLocation = node.SelectSingleNode("NetworkLocation")!.InnerText;
                }

                OutOfBoxExperienceInfoInstance.UseOOBEInfo = !OutOfBoxExperienceInfoInstance.HideEULAPage ||
                                                             !OutOfBoxExperienceInfoInstance.HideOEMRegistrationScreen ||
                                                             !OutOfBoxExperienceInfoInstance.HideOnlineAccountScreens ||
                                                             !OutOfBoxExperienceInfoInstance.HideWirelessSetupInOOBE ||
                                                             !OutOfBoxExperienceInfoInstance.HideLocalAccountScreen ||
                                                             !OutOfBoxExperienceInfoInstance.SkipMachineOOBE ||
                                                             !OutOfBoxExperienceInfoInstance.SkipUserOOBE ||
                                                             !string.IsNullOrEmpty(OutOfBoxExperienceInfoInstance.NetworkLocation);
            }
            catch (Exception ex) { Debug.WriteLine(ex.Message, ConsoleColor.Red); }
        }

        private void Export_OnClicked(object sender, RoutedEventArgs e)
        {

            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Dive Configuration (*.xml)|*.xml"
            };

            if (saveFileDialog.ShowDialog() != true) return;
            var fileNameOfOemLogo = Path.GetFileName(ApplyDetailsInstance.IconPath);
            if (fileNameOfOemLogo != "")
            {
                try
                {
                    File.Copy(ApplyDetailsInstance.IconPath,
                        Path.Combine(Path.GetDirectoryName(saveFileDialog.FileName)!, fileNameOfOemLogo));
                }
                catch
                {
                    // TODO: Add error handling
                }
            }

            var settings = new ApplyConfig.settings
            {
                AdministratorAccount = new ApplyConfig.settingsAdministratorAccount
                {
                    Username = DeploymentInfoInstance.Username,
                    Password = DeploymentInfoInstance.Password,
                },
                OEMSupport = new ApplyConfig.settingsOEMSupport
                {
                    OEMLogo = fileNameOfOemLogo,
                    Manufacturer = OemInfoInstance.Manufacturer,
                    Model = OemInfoInstance.Model,
                    SupportHours = OemInfoInstance.SupportHours,
                    SupportNo = OemInfoInstance.SupportPhone,
                    Homepage = OemInfoInstance.SupportURL
                },
                DeviceInfo = new ApplyConfig.settingsDeviceInfo
                {
                    DeviceName = DeviceInfoInstance.DeviceName,
                    ProductKey = DeviceInfoInstance.ProductKey,
                    RegisteredOwner = DeviceInfoInstance.RegisteredOwner,
                    RegisteredOrganization = DeviceInfoInstance.RegisteredOrganization,
                    Timezone = DeviceInfoInstance.TimeZone
                },
                DomainInfo = new ApplyConfig.settingsDomainInfo
                {
                    UserName = DomainInfoInstance.UserName,
                    Password = DomainInfoInstance.Password,
                    Domain = DomainInfoInstance.Domain
                },
                OutOfBoxExperience = new ApplyConfig.settingsOutOfBoxExperience
                {
                    HideEULAPage = OutOfBoxExperienceInfoInstance.HideEULAPage,
                    HideOEMRegistrationScreen = OutOfBoxExperienceInfoInstance.HideOEMRegistrationScreen,
                    HideOnlineAccountScreens = OutOfBoxExperienceInfoInstance.HideOnlineAccountScreens,
                    HideWirelessSetupInOOBE = OutOfBoxExperienceInfoInstance.HideWirelessSetupInOOBE,
                    HideLocalAccountScreen = OutOfBoxExperienceInfoInstance.HideLocalAccountScreen,
                    NetworkLocation = OutOfBoxExperienceInfoInstance.NetworkLocation,
                    SkipUserOOBE = OutOfBoxExperienceInfoInstance.SkipUserOOBE,
                    SkipMachineOOBE = OutOfBoxExperienceInfoInstance.SkipMachineOOBE
                }
            };

            var ns = new XmlSerializerNamespaces();
            ns.Add("", "");

            // Create the serializer
            var slz = new XmlSerializer(typeof(ApplyConfig.settings));
            using TextWriter writer = new StreamWriter(saveFileDialog.FileName);
            slz.Serialize(writer, settings);

            Debug.WriteLine("Successfully exported settings to file.");
        }

        private void DeploymentSettingsStep_OnLoaded(object sender, RoutedEventArgs e)
        {
            NavView.Navigate(typeof(Extras.UnattendConfigurationPages.DeploymentConfigurationPage));
        }

        private void ConfigAssistant(object sender, RoutedEventArgs e)
        {
            var w = new Extras.ConfigAssistant.ConfigAssistantWindow();
            w.ShowDialog();
        }
    }
}
