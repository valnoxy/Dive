using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Dive.UI.Common;
using Dive.UI.Pages;
using Dive.UI.Pages.TweaksPages;

namespace Dive.UI.Pages
{
    /// <summary>
    /// Interaktionslogik für TweaksDashboard.xaml
    /// </summary>
    public partial class TweaksDashboard : UserControl
    {
        public TweaksDashboard()
        {
            InitializeComponent();
        }

        private void SwitchToMigratePage(object sender, RoutedEventArgs e)
        {
            Common.Tweaks.CurrentMode = TweakMode.Migrate;
            TweaksContent.ContentWindow.FrameWindow.Content = new MigrateSettings();
        }

        private void SwitchToRepairPage(object sender, RoutedEventArgs e)
        {
            Common.Tweaks.CurrentMode = TweakMode.RepairBootloader;
        }
    }
}
