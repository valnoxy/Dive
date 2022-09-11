using System;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace deployaUI
{
    /// <summary>
    /// Interaktionslogik für MessageUI.xaml
    /// </summary>
    public partial class MessageUI: Wpf.Ui.Controls.UiWindow
    {
        public string Summary => ButtonPressed;

        public static string ButtonPressed;
        private static bool MainThread;
        DispatcherTimer _timer;
        TimeSpan _time;

        public MessageUI(string title, string message, string Btn1 = null, string Btn2 = null, bool IsMainThread = false, int timer = 0)
        {
            InitializeComponent();

            MessageTitle.Text = title;
            MessageText.Text = message;
            this.Btn1.Content = Btn1;
            this.Btn2.Content = Btn2;
            
            if (Btn1 == null)
                this.Btn1.Visibility = Visibility.Hidden;
            if (Btn2 == null)
                this.Btn2.Visibility = Visibility.Hidden;

            if (timer != 0)
            {
                _time = TimeSpan.FromSeconds(timer);

                _timer = new DispatcherTimer(new TimeSpan(0, 0, 1), DispatcherPriority.Normal, delegate
                {
                    LbTimer.Text = $"{_time.ToString("%s")}s before auto-selecting '{this.Btn2.Content}'.";
                    if (_time == TimeSpan.Zero)
                    {
                        _timer.Stop();
                        ButtonPressed = "Btn2";
                        if (MainThread) this.Hide();
                        else this.Close();
                    }
                    _time = _time.Add(TimeSpan.FromSeconds(-1));
                }, Application.Current.Dispatcher);

                _timer.Start();
            }
            else LbTimer.Visibility = Visibility.Hidden;

            MainThread = IsMainThread;
        }
        
        private void Btn1_OnClick(object sender, RoutedEventArgs e)
        {
            ButtonPressed = "Btn1";
            if (MainThread) this.Hide();
            else this.Close();
        }

        private void Btn2_OnClick(object sender, RoutedEventArgs e)
        {
            ButtonPressed = "Btn2";
            if (MainThread) this.Hide();
            else this.Close();
        }
    }
}
