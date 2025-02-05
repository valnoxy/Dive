using Dive.UI.Common;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Dive.UI.Common.UserInterface;
using Microsoft.Win32;

namespace Dive.UI.Pages.Extras.UnattendConfigurationPages
{
    /// <summary>
    /// Interaktionslogik für DeploymentConfigurationPage.xaml
    /// </summary>
    public partial class DeploymentConfigurationPage
    {
        private static readonly ApplyDetails ApplyDetailsInstance = ApplyDetails.Instance;
        private static readonly DeploymentInfo DeploymentInfoInstance = DeploymentInfo.Instance;
        private static readonly DeploymentOption DeploymentOptionInstance = DeploymentOption.Instance;
        private static readonly OemInfo OemInfoInstance = OemInfo.Instance;

        public DeploymentConfigurationPage()
        {
            InitializeComponent();

            DeploymentInfo.Instance.SettingChanged += SettingChanged;
            OemInfo.Instance.SettingChanged += SettingChanged;

            SModeSwitch.IsEnabled = ApplyDetailsInstance is { NTVersion: "10.0", Build: >= 17134 };
            DiveToRecovery.IsEnabled = ApplyDetailsInstance.NTVersion is "10.0" or "6.3" or "6.2";
            UpdateUiSettings();
        }

        private void SettingChanged(object sender, SettingChangedEventArgs e) => UpdateUiSettings();
        private void UpdateUiSettings()
        {
            SModeSwitch.IsChecked = DeploymentOptionInstance.UseSMode;
            CopyProfileToggle.IsChecked = DeploymentOptionInstance.UseCopyProfile;
            DiveToRecovery.IsChecked = DeploymentOptionInstance.AddDiveToWinRE;

            TbUser.Text = DeploymentInfoInstance.Username;
            TbPassword.Text = DeploymentInfoInstance.Password;

            TbLogo.Text = OemInfoInstance.LogoPath;
            TbManufacturer.Text = OemInfoInstance.Manufacturer;
            TbModel.Text = OemInfoInstance.Model;
            TbSupportHours.Text = OemInfoInstance.SupportHours;
            TbPhone.Text = OemInfoInstance.SupportPhone;
            TbUrl.Text = OemInfoInstance.SupportURL;

            ToggleUser.IsChecked = DeploymentInfoInstance.UseUserInfo;
            ToggleOem.IsChecked = OemInfoInstance.UseOemInfo;
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
                DeploymentInfoInstance.Username = TbUser.Text;
            }
        }

        private void TbPassword_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            DeploymentInfoInstance.Password = TbPassword.Text;
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
            OemInfoInstance.LogoPath = TbLogo.Text;
        }

        private void TbManufacturer_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            OemInfoInstance.Manufacturer = TbManufacturer.Text;
        }

        private void TbModel_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            OemInfoInstance.Model = TbModel.Text;
        }

        private void TbSupportHours_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            OemInfoInstance.SupportHours = TbSupportHours.Text;
        }

        private void TbPhone_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            OemInfoInstance.SupportPhone = TbPhone.Text;
        }

        private void TbUrl_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            OemInfoInstance.SupportURL = TbUrl.Text;
        }

        private void User_Switch(object sender, RoutedEventArgs e)
        {
            DeploymentInfoInstance.UseUserInfo = ToggleUser.IsChecked.Value;
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
            OemInfoInstance.UseOemInfo = ToggleOem.IsChecked.Value;
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
                        Debug.Write("Found");
                        Debug.Write(infFiles.Count.ToString(), true, ConsoleColor.DarkYellow);
                        Debug.Write("drivers.", true);
                        ApplyDetailsInstance.DriverList = infFiles;
                        break;
                    case 0:
                        Debug.Write("No drivers was found in the selected directory.");
                        ApplyDetailsInstance.DriverList = null;
                        break;
                }
            }
            catch (Exception ex) { Debug.WriteLine(ex.Message, ConsoleColor.Red); }
        }

        private void CodeIntegrityMode_Click(object sender, RoutedEventArgs e)
        {
            if (SModeSwitch.IsChecked == true)
            {
                DeploymentOptionInstance.UseSMode = true;
                Debug.Write("Using S Mode in Unattend: ");
                Debug.Write("Enabled\n", true, ConsoleColor.DarkYellow);
            }
            else
            {
                DeploymentOptionInstance.UseSMode = false;
                Debug.Write("Using S Mode in Unattend: ");
                Debug.Write("Disabled\n", true, ConsoleColor.DarkYellow);
            }
        }

        private void CopyProfileToggle_OnClick(object sender, RoutedEventArgs e)
        {
            if (CopyProfileToggle.IsChecked == true)
            {
                DeploymentOptionInstance.UseCopyProfile = true;
                Debug.Write("Using CopyProfile in Unattend: ");
                Debug.Write("Enabled\n", true, ConsoleColor.DarkYellow);
            }
            else
            {
                DeploymentOptionInstance.UseCopyProfile = false;
                Debug.Write("Using CopyProfile in Unattend: ");
                Debug.Write("Disabled\n", true, ConsoleColor.DarkYellow);
            }
        }

        private void DiveToRecovery_OnClick(object sender, RoutedEventArgs e)
        {
            if (DiveToRecovery.IsChecked == true)
            {
                DeploymentOptionInstance.AddDiveToWinRE = true;
                Debug.Write("Implement Dive into Windows RE image: ");
                Debug.Write("Enabled\n", true, ConsoleColor.DarkYellow);
            }
            else
            {
                DeploymentOptionInstance.AddDiveToWinRE = false;
                Debug.Write("Implement Dive into Windows RE image: ");
                Debug.Write("Disabled\n", true, ConsoleColor.DarkYellow);
            }
        }
    }
}
