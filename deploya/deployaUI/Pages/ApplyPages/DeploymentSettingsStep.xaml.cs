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

        private void User_OnChecked(object sender, RoutedEventArgs e)
        {
            Common.DeploymentInfo.UseUserInfo = ToggleUser.IsChecked.Value;
        }
        private void Oem_OnChecked(object sender, RoutedEventArgs e)
        {
            Common.OemInfo.UseOemInfo = ToggleOem.IsChecked.Value;
        }
    }
}
