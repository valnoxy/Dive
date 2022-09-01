using System.Diagnostics;
using System.Windows;
using Wpf.Ui.Appearance;

namespace deployaUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Wpf.Ui.Controls.UiWindow
    {
        public MainWindow()
        {
            Wpf.Ui.Appearance.Background.Apply(
              this,         // Window class
              BackgroundType.Mica // Background type
            );

            Theme.Apply(
              ThemeType.Dark      // Theme type
            );

            InitializeComponent();
            
#if DEBUG
            DebugString.Visibility = Visibility.Visible;
            DebugString.Text = "Debug build - This is not a production ready build.";
#else
            DebugString.Visibility = Visibility.Visible;
            DebugString.Text = "Beta build [B3 Public]";
#endif
            // Get version
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            System.Diagnostics.FileVersionInfo fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = fvi.FileVersion;
            VersionString.Text = $"[{version}]";
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
