using Dive.UI.Common;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Serialization;
using System.Xml;
using Microsoft.Win32;

namespace Dive.UI.Pages.Extras.UnattendConfigurationPages
{
    /// <summary>
    /// Interaktionslogik für DeploymentConfigurationPage.xaml
    /// </summary>
    public partial class DeploymentConfigurationPage : UserControl
    {
        public DeploymentConfigurationPage()
        {
            InitializeComponent();

            if (Common.ApplyDetails.NTVersion == "10.0" && Common.ApplyDetails.Build >= 17134)
                SModeSwitch.IsEnabled = true;
            else
                SModeSwitch.IsEnabled = false;

            DiveToRecovery.IsEnabled = Common.ApplyDetails.NTVersion is "10.0" or "6.3" or "6.2";

            SModeSwitch.IsChecked = Common.DeploymentOption.UseSMode;
            CopyProfileToggle.IsChecked = Common.DeploymentOption.UseCopyProfile;
            DiveToRecovery.IsChecked = Common.DeploymentOption.AddDiveToWinRE;
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
                DeploymentInfo.Username = TbUser.Text;
            }
        }

        private void TbPassword_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            DeploymentInfo.Password = TbPassword.Text;
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
            OemInfo.LogoPath = TbLogo.Text;
        }

        private void TbManufacturer_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            OemInfo.Manufacturer = TbManufacturer.Text;
        }

        private void TbModel_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            OemInfo.Model = TbModel.Text;
        }

        private void TbSupportHours_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            OemInfo.SupportHours = TbSupportHours.Text;
        }

        private void TbPhone_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            OemInfo.SupportPhone = TbPhone.Text;
        }

        private void TbUrl_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            OemInfo.SupportURL = TbUrl.Text;
        }

        private void User_Switch(object sender, RoutedEventArgs e)
        {
            DeploymentInfo.UseUserInfo = ToggleUser.IsChecked.Value;
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
            OemInfo.UseOemInfo = ToggleOem.IsChecked.Value;
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
                        ApplyDetails.DriverList = infFiles;
                        break;
                    case 0:
                        Debug.Write("No drivers was found in the selected directory.");
                        ApplyDetails.DriverList = null;
                        break;
                }
            }
            catch (Exception ex) { Debug.WriteLine(ex.Message, ConsoleColor.Red); }
        }

        private void CodeIntegrityMode_Click(object sender, RoutedEventArgs e)
        {
            if (SModeSwitch.IsChecked == true)
            {
                Common.DeploymentOption.UseSMode = true;
                Common.Debug.Write("Using S Mode in Unattend: ");
                Common.Debug.Write("Enabled\n", true, ConsoleColor.DarkYellow);
            }
            else
            {
                Common.DeploymentOption.UseSMode = false;
                Common.Debug.Write("Using S Mode in Unattend: ");
                Common.Debug.Write("Disabled\n", true, ConsoleColor.DarkYellow);
            }
        }

        private void CopyProfileToggle_OnClick(object sender, RoutedEventArgs e)
        {
            if (CopyProfileToggle.IsChecked == true)
            {
                Common.DeploymentOption.UseCopyProfile = true;
                Common.Debug.Write("Using CopyProfile in Unattend: ");
                Common.Debug.Write("Enabled\n", true, ConsoleColor.DarkYellow);
            }
            else
            {
                Common.DeploymentOption.UseCopyProfile = false;
                Common.Debug.Write("Using CopyProfile in Unattend: ");
                Common.Debug.Write("Disabled\n", true, ConsoleColor.DarkYellow);
            }
        }

        private void DiveToRecovery_OnClick(object sender, RoutedEventArgs e)
        {
            if (DiveToRecovery.IsChecked == true)
            {
                Common.DeploymentOption.AddDiveToWinRE = true;
                Common.Debug.Write("Implement Dive into Windows RE image: ");
                Common.Debug.Write("Enabled\n", true, ConsoleColor.DarkYellow);
            }
            else
            {
                Common.DeploymentOption.AddDiveToWinRE = false;
                Common.Debug.Write("Implement Dive into Windows RE image: ");
                Common.Debug.Write("Disabled\n", true, ConsoleColor.DarkYellow);
            }
        }
    }
}
