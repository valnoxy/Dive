using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using Dive.UI.Common;

namespace Dive.UI.Pages
{
    /// <summary>
    /// Interaktionslogik für AboutPage.xaml
    /// </summary>
    public partial class AboutPage : UserControl
    {
        public AboutPage()
        {
            InitializeComponent();

            var versionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly()!.Location);
            ValueVersion.Content = $"{versionInfo.ProductName} v{versionInfo.ProductVersion}";
            ValueCopyright.Content = versionInfo.LegalCopyright;

            // Define Core Version
            var coreVersion = typeof(Core.Common.Entities).Assembly
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion;
            ValueVersionCore.Content = $"{versionInfo.ProductName} Core v{coreVersion}";

            // Define Plugins Version
            var pluginList = Plugin.PluginManager.GetPlugins();
            foreach (var plugin in pluginList)
            {
                ValueVersionPlugins.Content += $"{plugin.GetPluginInfo().Name} v{plugin.GetPluginInfo().Version}\n";
            }
            ValueVersionPlugins.Content = ValueVersionPlugins.Content.ToString()!.TrimEnd('\n');

            LicenseTitle.Text = Common.Licensing.Validation.Info.Valid
                ? $"Founders Edition"
                : "License Status";
            LicenseStatus.Text = Common.Licensing.Validation.Info.Valid 
                ? $"Licensed to {Common.Licensing.Validation.Info.LicenseName} ({Common.Licensing.Validation.Info.LicenseEmail})" 
                : "You are currently using the Free Edition.";

            if (!Common.Licensing.Validation.Info.Valid)
            {
                LicensePanel.Visibility = Visibility.Collapsed;
            }
        }

        private void Homepage_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo { FileName = "https://exploitox.de", UseShellExecute = true });
            }
            catch (System.ComponentModel.Win32Exception noBrowser)
            {
                if (noBrowser.ErrorCode == -2147467259)
                    MessageBox.Show(noBrowser.Message);
            }
            catch (Exception other)
            {
                MessageBox.Show(other.Message);
            }
        }

        private void Donate_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo { FileName = "https://paypal.me/valnoxy", UseShellExecute = true });
            }
            catch (System.ComponentModel.Win32Exception noBrowser)
            {
                if (noBrowser.ErrorCode == -2147467259)
                    MessageBox.Show(noBrowser.Message);
            }
            catch (Exception other)
            {
                MessageBox.Show(other.Message);
            }
        }

        private void SourceCode_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo { FileName = "https://github.com/valnoxy/dive", UseShellExecute = true });
            }
            catch (System.ComponentModel.Win32Exception noBrowser)
            {
                if (noBrowser.ErrorCode == -2147467259)
                    MessageBox.Show(noBrowser.Message);
            }
            catch (Exception other)
            {
                MessageBox.Show(other.Message);
            }
        }
    }
}
