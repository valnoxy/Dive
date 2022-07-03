using deploya.Pages.ApplyPages;
using System.Windows;
using System.Windows.Controls;

namespace deploya.Pages
{
    /// <summary>
    /// Interaktionslogik für ApplyContent.xaml
    /// </summary>
    public partial class ApplyContent : UserControl
    {
        public static ApplyContent? ContentWindow;

        SKUSelectStep SKUSS = new SKUSelectStep();

        public ApplyContent()
        {
            InitializeComponent();

            NextBtn.IsEnabled = false;
            FrameWindow.Content = SKUSS;
            ContentWindow = this;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            switch (FrameWindow.Content)
            {
                case SKUSelectStep:
                    //FrameWindow.Content = PRSV;
                    //PRSV.InitializeAsync();
                    break;
                default:
                    break;
            }
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            FrameWindow.Content = SKUSS;
            NextBtn.IsEnabled = false;
            NextBtn.Visibility = Visibility.Visible;
        }
    }
}
