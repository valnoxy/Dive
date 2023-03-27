using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;

namespace deployaUI
{
    /// <summary>
    /// Interaktionslogik für SplashScreen.xaml
    /// </summary>
    public partial class SplashScreen : Wpf.Ui.Controls.UiWindow
    {
        public SplashScreen()
        {
            InitializeComponent();

            // Get version
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            var version = fvi.FileVersion;
            VersionLabel.Text = $"Version {version}";
#if DEBUG
            VersionLabel.Text = $"Version {version} - Debug build";
#endif

            LanguageDropDown.Items.Add("English");
            LanguageDropDown.Items.Add("Deutsch");
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
