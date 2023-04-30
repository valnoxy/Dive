using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Dive.UI.Initialization
{
    /// <summary>
    /// Interaktionslogik für BootWindow.xaml
    /// </summary>
    public partial class BootWindow : Window
    {
        public Configuration? Config { get; set; }

        public BootWindow(Configuration? config)
        {
            InitializeComponent();

            if (config == null)
            {
                const string message = "This playbook contains an invalid configuration. ";
                const string title = "Dive - AutoInit Module";
                const string btn1 = "OK";

                var w = new MessageUI(title, message, btn1, null, true);
                w.ShowDialog();
                Environment.Exit(1);
            }

            Config = config;
        }

        private bool _easterEggIsEnabled = false;

        private void MainWindow_OnKeyDown(object sender, KeyEventArgs e)
        {
            // Check name of Author
            if (!new[] { "exploitox", "wolkenhof" }.Any(c => Config!.Author.ToLower().Contains(c)))
                return;

            if (Keyboard.Modifiers != ModifierKeys.Control || e.Key != Key.F) return;
            if (!_easterEggIsEnabled)
            {
                EasterEgg.Visibility = Visibility.Visible;
                ProgressRing.Visibility = Visibility.Hidden;
                _easterEggIsEnabled = true;
            }
            else
            {
                EasterEgg.Visibility = Visibility.Hidden;
                ProgressRing.Visibility = Visibility.Visible;
                _easterEggIsEnabled = false;
            }
        }
    }
}
