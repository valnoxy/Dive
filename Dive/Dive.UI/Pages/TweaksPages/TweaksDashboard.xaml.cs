using System.Windows;
using System.Windows.Controls;
using Dive.UI.Common;
using Dive.UI.Pages.TweaksPages.PlayBook;

namespace Dive.UI.Pages.TweaksPages
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

        private void SwitchToAutoInit(object sender, RoutedEventArgs e)
        {
            Common.Tweaks.CurrentMode = TweakMode.AutoInit;
            TweaksContent.ContentWindow.FrameWindow.Content = new PlayBookLoad();
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
