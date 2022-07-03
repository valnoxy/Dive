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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace deploya_setup.Pages
{
    /// <summary>
    /// Interaktionslogik für ContentPage.xaml
    /// </summary>
    public partial class ContentPage : Page
    {
        WelcomeStep WS = new WelcomeStep();
        
        public ContentPage()
        {
            InitializeComponent();
            FrameWindow.Content = WS;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            switch (FrameWindow.Content)
            {
                case deploya_setup.Pages.WelcomeStep:
                    //FrameWindow.Content = PRSV;
                    NextBtn.Visibility = Visibility.Hidden;
                    //PRSV.InitializeAsync();
                    break;
                default:
                    break;
            }
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            FrameWindow.Content = WS;
            NextBtn.Visibility = Visibility.Visible;
        }
    }
}
