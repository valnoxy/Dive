using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;

namespace deployaUI.Pages.CapturePages
{
    /// <summary>
    /// Interaktionslogik für SettingsStep.xaml
    /// </summary>
    public partial class SettingsStep : System.Windows.Controls.UserControl
    {
        public SettingsStep()
        {
            InitializeComponent();
        }

        private void Source_OpenFolderClick(object sender, RoutedEventArgs e)
        {
            using var dialog = new FolderBrowserDialog
            {
                Description = "Select the source directory to capture.",
                UseDescriptionForTitle = true,
                SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyComputer),
                ShowNewFolderButton = false
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                TbPath.Text = dialog.SelectedPath;
                Common.CaptureInfo.PathToCapture = dialog.SelectedPath;
                Validate();
            }
        }

        private void TbName_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            Common.CaptureInfo.Name = TbName.Text;
            Validate();
        }

        private void TbDescription_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            Common.CaptureInfo.Description = TbDescription.Text;
            Validate();
        }

        private void Validate()
        {
            CaptureContent.ContentWindow.NextBtn.IsEnabled =
                !String.IsNullOrEmpty(TbName.Text) && Directory.Exists(TbPath.Text);
        }
    }
}
