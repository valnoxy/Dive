using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using deployaUI.Common;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Path = System.IO.Path;
using Wpf.Ui.Common;
using System.Xml;
using deployaUI.Pages.Extras;
using Wpf.Ui.Controls;

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
            var openFileDialog = new OpenFileDialog
            {
                Filter = "OEM Logo (*.bmp)|*.bmp"
            };

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
            if (ToggleUser.IsChecked.Value)
            {
                Debug.Write("Use User Information in Unattend: ");
                Debug.Write("Enabled\n", true, ConsoleColor.DarkYellow);
            }
            else
            {
                Debug.Write("Use User Information in Unattend: ");
                Debug.Write("Disabled\n", true, ConsoleColor.DarkYellow);
            }
        }

        private void Oem_Switch(object sender, RoutedEventArgs e)
        {
            Common.OemInfo.UseOemInfo = ToggleOem.IsChecked.Value;
            if (ToggleOem.IsChecked.Value)
            {
                Debug.Write("Use OEM Information in Unattend: ");
                Debug.Write("Enabled\n", true, ConsoleColor.DarkYellow);
            }
            else
            {
                Debug.Write("Use OEM Information in Unattend: ");
                Debug.Write("Disabled\n", true, ConsoleColor.DarkYellow);
            }
        }

        private void Import_OnClicked(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Dive Configuration (*.xml)|*.xml"
            };
            if (openFileDialog.ShowDialog() != true) return;
            
            try
            {
                var doc = new XmlDocument();
                doc.LoadXml(File.ReadAllText(openFileDialog.FileName));
                var imageNames = doc.DocumentElement!.SelectNodes("/settings/AdministratorAccount");

                foreach (XmlNode node in imageNames!)
                {
                    Common.DeploymentInfo.Username = node.SelectSingleNode("Username")!.InnerText;
                    Common.DeploymentInfo.Password = node.SelectSingleNode("Password")!.InnerText;
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
            catch (Exception ex) { Debug.WriteLine(ex.Message, ConsoleColor.Red); }

            try
            {
                var doc = new XmlDocument();
                doc.LoadXml(File.ReadAllText(openFileDialog.FileName));

                var imageNames = doc.DocumentElement!.SelectNodes("/settings/OEMSupport");

                foreach (XmlNode node in imageNames!)
                {
                    var oemLogoFileName = node.SelectSingleNode("OEMLogo")!.InnerText;
                    var oemLogoPath = Path.Combine(Path.GetDirectoryName(openFileDialog.FileName)!, oemLogoFileName);
                    if (oemLogoFileName != "")
                    {
                        if (File.Exists(oemLogoPath))
                        {
                            Common.OemInfo.LogoPath = oemLogoPath;
                        }
                        else
                        {
                            RootSnackbar.Appearance = ControlAppearance.Caution;
                            RootSnackbar.Icon = SymbolRegular.ImageOff24;
                            RootSnackbar.Show("An error has occurred.", $"OEM logo does not exists in the same location as the configuration.");
                        }
                    }

                    Common.OemInfo.Manufacturer = node.SelectSingleNode("Manufacturer")!.InnerText;
                    Common.OemInfo.Model = node.SelectSingleNode("Model")!.InnerText;
                    Common.OemInfo.SupportHours = node.SelectSingleNode("SupportHours")!.InnerText;
                    Common.OemInfo.SupportPhone = node.SelectSingleNode("SupportNo")!.InnerText;
                    Common.OemInfo.SupportURL = node.SelectSingleNode("Homepage")!.InnerText;
                }

                TbLogo.Text = Common.OemInfo.LogoPath;
                TbManufacturer.Text = Common.OemInfo.Manufacturer;
                TbModel.Text = Common.OemInfo.Model;
                TbSupportHours.Text = Common.OemInfo.SupportHours;
                TbPhone.Text = Common.OemInfo.SupportPhone;
                TbUrl.Text = Common.OemInfo.SupportURL;

                Common.OemInfo.UseOemInfo = Common.OemInfo.LogoPath != "" || 
                                            Common.OemInfo.Manufacturer != "" || 
                                            Common.OemInfo.Model != "" || 
                                            Common.OemInfo.SupportHours != "" || 
                                            Common.OemInfo.SupportPhone != "" || 
                                            Common.OemInfo.SupportURL != "";
                ToggleOem.IsChecked = Common.OemInfo.UseOemInfo;
                Debug.WriteLine("Successfully imported settings from file.");
            }
            catch (Exception ex) { Debug.WriteLine(ex.Message, ConsoleColor.DarkRed); }
        }

        private void Export_OnClicked(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Dive Configuration (*.xml)|*.xml"
            };

            if (saveFileDialog.ShowDialog() != true) return;
            var fileNameOfOemLogo = Path.GetFileName(TbLogo.Text);
            if (fileNameOfOemLogo != "")
            {
                try
                {
                    File.Copy(TbLogo.Text,
                        Path.Combine(Path.GetDirectoryName(saveFileDialog.FileName)!, fileNameOfOemLogo));
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
                    OEMLogo = fileNameOfOemLogo,
                    Manufacturer = Common.OemInfo.Manufacturer,
                    Model = Common.OemInfo.Model,
                    SupportHours = Common.OemInfo.SupportHours,
                    SupportNo = Common.OemInfo.SupportPhone,
                    Homepage = Common.OemInfo.SupportURL
                }
            };

            var ns = new XmlSerializerNamespaces();
            ns.Add("", "");

            // Create the serializer
            var slz = new XmlSerializer(typeof(XMLSetting.settings));
            using TextWriter writer = new StreamWriter(saveFileDialog.FileName);
            slz.Serialize(writer, settings);

            Debug.WriteLine("Successfully exported settings to file.");
        }

        private void Source_OpenFolderClick(object sender, RoutedEventArgs e)
        {
            using var dialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "Select the directory with your drivers.",
                UseDescriptionForTitle = true,
                SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyComputer),
                ShowNewFolderButton = false
            };

            if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
            TbDrvPath.Text = dialog.SelectedPath;
            try
            {
                var infFiles = Directory.GetFiles(dialog.SelectedPath, "*.inf", SearchOption.AllDirectories)
                    .Select(Path.GetFullPath)
                    .ToList();
                switch (infFiles.Count)
                {
                    case > 0:
                        if (File.Exists("X:\\Windows\\System32\\wpeutil.exe"))
                        {
                            var driverWindow = new LoadDriversLiveSystem(infFiles);
                            driverWindow.ShowDialog();
                        }
                        Common.Debug.Write("Found");
                        Common.Debug.Write(infFiles.Count.ToString(), true, ConsoleColor.DarkYellow);
                        Common.Debug.Write("drivers.", true);
                        Common.ApplyDetails.DriverList = infFiles;
                        break;
                    case 0:
                        RootSnackbar.Appearance = ControlAppearance.Danger;
                        RootSnackbar.Icon = SymbolRegular.Settings32;
                        RootSnackbar.Show("An error has occurred.", $"No inf files found in the selected directory.");
                        Common.Debug.Write("No drivers was found in the selected directory.");
                        Common.ApplyDetails.DriverList = null;
                        break;
                }
            }
            catch (Exception ex) { Debug.WriteLine(ex.Message, ConsoleColor.Red); }
        }

        private void MoreSettings_OnClicked(object sender, RoutedEventArgs e)
        {
            var ucWindow = new UnattendConfiguration();
            ucWindow.ShowDialog();
        }
    }
}
