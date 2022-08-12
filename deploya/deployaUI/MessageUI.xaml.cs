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
        public MessageUI(string title, string message, string Btn1 = null, string Btn2 = null)
        {
            InitializeComponent();

            MessageTitle.Text = title;
            MessageText.Text = message;
            this.Btn1.Content = Btn1;
            this.Btn2.Content = Btn2;
        }

        private void Btn1_OnClick(object sender, RoutedEventArgs e)
        {
            ButtonPressed = "Btn1";
            this.Close();
        }

        private void Btn2_OnClick(object sender, RoutedEventArgs e)
        {
            ButtonPressed = "Btn2";
            this.Close();
        }
    }
}
