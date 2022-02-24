using System.Windows;
using System.Windows.Controls;
using deploya.Pages;

namespace deploya.Pages
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
            (Application.Current.MainWindow as MainUI)?.RootNavigation.Navigate("apply");
        }
    }
}
