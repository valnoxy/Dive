using System;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace Dive.UI
{
    /// <summary>
    /// Interaktionslogik für ErrorHandle.xaml
    /// </summary>
    public partial class ErrorHandle
    {
        public virtual string? Summary => _buttonPressed;

        private static string? _buttonPressed;
        private static bool _mainThread;

        public ErrorHandle(string title, string message, string btn1 = null, string btn2 = null, bool isMainThread = false, int timer = 0)
        {
            InitializeComponent();

            MessageTitle.Text = title;
            MessageText.Text = message;
            this.Btn1.Content = btn1;
            this.Btn2.Content = btn2;

            if (btn1 is null or "")
                this.Btn1.Visibility = Visibility.Hidden;
            if (btn2 is null or "")
                this.Btn2.Visibility = Visibility.Hidden;

            _mainThread = isMainThread;
        }
        
        private void Btn1_OnClick(object sender, RoutedEventArgs e)
        {
            _buttonPressed = "Btn1";
            if (_mainThread) this.Hide();
            else this.Close();
        }

        private void Btn2_OnClick(object sender, RoutedEventArgs e)
        {
            _buttonPressed = "Btn2";
            if (_mainThread) this.Hide();
            else this.Close();
        }
    }
}
