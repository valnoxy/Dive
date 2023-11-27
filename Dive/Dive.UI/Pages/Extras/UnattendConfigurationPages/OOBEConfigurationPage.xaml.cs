using System;
using System.Windows;
using System.Windows.Controls;
using Dive.UI.Common;
using TimeZone = Dive.UI.Common.Configuration.TimeZone;

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

            TimeZoneDropDown.Items.Add("(UTC-12:00) International Date Line West");
            TimeZoneDropDown.Items.Add("(UTC-11:00) Coordinated Universal Time-11");
            TimeZoneDropDown.Items.Add("(UTC-10:00) Aleutian Islands");
            TimeZoneDropDown.Items.Add("(UTC-10:00) Hawaii");
            TimeZoneDropDown.Items.Add("(UTC-09:30) Marquesas Islands");
            TimeZoneDropDown.Items.Add("(UTC-09:00) Alaska");
            TimeZoneDropDown.Items.Add("(UTC-09:00) Coordinated Universal Time-09");
            TimeZoneDropDown.Items.Add("(UTC-08:00) Baja California");
            TimeZoneDropDown.Items.Add("(UTC-08:00) Coordinated Universal Time-08");
            TimeZoneDropDown.Items.Add("(UTC-08:00) Pacific Time (US & Canada)");
            TimeZoneDropDown.Items.Add("(UTC-07:00) Arizona");
            TimeZoneDropDown.Items.Add("(UTC-07:00) La Paz, Mazatlan");
            TimeZoneDropDown.Items.Add("(UTC-07:00) Mountain Time (US & Canada)");
            TimeZoneDropDown.Items.Add("(UTC-07:00) Yukon");
            TimeZoneDropDown.Items.Add("(UTC-06:00) Central America");
            TimeZoneDropDown.Items.Add("(UTC-06:00) Central Time (US & Canada)");
            TimeZoneDropDown.Items.Add("(UTC-06:00) Easter Island");
            TimeZoneDropDown.Items.Add("(UTC-06:00) Guadalajara, Mexico City, Monterrey");
            TimeZoneDropDown.Items.Add("(UTC-06:00) Saskatchewan");
            TimeZoneDropDown.Items.Add("(UTC-05:00) Bogota, Lima, Quito, Rio Branco");
            TimeZoneDropDown.Items.Add("(UTC-05:00) Chetumal");
            TimeZoneDropDown.Items.Add("(UTC-05:00) Eastern Time (US & Canada)");
            TimeZoneDropDown.Items.Add("(UTC-05:00) Haiti");
            TimeZoneDropDown.Items.Add("(UTC-05:00) Havana");
            TimeZoneDropDown.Items.Add("(UTC-05:00) Indiana (East)");
            TimeZoneDropDown.Items.Add("(UTC-05:00) Turks and Caicos");
            TimeZoneDropDown.Items.Add("(UTC-04:00) Asuncion");
            TimeZoneDropDown.Items.Add("(UTC-04:00) Atlantic Time (Canada)");
            TimeZoneDropDown.Items.Add("(UTC-04:00) Caracas");
            TimeZoneDropDown.Items.Add("(UTC-04:00) Cuiaba");
            TimeZoneDropDown.Items.Add("(UTC-04:00) Georgetown, La Paz, Manaus, San Juan");
            TimeZoneDropDown.Items.Add("(UTC-04:00) Santiago");
            TimeZoneDropDown.Items.Add("(UTC-03:30) Newfoundland");
            TimeZoneDropDown.Items.Add("(UTC-03:00) Araguaina");
            TimeZoneDropDown.Items.Add("(UTC-03:00) Brasilia");
            TimeZoneDropDown.Items.Add("(UTC-03:00) Cayenne, Fortaleza");
            TimeZoneDropDown.Items.Add("(UTC-03:00) City of Buenos Aires");
            TimeZoneDropDown.Items.Add("(UTC-03:00) Greenland");
            TimeZoneDropDown.Items.Add("(UTC-03:00) Montevideo");
            TimeZoneDropDown.Items.Add("(UTC-03:00) Punta Arenas");
            TimeZoneDropDown.Items.Add("(UTC-03:00) Saint Pierre and Miquelon");
            TimeZoneDropDown.Items.Add("(UTC-03:00) Salvador");
            TimeZoneDropDown.Items.Add("(UTC-02:00) Coordinated Universal Time-02");
            TimeZoneDropDown.Items.Add("(UTC-01:00) Azores");
            TimeZoneDropDown.Items.Add("(UTC-01:00) Cabo Verde Is.");
            TimeZoneDropDown.Items.Add("(UTC) Coordinated Universal Time");
            TimeZoneDropDown.Items.Add("(UTC+00:00) Dublin, Edinburgh, Lisbon, London");
            TimeZoneDropDown.Items.Add("(UTC+00:00) Monrovia, Reykjavik");
            TimeZoneDropDown.Items.Add("(UTC+00:00) Sao Tome");
            TimeZoneDropDown.Items.Add("(UTC+01:00) Casablanca");
            TimeZoneDropDown.Items.Add("(UTC+01:00) Amsterdam, Berlin, Bern, Rome, Stockholm, Vienna");
            TimeZoneDropDown.Items.Add("(UTC+01:00) Belgrade, Bratislava, Budapest, Ljubljana, Prague");
            TimeZoneDropDown.Items.Add("(UTC+01:00) Brussels, Copenhagen, Madrid, Paris");
            TimeZoneDropDown.Items.Add("(UTC+01:00) Sarajevo, Skopje, Warsaw, Zagreb");
            TimeZoneDropDown.Items.Add("(UTC+01:00) West Central Africa");
            TimeZoneDropDown.Items.Add("(UTC+02:00) Athens, Bucharest");
            TimeZoneDropDown.Items.Add("(UTC+02:00) Beirut");
            TimeZoneDropDown.Items.Add("(UTC+02:00) Cairo");
            TimeZoneDropDown.Items.Add("(UTC+02:00) Chisinau");
            TimeZoneDropDown.Items.Add("(UTC+02:00) Damascus");
            TimeZoneDropDown.Items.Add("(UTC+02:00) Gaza, Hebron");
            TimeZoneDropDown.Items.Add("(UTC+02:00) Harare, Pretoria");
            TimeZoneDropDown.Items.Add("(UTC+02:00) Helsinki, Kyiv, Riga, Sofia, Tallinn, Vilnius");
            TimeZoneDropDown.Items.Add("(UTC+02:00) Jerusalem");
            TimeZoneDropDown.Items.Add("(UTC+02:00) Juba");
            TimeZoneDropDown.Items.Add("(UTC+02:00) Kaliningrad");
            TimeZoneDropDown.Items.Add("(UTC+02:00) Khartoum");
            TimeZoneDropDown.Items.Add("(UTC+02:00) Tripoli");
            TimeZoneDropDown.Items.Add("(UTC+02:00) Windhoek");
            TimeZoneDropDown.Items.Add("(UTC+03:00) Amman");
            TimeZoneDropDown.Items.Add("(UTC+03:00) Baghdad");
            TimeZoneDropDown.Items.Add("(UTC+03:00) Istanbul");
            TimeZoneDropDown.Items.Add("(UTC+03:00) Kuwait, Riyadh");
            TimeZoneDropDown.Items.Add("(UTC+03:00) Minsk");
            TimeZoneDropDown.Items.Add("(UTC+03:00) Moscow, St. Petersburg");
            TimeZoneDropDown.Items.Add("(UTC+03:00) Nairobi");
            TimeZoneDropDown.Items.Add("(UTC+03:00) Volgograd");
            TimeZoneDropDown.Items.Add("(UTC+03:30) Tehran");
            TimeZoneDropDown.Items.Add("(UTC+04:00) Abu Dhabi, Muscat");
            TimeZoneDropDown.Items.Add("(UTC+04:00) Astrakhan, Ulyanovsk");
            TimeZoneDropDown.Items.Add("(UTC+04:00) Baku");
            TimeZoneDropDown.Items.Add("(UTC+04:00) Izhevsk, Samara");
            TimeZoneDropDown.Items.Add("(UTC+04:00) Port Louis");
            TimeZoneDropDown.Items.Add("(UTC+04:00) Saratov");
            TimeZoneDropDown.Items.Add("(UTC+04:00) Tbilisi");
            TimeZoneDropDown.Items.Add("(UTC+04:00) Yerevan");
            TimeZoneDropDown.Items.Add("(UTC+04:30) Kabul");
            TimeZoneDropDown.Items.Add("(UTC+05:00) Ashgabat, Tashkent");
            TimeZoneDropDown.Items.Add("(UTC+05:00) Ekaterinburg");
            TimeZoneDropDown.Items.Add("(UTC+05:00) Islamabad, Karachi");
            TimeZoneDropDown.Items.Add("(UTC+05:00) Qyzylorda");
            TimeZoneDropDown.Items.Add("(UTC+05:30) Chennai, Kolkata, Mumbai, New Delhi");
            TimeZoneDropDown.Items.Add("(UTC+05:30) Sri Jayawardenepura");
            TimeZoneDropDown.Items.Add("(UTC+05:45) Kathmandu");
            TimeZoneDropDown.Items.Add("(UTC+06:00) Astana");
            TimeZoneDropDown.Items.Add("(UTC+06:00) Dhaka");
            TimeZoneDropDown.Items.Add("(UTC+06:00) Omsk");
            TimeZoneDropDown.Items.Add("(UTC+06:30) Yangon (Rangoon)");
            TimeZoneDropDown.Items.Add("(UTC+07:00) Bangkok, Hanoi, Jakarta");
            TimeZoneDropDown.Items.Add("(UTC+07:00) Barnaul, Gorno-Altaysk");
            TimeZoneDropDown.Items.Add("(UTC+07:00) Hovd");
            TimeZoneDropDown.Items.Add("(UTC+07:00) Krasnoyarsk");
            TimeZoneDropDown.Items.Add("(UTC+07:00) Novosibirsk");
            TimeZoneDropDown.Items.Add("(UTC+07:00) Tomsk");
            TimeZoneDropDown.Items.Add("(UTC+08:00) Beijing, Chongqing, Hong Kong, Urumqi");
            TimeZoneDropDown.Items.Add("(UTC+08:00) Irkutsk");
            TimeZoneDropDown.Items.Add("(UTC+08:00) Kuala Lumpur, Singapore");
            TimeZoneDropDown.Items.Add("(UTC+08:00) Perth");
            TimeZoneDropDown.Items.Add("(UTC+08:00) Taipei");
            TimeZoneDropDown.Items.Add("(UTC+08:00) Ulaanbaatar");
            TimeZoneDropDown.Items.Add("(UTC+08:45) Eucla");
            TimeZoneDropDown.Items.Add("(UTC+09:00) Chita");
            TimeZoneDropDown.Items.Add("(UTC+09:00) Osaka, Sapporo, Tokyo");
            TimeZoneDropDown.Items.Add("(UTC+09:00) Pyongyang");
            TimeZoneDropDown.Items.Add("(UTC+09:00) Seoul");
            TimeZoneDropDown.Items.Add("(UTC+09:00) Yakutsk");
            TimeZoneDropDown.Items.Add("(UTC+09:30) Adelaide");
            TimeZoneDropDown.Items.Add("(UTC+09:30) Darwin");
            TimeZoneDropDown.Items.Add("(UTC+10:00) Brisbane");
            TimeZoneDropDown.Items.Add("(UTC+10:00) Canberra, Melbourne, Sydney");
            TimeZoneDropDown.Items.Add("(UTC+10:00) Guam, Port Moresby");
            TimeZoneDropDown.Items.Add("(UTC+10:00) Hobart");
            TimeZoneDropDown.Items.Add("(UTC+10:00) Vladivostok");
            TimeZoneDropDown.Items.Add("(UTC+10:30) Lord Howe Island");
            TimeZoneDropDown.Items.Add("(UTC+11:00) Bougainville Island");
            TimeZoneDropDown.Items.Add("(UTC+11:00) Chokurdakh");
            TimeZoneDropDown.Items.Add("(UTC+11:00) Magadan");
            TimeZoneDropDown.Items.Add("(UTC+11:00) Norfolk Island");
            TimeZoneDropDown.Items.Add("(UTC+11:00) Sakhalin");
            TimeZoneDropDown.Items.Add("(UTC+11:00) Solomon Is., New Caledonia");
            TimeZoneDropDown.Items.Add("(UTC+12:00) Anadyr, Petropavlovsk-Kamchatsky");
            TimeZoneDropDown.Items.Add("(UTC+12:00) Auckland, Wellington");
            TimeZoneDropDown.Items.Add("(UTC+12:00) Coordinated Universal Time+12");
            TimeZoneDropDown.Items.Add("(UTC+12:00) Fiji");
            TimeZoneDropDown.Items.Add("(UTC+12:45) Chatham Islands");
            TimeZoneDropDown.Items.Add("(UTC+13:00) Coordinated Universal Time+13");
            TimeZoneDropDown.Items.Add("(UTC+13:00) Nuku'alofa");
            TimeZoneDropDown.Items.Add("(UTC+13:00) Samoa");
            TimeZoneDropDown.Items.Add("(UTC+14:00) Kiritimati Island");

            if (string.IsNullOrEmpty(OutOfBoxExperienceInfo.NetworkLocation))
                NetworkLocationDropDown.SelectedIndex = 0;
            else
                NetworkLocationDropDown.SelectedIndex = OutOfBoxExperienceInfo.NetworkLocation switch
                {
                    "Home" => 1,
                    "Work" => 2,
                    _ => NetworkLocationDropDown.SelectedIndex
                };

            NetworkLocationDropDown.SelectedItem = string.IsNullOrEmpty(DeviceInfo.TimeZone) ? 50 : DeviceInfo.TimeZoneId;
            
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
            DeviceNameTextBox.Text = DeviceInfo.DeviceName;
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
            Debug.WriteLine("Password: " + DomainInfo.Password);
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
            OutOfBoxExperienceInfo.UseOOBEInfo = ToggleOobeSetup.IsChecked!.Value;
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
            OutOfBoxExperienceInfo.HideEULAPage = HideEulaPageSwitch.IsChecked!.Value;
        }

        private void HideOemRegistrationPageSwitch_OnClick(object sender, RoutedEventArgs e)
        {
            OutOfBoxExperienceInfo.HideOEMRegistrationScreen = HideOemRegistrationPageSwitch.IsChecked!.Value;
        }

        private void HideOnlineAccountPageSwitch_OnClick(object sender, RoutedEventArgs e)
        {
            OutOfBoxExperienceInfo.HideOnlineAccountScreens = HideOnlineAccountPageSwitch.IsChecked!.Value;
        }

        private void HideWirelessSetupPageSwitch_OnClick(object sender, RoutedEventArgs e)
        {
            OutOfBoxExperienceInfo.HideWirelessSetupInOOBE = HideWirelessSetupPageSwitch.IsChecked!.Value;
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
        
        private void TimeZoneDropDown_SelectionChanged(object sender, RoutedEventArgs e)
        {
            var cmb = (ComboBox)sender;
            
            DeviceInfo.TimeZone = TimeZone.GetTimeZone(cmb.SelectedIndex);
            DeviceInfo.TimeZoneId = cmb.SelectedIndex;
            Debug.Write($"Selected TimeZone in Unattend (Index {cmb.SelectedIndex}): ");
            Debug.Write($"{DeviceInfo.TimeZone}\n", true, ConsoleColor.DarkYellow);
        }

        private void SkipMachineOOBESwitch_OnClick(object sender, RoutedEventArgs e)
        {
            OutOfBoxExperienceInfo.SkipMachineOOBE = SkipMachineOOBESwitch.IsChecked!.Value;
        }

        private void SkipUserOOBESwitch_OnClick(object sender, RoutedEventArgs e)
        {
            OutOfBoxExperienceInfo.SkipUserOOBE = SkipUserOOBESwitch.IsChecked!.Value;
        }

        private void ProductKeyTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            DeviceInfo.ProductKey = ProductKeyTextBox.Text;
        }

        private void DeviceNameTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            DeviceInfo.DeviceName = DeviceNameTextBox.Text;
        }
    }
}
