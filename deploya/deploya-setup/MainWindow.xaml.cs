using deploya_setup.Pages;
using System;
using System.Windows;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace deploya_setup
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        WelcomeStep WS = new WelcomeStep();
        SKUSelectStep SSS = new SKUSelectStep();
        DiskSelectStep DSS = new DiskSelectStep();
        
        public MainWindow()
        {
            InitializeComponent();

#if DEBUG
            if (File.Exists("X:\\Windows\\System32\\wpeutil.exe"))
            {
                Titlebar.Title = "deploya Setup [Debug build] - WinPE instance";
            }
            else
            {
                Titlebar.Title = "deploya Setup [Debug build] - No WinPE instance";
            }
#endif

            Loaded += (sender, args) =>
            {
                Wpf.Ui.Appearance.Watcher.Watch(
                  this,                                     // Window class
                  Wpf.Ui.Appearance.BackgroundType.Mica,    // Background type
                  true                                      // Whether to change accents automatically
                );
            };

            FrameWindow.Content = WS;
        }
        
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            switch (FrameWindow.Content)
            {
                case WelcomeStep:
                    FrameWindow.Content = SSS;
                    NextBtn.Visibility = Visibility.Visible;
                    NextBtn.IsEnabled = false;
                    break;
                case SKUSelectStep:
                    FrameWindow.Content = DSS;
                    NextBtn.Visibility = Visibility.Visible;
                    NextBtn.IsEnabled = false;
                    break;
                default:
                    break;
            }
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            FrameWindow.Content = WS;
            NextBtn.Visibility = Visibility.Visible;
            NextBtn.IsEnabled = true;
        }

    }
}
