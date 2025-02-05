using System;
using Kevsoft.WLED;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Dive.UI.Common;
using Dive.UI.Common.UserInterface;

namespace Dive.UI.Pages.Extras.FunPages
{
    /// <summary>
    /// Interaktionslogik für FunSettings.xaml
    /// </summary>
    public partial class FunSettings
    {
        public FunSettings()
        {
            InitializeComponent();
            WledIpTextBox.Text = Common.Configuration.FunConfig.WledControllerIp;
            ToggleWled.IsChecked = Common.Configuration.FunConfig.WledCommunication;
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e) => Close();

        private async void WledIpTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var validIp = IPAddress.TryParse(WledIpTextBox.Text, out var ip);

            if (validIp)
            {
                await ConnectionTest(ip.ToString());
            }
        }

        private async Task ConnectionTest(string? ip)
        {
            try
            {
                Common.Configuration.FunConfig.WledClient = new WLedClient($"http://{ip}");
                var data = await Common.Configuration.FunConfig.WledClient.Get();
                WledStatus.Text = $"Connected with {data.Information.Name}";
                Common.Configuration.FunConfig.WledControllerIp = ip;
                Common.Configuration.FunConfig.AvailableLEDs = data.Information.Leds.Count;
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine("Failed connection: " + ex.Message);
            }
            catch (Exception ex)
            {
                WledStatus.Text = "Not connected!";
                Debug.WriteLine("Failed connection: " + ex.Message);
            }
        }

        private void ToggleWled_Checked(object sender, RoutedEventArgs e)
        {
            Common.Configuration.FunConfig.WledCommunication = (bool)ToggleWled.IsChecked!;
        }
    }
}
