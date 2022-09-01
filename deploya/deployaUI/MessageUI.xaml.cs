using System.Windows;

namespace deployaUI
{
    /// <summary>
    /// Interaktionslogik für MessageUI.xaml
    /// </summary>
    public partial class MessageUI: Wpf.Ui.Controls.UiWindow
    {
        public string Summary
        {
            get { return ButtonPressed; }
        }

        public static string ButtonPressed = null;
        private static bool MainThread;
        public MessageUI(string title, string message, string Btn1 = null, string Btn2 = null, bool IsMainThread = false)
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
