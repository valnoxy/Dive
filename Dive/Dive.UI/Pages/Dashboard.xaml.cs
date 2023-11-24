using System.Windows;
using System.Windows.Controls;
using Dive.UI.Pages;

namespace Dive.UI.Pages
{
    /// <summary>
    /// Interaktionslogik für Dashboard.xaml
    /// </summary>
    public partial class Dashboard : Page
    {
        public Dashboard()
        {
            InitializeComponent();
        }

        private void SwitchToApplyPage(object sender, RoutedEventArgs e)
        {
            (Application.Current.MainWindow as MainWindow)?.RootNavigation.Navigate(typeof(ApplyContent));
        }

        private void SwitchToCapturePage(object sender, RoutedEventArgs e)
        {
            (Application.Current.MainWindow as MainWindow)?.RootNavigation.Navigate(typeof(CaptureContent));
        }

        private void SwitchToCloudPage(object sender, RoutedEventArgs e)
        {
            (Application.Current.MainWindow as MainWindow)?.RootNavigation.Navigate(typeof(CloudContent));
        }

        private void SwitchToRepairPage(object sender, RoutedEventArgs e)
        {
            (Application.Current.MainWindow as MainWindow)?.RootNavigation.Navigate(typeof(TweaksContent));
        }
    }
}
