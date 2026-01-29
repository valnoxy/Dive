using Dive.UI.Common;
using Dive.UI.Pages.TweaksPages.PlayBook;
using System.Windows;

namespace Dive.UI.Pages.TweaksPages
{
    /// <summary>
    /// Interaktionslogik für TweaksDashboard.xaml
    /// </summary>
    public partial class TweaksDashboard
    {
        private static readonly Tweaks TweaksInstance = Tweaks.Instance;

        public TweaksDashboard()
        {
            InitializeComponent();
        }

        private void SwitchToAutoInit(object sender, RoutedEventArgs e)
        {
            TweaksInstance.CurrentMode = TweakMode.AutoInit;
            TweaksContent.ContentWindow!.FrameWindow.Content = new PlayBookLoad();
        }

        private void SwitchToUserMigratePage(object sender, RoutedEventArgs e)
        {
            TweaksInstance.CurrentMode = TweakMode.MigrateUser;
            TweaksContent.ContentWindow!.FrameWindow.Content = new USMT.MethodSelection();
        }

        private void SwitchToMigratePage(object sender, RoutedEventArgs e)
        {
            TweaksInstance.CurrentMode = TweakMode.Migrate;
            TweaksContent.ContentWindow!.FrameWindow.Content = new MigrateSettings();
        }

        private void SwitchToRepairPage(object sender, RoutedEventArgs e)
        {
            TweaksInstance.CurrentMode = TweakMode.RepairBootloader;
        }
    }
}
