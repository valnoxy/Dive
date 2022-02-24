using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace deploya
{
    /// <summary>
    /// Interaction logic for MainUI.xaml
    /// </summary>
    public partial class MainUI : Window
    {
        private bool _isDarkTheme = false;

        public MainUI()
        {
            WPFUI.Background.Manager.Apply(this);

            InitializeComponent();
#if DEBUG
            DebugString.Visibility = Visibility.Visible;
#else
            DebugString.Visibility = Visibility.Hidden;
#endif

            // Enable Mica background
            ApplyBackgroundEffect(2);
        }

        private void ApplyBackgroundEffect(int index)
        {
            IntPtr windowHandle = new WindowInteropHelper(this).Handle;

            WPFUI.Background.Manager.Remove(windowHandle);

            if (_isDarkTheme)
            {
                WPFUI.Background.Manager.ApplyDarkMode(windowHandle);
            }
            else
            {
                WPFUI.Background.Manager.RemoveDarkMode(windowHandle);
            }

            /*
             * Theme Testing ...
             *  - TODO: Test all available themes; Clean-up code
             */
#warning All debug themes are still implemented
            switch (index)
            {
                case 1: // AUTO
                    this.Background = Brushes.Transparent;
                    WPFUI.Background.Manager.Apply(WPFUI.Background.BackgroundType.Auto, windowHandle);
                    break;

                case 2: // Mica
                    this.Background = Brushes.Transparent;
                    WPFUI.Background.Manager.Apply(WPFUI.Background.BackgroundType.Mica, windowHandle);
                    break;

                case 3: // Tabbed
                    this.Background = Brushes.Transparent;
                    WPFUI.Background.Manager.Apply(WPFUI.Background.BackgroundType.Tabbed, windowHandle);
                    break;

                case 4: // Acrylic
                    this.Background = Brushes.Transparent;
                    WPFUI.Background.Manager.Apply(WPFUI.Background.BackgroundType.Acrylic, windowHandle);
                    break;
            }
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
