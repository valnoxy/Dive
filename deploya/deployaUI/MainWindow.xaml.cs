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
        private bool _displayDebugConsole = false;

        public MainWindow()
        {
            Wpf.Ui.Appearance.Background.Apply(
              this,         // Window class
              BackgroundType.Mica // Background type
            );

            InitializeComponent();
            
#if DEBUG
            DebugString.Visibility = Visibility.Visible;
            DebugString.Text = "Debug build - This is not a production ready build.";
            _displayDebugConsole = true;
#else
            DebugString.Visibility = Visibility.Visible;
            DebugString.Text = "Beta build [B5 Public]";
#endif
            // Get version
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
            var version = fvi.FileVersion;
            VersionString.Text = $"[{version}]";
        }

        private void RootNavigation_OnLoaded(object sender, RoutedEventArgs e)
        {
            RootNavigation.Navigate("dashboard");
        }

        private void CommandLine_Click(object sender, RoutedEventArgs e)
        {
            /*
            var p = new Process();
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.CreateNoWindow = false;
            p.StartInfo.UseShellExecute = true;
            p.Start();
            */
            var handle = App.GetConsoleWindow();

            if (_displayDebugConsole == true)
            {
                App.ShowWindow(handle, App.SW_HIDE);
                _displayDebugConsole = false;
            }
            else
            {
                App.ShowWindow(handle, App.SW_SHOW);
                _displayDebugConsole = true;
            }
        }        
    }
}
