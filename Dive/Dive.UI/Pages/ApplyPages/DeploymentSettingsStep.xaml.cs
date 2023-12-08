using Dive.UI.Common;
using Dive.UI.Pages.Extras;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Serialization;
using System.Xml;
using Microsoft.Win32;

namespace Dive.UI.Pages.ApplyPages
{
    /// <summary>
    /// Interaktionslogik für DeploymentSettingsStep.xaml
    /// </summary>
    public partial class DeploymentSettingsStep : UserControl
    {
        public DeploymentSettingsStep()
        {
            InitializeComponent();
            DeploymentInfo.UseUserInfo = false;
            OemInfo.UseOemInfo = false;
        }

        private void Import_OnClicked(object sender, RoutedEventArgs e)
        {
            /*
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
                    DeploymentInfo.Username = node.SelectSingleNode("Username")!.InnerText;
                    DeploymentInfo.Password = node.SelectSingleNode("Password")!.InnerText;
                }

                TbUser.Text = DeploymentInfo.Username;
                TbPassword.Text = DeploymentInfo.Password;
                if (DeploymentInfo.Username != "" || DeploymentInfo.Password != "")
                {
                    DeploymentInfo.UseUserInfo = true;
                    ToggleUser.IsChecked = true;
                }
                else
                {
                    DeploymentInfo.UseUserInfo = false;
                    ToggleUser.IsChecked = false;
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
                            OemInfo.LogoPath = oemLogoPath;
                        }
                        else
                        {
                            // TODO: Add Error handling
                        }
                    }

                    OemInfo.Manufacturer = node.SelectSingleNode("Manufacturer")!.InnerText;
                    OemInfo.Model = node.SelectSingleNode("Model")!.InnerText;
                    OemInfo.SupportHours = node.SelectSingleNode("SupportHours")!.InnerText;
                    OemInfo.SupportPhone = node.SelectSingleNode("SupportNo")!.InnerText;
                    OemInfo.SupportURL = node.SelectSingleNode("Homepage")!.InnerText;
                }

                TbLogo.Text = OemInfo.LogoPath;
                TbManufacturer.Text = OemInfo.Manufacturer;
                TbModel.Text = OemInfo.Model;
                TbSupportHours.Text = OemInfo.SupportHours;
                TbPhone.Text = OemInfo.SupportPhone;
                TbUrl.Text = OemInfo.SupportURL;

                OemInfo.UseOemInfo = !string.IsNullOrEmpty(OemInfo.LogoPath) ||
                                     !string.IsNullOrEmpty(OemInfo.Manufacturer) ||
                                     !string.IsNullOrEmpty(OemInfo.Model) ||
                                     !string.IsNullOrEmpty(OemInfo.SupportHours) ||
                                     !string.IsNullOrEmpty(OemInfo.SupportPhone) ||
                                     !string.IsNullOrEmpty(OemInfo.SupportURL);

                ToggleOem.IsChecked = OemInfo.UseOemInfo;
                Debug.WriteLine("Successfully imported settings from file.");
            }
            catch (Exception ex) { Debug.WriteLine(ex.Message, ConsoleColor.DarkRed); }

            try
            {
                var imageNames = doc.DocumentElement!.SelectNodes("/settings/DeviceInfo");

                foreach (XmlNode node in imageNames!)
                {
                    DeviceInfo.DeviceName = node.SelectSingleNode("DeviceName")!.InnerText;
                    DeviceInfo.RegisteredOwner = node.SelectSingleNode("RegisteredOwner")!.InnerText;
                    DeviceInfo.RegisteredOrganization = node.SelectSingleNode("RegisteredOrganization")!.InnerText;
                    DeviceInfo.ProductKey = node.SelectSingleNode("ProductKey")!.InnerText;
                    DeviceInfo.TimeZone = node.SelectSingleNode("Timezone")!.InnerText;
                }

                DeviceInfo.UseDeviceInfo = !string.IsNullOrEmpty(DeviceInfo.DeviceName) ||
                                           !string.IsNullOrEmpty(DeviceInfo.RegisteredOwner) ||
                                           !string.IsNullOrEmpty(DeviceInfo.RegisteredOrganization) ||
                                           !string.IsNullOrEmpty(DeviceInfo.ProductKey) ||
                                           !string.IsNullOrEmpty(DeviceInfo.TimeZone);
            }
            catch (Exception ex) { Debug.WriteLine(ex.Message, ConsoleColor.Red); }

            try
            {
                var imageNames = doc.DocumentElement!.SelectNodes("/settings/DomainInfo");

                foreach (XmlNode node in imageNames!)
                {
                    DomainInfo.UserName = node.SelectSingleNode("UserName")!.InnerText;
                    DomainInfo.Password = node.SelectSingleNode("Password")!.InnerText;
                    DomainInfo.Domain = node.SelectSingleNode("Domain")!.InnerText;
                }

                DomainInfo.UseDomainInfo = !string.IsNullOrEmpty(DomainInfo.UserName) ||
                                           !string.IsNullOrEmpty(DomainInfo.Password) ||
                                           !string.IsNullOrEmpty(DomainInfo.Domain);
            }
            catch (Exception ex) { Debug.WriteLine(ex.Message, ConsoleColor.Red); }

            try
            {
                var imageNames = doc.DocumentElement!.SelectNodes("/settings/OutOfBoxExperience");

                foreach (XmlNode node in imageNames!)
                {
                    OutOfBoxExperienceInfo.HideEULAPage = node.SelectSingleNode("HideEULAPage")!.InnerText == "true";
                    OutOfBoxExperienceInfo.HideOEMRegistrationScreen = node.SelectSingleNode("HideOEMRegistrationScreen")!.InnerText == "true";
                    OutOfBoxExperienceInfo.HideOnlineAccountScreens = node.SelectSingleNode("HideOnlineAccountScreens")!.InnerText == "true";
                    OutOfBoxExperienceInfo.HideWirelessSetupInOOBE = node.SelectSingleNode("HideWirelessSetupInOOBE")!.InnerText == "true";
                    OutOfBoxExperienceInfo.HideLocalAccountScreen = node.SelectSingleNode("HideLocalAccountScreen")!.InnerText == "true";
                    OutOfBoxExperienceInfo.SkipMachineOOBE = node.SelectSingleNode("SkipMachineOOBE")!.InnerText == "true";
                    OutOfBoxExperienceInfo.SkipUserOOBE = node.SelectSingleNode("SkipUserOOBE")!.InnerText == "true";
                    OutOfBoxExperienceInfo.NetworkLocation = node.SelectSingleNode("NetworkLocation")!.InnerText;
                }

                OutOfBoxExperienceInfo.UseOOBEInfo = !OutOfBoxExperienceInfo.HideEULAPage ||
                                                     !OutOfBoxExperienceInfo.HideOEMRegistrationScreen ||
                                                     !OutOfBoxExperienceInfo.HideOnlineAccountScreens ||
                                                     !OutOfBoxExperienceInfo.HideWirelessSetupInOOBE ||
                                                     !OutOfBoxExperienceInfo.HideLocalAccountScreen ||
                                                     !OutOfBoxExperienceInfo.SkipMachineOOBE ||
                                                     !OutOfBoxExperienceInfo.SkipUserOOBE ||
                                                     !string.IsNullOrEmpty(OutOfBoxExperienceInfo.NetworkLocation);
            }
            catch (Exception ex) { Debug.WriteLine(ex.Message, ConsoleColor.Red); }
            */
        }

