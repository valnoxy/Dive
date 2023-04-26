using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
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

            var versionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location);
            ValueVersion.Content = $"{versionInfo.ProductName} V. {versionInfo.ProductVersion}";
            ValueCopyright.Content = versionInfo.LegalCopyright;

            // Define Core Version
            var coreVersion = typeof(Core.Common.Entities).Assembly
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion;
            ValueVersionCore.Content = $"{versionInfo.ProductName} Core V. {coreVersion}";

            // Define AutoInit Module Version
            var autoInitVersion = typeof(AutoInit.AppxManagement).Assembly
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion;
            ValueVersionAutoInit.Content = $"{versionInfo.ProductName} AutoInit Module V. {autoInitVersion}";
        }
    }
}
