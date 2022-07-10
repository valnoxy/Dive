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
            Wpf.Ui.Appearance.Background.Apply(
              this,                                 // Window class
              Wpf.Ui.Appearance.BackgroundType.Mica // Background type
            );

            Wpf.Ui.Appearance.Theme.Apply(
              Wpf.Ui.Appearance.ThemeType.Dark,     // Theme type
              Wpf.Ui.Appearance.BackgroundType.Mica, // Background type
              true                                   // Whether to change accents automatically
            );

            InitializeComponent();
            
#if DEBUG
            DebugString.Visibility = Visibility.Visible;
            DebugString.Text = "Debug build - This is not a production ready build.";
#else
            DebugString.Visibility = Visibility.Visible;
            DebugString.Text = "Beta build [B1 Internal]";
#endif
        }

        private void RootNavigation_OnLoaded(object sender, RoutedEventArgs e)
        {
            RootNavigation.Navigate("dashboard");
        }

        private void CommandLine_Click(object sender, RoutedEventArgs e)
        {
            Process p = new Process();
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.CreateNoWindow = false;
            p.StartInfo.UseShellExecute = true;
            p.Start();
        }        
    }
}
