using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using deployaUI.Common;
using System.IO;
using System.Xml.Serialization;
using Path = System.IO.Path;
using Wpf.Ui.Common;
using deploya_core;
using System.Diagnostics.Metrics;
using System.Xml;

namespace deployaUI.Pages.ApplyPages
{
    /// <summary>
    /// Interaktionslogik für DeploymentSettingsStep.xaml
    /// </summary>
    public partial class DeploymentSettingsStep : UserControl
    {
        public DeploymentSettingsStep()
        {
            InitializeComponent();
            Common.DeploymentInfo.UseUserInfo = false;
            Common.OemInfo.UseOemInfo = false;
        }

        private void TbUser_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (TbUser.Text == "")
            {
                TbPassword.IsEnabled = false;
                TbPassword.Text = "";
            }
            else
            {
                TbPassword.IsEnabled = true;
                Common.DeploymentInfo.Username = TbUser.Text;
            }
        }

        private void TbPassword_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            Common.DeploymentInfo.Password = TbPassword.Text;
        }

        private void OEMLogo_OpenFileClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "OEM Logo (*.bmp)|*.bmp";
            if (openFileDialog.ShowDialog() == true)
            {
                TbLogo.Text = openFileDialog.FileName;
            }
        }

        private void TbLogo_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            Common.OemInfo.LogoPath = TbLogo.Text;
        }

        private void TbManufacturer_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            Common.OemInfo.Manufacturer = TbManufacturer.Text;
        }

        private void TbModel_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            Common.OemInfo.Model = TbModel.Text;
        }

        private void TbSupportHours_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            Common.OemInfo.SupportHours = TbSupportHours.Text;
        }

        private void TbPhone_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            Common.OemInfo.SupportPhone = TbPhone.Text;
        }

        private void TbUrl_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            Common.OemInfo.SupportURL = TbUrl.Text;
        }

        private void User_Switch(object sender, RoutedEventArgs e)
        {
            Common.DeploymentInfo.UseUserInfo = ToggleUser.IsChecked.Value;
            Debug.WriteLine(ToggleUser.IsChecked.Value ? "Use User Info: True" : "Use User Info: False");
        }

        private void Oem_Switch(object sender, RoutedEventArgs e)
        {
            Common.OemInfo.UseOemInfo = ToggleOem.IsChecked.Value;
        }

        private void Import_OnClicked(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Dive Configuration (*.xml)|*.xml";
            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(File.ReadAllText(openFileDialog.FileName));

                    XmlNodeList imageNames = doc.DocumentElement.SelectNodes("/settings/AdministratorAccount");

                    foreach (XmlNode node in imageNames)
                    {
                        Common.DeploymentInfo.Username = node.SelectSingleNode("Username").InnerText;
                        Common.DeploymentInfo.Password = node.SelectSingleNode("Password").InnerText;
                    }

                    TbUser.Text = Common.DeploymentInfo.Username;
                    TbPassword.Text = Common.DeploymentInfo.Password;
                    if (Common.DeploymentInfo.Username != "" || Common.DeploymentInfo.Password != "")
                    {
                        Common.DeploymentInfo.UseUserInfo = true;
                        ToggleUser.IsChecked = true;
                    }
                    else
                    {
                        Common.DeploymentInfo.UseUserInfo = false;
                        ToggleUser.IsChecked = false;
                    }
                }
                catch (Exception ex) { Console.WriteLine(ex.Message); }

                try
                {
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(File.ReadAllText(openFileDialog.FileName));

                    XmlNodeList imageNames = doc.DocumentElement.SelectNodes("/settings/OEMSupport");

                    foreach (XmlNode node in imageNames)
                    {
                        string OemLogoFileName = node.SelectSingleNode("OEMLogo").InnerText;
                        string OemLogoPath = Path.Combine(Path.GetDirectoryName(openFileDialog.FileName), OemLogoFileName);
                        if (OemLogoFileName != "")
                        {
                            if (File.Exists(OemLogoPath))
                            {
                                Common.OemInfo.LogoPath = OemLogoPath;
                            }
                            else
                            {
                                RootSnackbar.Appearance = ControlAppearance.Caution;
                                RootSnackbar.Icon = SymbolRegular.ImageOff24;
                                RootSnackbar.Show("An error has occurred.", $"OEM logo does not exists in the same location as the configuration.");
                            }
                        }

                        Common.OemInfo.Manufacturer = node.SelectSingleNode("Manufacturer").InnerText;
                        Common.OemInfo.Model = node.SelectSingleNode("Model").InnerText;
                        Common.OemInfo.SupportHours = node.SelectSingleNode("SupportHours").InnerText;
                        Common.OemInfo.SupportPhone = node.SelectSingleNode("SupportNo").InnerText;
                        Common.OemInfo.SupportURL = node.SelectSingleNode("Homepage").InnerText;
                    }

                    TbLogo.Text = Common.OemInfo.LogoPath;
                    TbManufacturer.Text = Common.OemInfo.Manufacturer;
                    TbModel.Text = Common.OemInfo.Model;
                    TbSupportHours.Text = Common.OemInfo.SupportHours;
                    TbPhone.Text = Common.OemInfo.SupportPhone;
                    TbUrl.Text = Common.OemInfo.SupportURL;
                    if (Common.OemInfo.LogoPath != "" 
                        || Common.OemInfo.Manufacturer != ""
                        || Common.OemInfo.Model != ""
                        || Common.OemInfo.SupportHours != ""
                        || Common.OemInfo.SupportPhone != ""
                        || Common.OemInfo.SupportURL != "")
                    {
                        Common.OemInfo.UseOemInfo = true;
                        ToggleOem.IsChecked = true;
                    }
                    else
                    {
                        Common.OemInfo.UseOemInfo = false;
                        ToggleOem.IsChecked = false;
                    }
                }
                catch (Exception ex) { Console.WriteLine(ex.Message); }
            }
            
        }

        private void Export_OnClicked(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Dive Configuration (*.xml)|*.xml";
            if (saveFileDialog.ShowDialog() == true)
            {
                string FileNameOfOemLogo = Path.GetFileName(TbLogo.Text);
                if (FileNameOfOemLogo != "")
                {
                    try
                    {
                        File.Copy(TbLogo.Text,
                            Path.Combine(Path.GetDirectoryName(saveFileDialog.FileName), FileNameOfOemLogo));
                    }
                    catch
                    {
                        RootSnackbar.Appearance = ControlAppearance.Caution;
                        RootSnackbar.Icon = SymbolRegular.ImageOff24;
                        RootSnackbar.Show("An error has occurred.", $"Cannot copy OEM logo to the location of the exported configuration.");
                    }
                }

                var settings = new XMLSetting.settings
                {
                    AdministratorAccount = new XMLSetting.settingsAdministratorAccount()
                    {
                        Username = Common.DeploymentInfo.Username,
                        Password = Common.DeploymentInfo.Password,
                    },
                    OEMSupport = new XMLSetting.settingsOEMSupport()
                    {
                        OEMLogo = FileNameOfOemLogo,
                        Manufacturer = Common.OemInfo.Manufacturer,
                        Model = Common.OemInfo.Model,
                        SupportHours = Common.OemInfo.SupportHours,
                        SupportNo = Common.OemInfo.SupportPhone,
                        Homepage = Common.OemInfo.SupportURL
                    }
                };

                XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                ns.Add("", "");

                //Create the serializer
                XmlSerializer slz = new XmlSerializer(typeof(XMLSetting.settings));
                using (TextWriter writer = new StreamWriter(saveFileDialog.FileName))
                {
                    slz.Serialize(writer, settings);
                }
            }
        }
    }
}
