using System;
using System.Windows;
using System.Windows.Controls;
using Dive.UI.Common;

namespace Dive.UI.Pages.Extras.UnattendConfigurationPages
{
    /// <summary>
    /// Interaktionslogik für OOBEConfigurationPage.xaml
    /// </summary>
    public partial class OOBEConfigurationPage : UserControl
    {
        public OOBEConfigurationPage()
        {
            InitializeComponent();

            NetworkLocationDropDown.Items.Add("Default");
            NetworkLocationDropDown.Items.Add("Home");
            NetworkLocationDropDown.Items.Add("Work");
            
            if (string.IsNullOrEmpty(OutOfBoxExperienceInfo.NetworkLocation))
                NetworkLocationDropDown.SelectedIndex = 0;
            else
                NetworkLocationDropDown.SelectedIndex = OutOfBoxExperienceInfo.NetworkLocation switch
                {
                    "Home" => 1,
                    "Work" => 2,
                    _ => NetworkLocationDropDown.SelectedIndex
                };

            TbUser.Text = DeviceInfo.RegisteredOwner;
            TbOrganization.Text = DeviceInfo.RegisteredOrganization;
            ToggleDomain.IsChecked = DomainInfo.UseDomainInfo;
            TbDomainUser.Text = DomainInfo.UserName;
            TbDomainPassword.Text = DomainInfo.Password;
            TbDomain.Text = DomainInfo.Domain;
            ToggleOobeSetup.IsChecked = OutOfBoxExperienceInfo.UseOOBEInfo;
            HideEulaPageSwitch.IsChecked = OutOfBoxExperienceInfo.HideEULAPage;
            HideOemRegistrationPageSwitch.IsChecked = OutOfBoxExperienceInfo.HideOEMRegistrationScreen;
            HideOnlineAccountPageSwitch.IsChecked = OutOfBoxExperienceInfo.HideOnlineAccountScreens;
            HideWirelessSetupPageSwitch.IsChecked = OutOfBoxExperienceInfo.HideWirelessSetupInOOBE;
            SkipMachineOOBESwitch.IsChecked = OutOfBoxExperienceInfo.SkipMachineOOBE;
            SkipUserOOBESwitch.IsChecked = OutOfBoxExperienceInfo.SkipUserOOBE;
            ProductKeyTextBox.Text = DeviceInfo.ProductKey;
            // TODO: Add Device Name (already included in the XML Builder)
        }

        private void TbUser_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            DeviceInfo.RegisteredOwner = TbUser.Text;
        }

        private void TbOrganization_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            DeviceInfo.RegisteredOrganization = TbOrganization.Text;
        }

        private void TbDomainUser_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            DomainInfo.UserName = TbDomainUser.Text;
        }

        private void TbDomainPassword_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            DomainInfo.Password = TbDomainPassword.Text;
        }

        private void TbDomain_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            DomainInfo.Domain = TbDomain.Text;
        }

        private void ToggleDomain_OnClick(object sender, RoutedEventArgs e)
        {
            DomainInfo.UseDomainInfo = ToggleDomain.IsChecked.Value;
            if (ToggleDomain.IsChecked.Value)
            {
                Debug.Write("Use Domain Information in Unattend: ");
                Debug.Write("Enabled\n", true, ConsoleColor.DarkYellow);
            }
            else
            {
                Debug.Write("Use Domain Information in Unattend: ");
                Debug.Write("Disabled\n", true, ConsoleColor.DarkYellow);
            }
        }

        private void ToggleOobeSetup_OnClick(object sender, RoutedEventArgs e)
        {
            OutOfBoxExperienceInfo.UseOOBEInfo = ToggleOobeSetup.IsChecked.Value;
            if (ToggleOobeSetup.IsChecked.Value)
            {
                Debug.Write("Use OOBE Information in Unattend: ");
                Debug.Write("Enabled\n", true, ConsoleColor.DarkYellow);
            }
            else
            {
                Debug.Write("Use OOBE Information in Unattend: ");
                Debug.Write("Disabled\n", true, ConsoleColor.DarkYellow);
            }
        }

        private void HideEulaPageSwitch_OnClick(object sender, RoutedEventArgs e)
        {
            OutOfBoxExperienceInfo.HideEULAPage = HideEulaPageSwitch.IsChecked.Value;
        }

        private void HideOemRegistrationPageSwitch_OnClick(object sender, RoutedEventArgs e)
        {
            OutOfBoxExperienceInfo.HideOEMRegistrationScreen = HideOemRegistrationPageSwitch.IsChecked.Value;
        }

        private void HideOnlineAccountPageSwitch_OnClick(object sender, RoutedEventArgs e)
        {
            OutOfBoxExperienceInfo.HideOnlineAccountScreens = HideOnlineAccountPageSwitch.IsChecked.Value;
        }

        private void HideWirelessSetupPageSwitch_OnClick(object sender, RoutedEventArgs e)
        {
            OutOfBoxExperienceInfo.HideWirelessSetupInOOBE = HideWirelessSetupPageSwitch.IsChecked.Value;
        }

        private void NetworkLocationDropDown_OnSelected(object sender, RoutedEventArgs e)
        {
            OutOfBoxExperienceInfo.NetworkLocation = NetworkLocationDropDown.SelectedIndex switch
            {
                0 => "",
                1 => "Home",
                2 => "Work",
                _ => OutOfBoxExperienceInfo.NetworkLocation
            };
            Debug.Write("Selected Network Location in Unattend: ");
            Debug.Write($"{OutOfBoxExperienceInfo.NetworkLocation}\n", true, ConsoleColor.DarkYellow);
        }

        private void SkipMachineOOBESwitch_OnClick(object sender, RoutedEventArgs e)
        {
            OutOfBoxExperienceInfo.SkipMachineOOBE = SkipMachineOOBESwitch.IsChecked.Value;
        }

        private void SkipUserOOBESwitch_OnClick(object sender, RoutedEventArgs e)
        {
            OutOfBoxExperienceInfo.SkipUserOOBE = SkipUserOOBESwitch.IsChecked.Value;
        }

        private void ProductKeyTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            DeviceInfo.ProductKey = ProductKeyTextBox.Text;
        }
    }
}