        private void Export_OnClicked(object sender, RoutedEventArgs e)
        {
            
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Dive Configuration (*.xml)|*.xml"
            };

            if (saveFileDialog.ShowDialog() != true) return;
            var fileNameOfOemLogo = Path.GetFileName(ApplyDetails.IconPath);
            if (fileNameOfOemLogo != "")
            {
                try
                {
                    File.Copy(ApplyDetails.IconPath,
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
                    Username = DeploymentInfo.Username,
                    Password = DeploymentInfo.Password,
                },
                OEMSupport = new ApplyConfig.settingsOEMSupport
                {
                    OEMLogo = fileNameOfOemLogo,
                    Manufacturer = OemInfo.Manufacturer,
                    Model = OemInfo.Model,
                    SupportHours = OemInfo.SupportHours,
                    SupportNo = OemInfo.SupportPhone,
                    Homepage = OemInfo.SupportURL
                },
                DeviceInfo = new ApplyConfig.settingsDeviceInfo
                {
                    DeviceName = DeviceInfo.DeviceName,
                    ProductKey = DeviceInfo.ProductKey,
                    RegisteredOwner = DeviceInfo.RegisteredOwner,
                    RegisteredOrganization = DeviceInfo.RegisteredOrganization,
                    Timezone = DeviceInfo.TimeZone
                },
                DomainInfo = new ApplyConfig.settingsDomainInfo
                {
                    UserName = DomainInfo.UserName,
                    Password = DomainInfo.Password,
                    Domain = DomainInfo.Domain
                },
                OutOfBoxExperience = new ApplyConfig.settingsOutOfBoxExperience
                {
                    HideEULAPage = OutOfBoxExperienceInfo.HideEULAPage,
                    HideOEMRegistrationScreen = OutOfBoxExperienceInfo.HideOEMRegistrationScreen,
                    HideOnlineAccountScreens = OutOfBoxExperienceInfo.HideOnlineAccountScreens,
                    HideWirelessSetupInOOBE = OutOfBoxExperienceInfo.HideWirelessSetupInOOBE,
                    HideLocalAccountScreen = OutOfBoxExperienceInfo.HideLocalAccountScreen,
                    NetworkLocation = OutOfBoxExperienceInfo.NetworkLocation,
                    SkipUserOOBE = OutOfBoxExperienceInfo.SkipUserOOBE,
                    SkipMachineOOBE = OutOfBoxExperienceInfo.SkipMachineOOBE
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
    }
}
