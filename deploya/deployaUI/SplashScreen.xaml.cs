using System;
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

            LanguageDropDown.Items.Add("English");
            LanguageDropDown.Items.Add("Deutsch");
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
