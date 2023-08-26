using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
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

            // Define AutoInit Module Version
            var autoInitVersion = typeof(AutoInit.AppxManagement).Assembly
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion;
            ValueVersionAutoInit.Content = $"{versionInfo.ProductName} AutoInit Module v{autoInitVersion}";
        }

        private void Homepage_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo() { FileName = "https://exploitox.de", UseShellExecute = true });
            }
            catch
            {
                // ignored
            }
        }

        private void Donate_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo() { FileName = "https://paypal.me/valnoxy", UseShellExecute = true });
            }
            catch
            {
                // ignored
            }
        }

        private void SourceCode_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo() { FileName = "https://github.com/valnoxy/dive", UseShellExecute = true });
            }
            catch (System.ComponentModel.Win32Exception noBrowser)
            {
                if (noBrowser.ErrorCode == -2147467259)
                    MessageBox.Show(noBrowser.Message);
            }
            catch (System.Exception other)
            {
                MessageBox.Show(other.Message);
            }
        }
    }
}
