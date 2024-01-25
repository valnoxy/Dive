using System.Windows;

namespace Dive.UI.Pages.TweaksPages.USMT
{
    /// <summary>
    /// Interaktionslogik für MethodSelection.xaml
    /// </summary>
    public partial class MethodSelection
    {
        public MethodSelection()
        {
            InitializeComponent();
        }

        private void SwitchToLocalAccountManagement(object sender, RoutedEventArgs e)
        {
            TweaksContent.ContentWindow!.FrameWindow.Content = new UserList();
        }
    }
}
