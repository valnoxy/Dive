using Dive.UI.Pages.Extras.AnswerConfiguration;
using System.Windows;

namespace Dive.UI.Pages.Extras.ConfigAssistant
{
    /// <summary>
    /// Interaktionslogik für ConfigAssistantWindow.xaml
    /// </summary>
    public partial class ConfigAssistantWindow
    {
        internal static ConfigAssistantWindow? ContentWindow;
        private static Welcome? _welcomePage;
        private static SetupConfig? _setupConfig;

        public ConfigAssistantWindow()
        {
            InitializeComponent();
            NextBtn.IsEnabled = true;
            BackBtn.IsEnabled = false;
            FrameWindow.Content = _welcomePage = new Welcome();
            ContentWindow = this;

            _setupConfig = new SetupConfig();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            switch (FrameWindow.Content)
            {
                case Welcome:
                    FrameWindow.Content = _setupConfig;
                    NextBtn.IsEnabled = true;
                    BackBtn.IsEnabled = true;
                    break;
                default:
                    break;
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            switch (FrameWindow.Content)
            {
                case SetupConfig:
                    FrameWindow.Content = _welcomePage;
                    NextBtn.IsEnabled = true;
                    BackBtn.IsEnabled = false;
                    break;
                default:
                    break;
            }
        }
    }
}
