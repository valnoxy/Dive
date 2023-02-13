using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using deployaUI.Common;
using deployaUI.Pages;
using deployaUI.Pages.TweaksPages;

namespace deployaUI.Pages
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
