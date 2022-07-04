using System.Diagnostics;
using System.Windows;
using Wpf.Ui.Appearance;

namespace deploya
{
    /// <summary>
    /// Interaction logic for MainUI.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            Loaded += (sender, args) =>
            {
                Wpf.Ui.Appearance.Watcher.Watch(
                  this,                           // Window class
                  BackgroundType.Mica,            // Background type
                  true                            // Whether to change accents automatically
                );
            };

            InitializeComponent();
            
#if DEBUG
            DebugString.Visibility = Visibility.Visible;
#else
            DebugString.Visibility = Visibility.Hidden;
#endif
        }

        private void RootNavigation_OnLoaded(object sender, RoutedEventArgs e)
        {
            RootNavigation.Navigate("dashboard");
        }

        private void CommandLine_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("cmd.exe");
        }

        
    }
}
