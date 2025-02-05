using Dive.Core;
using Dive.Core.Common;
using System.ComponentModel;
using System.IO;
using System.Windows.Media;
using Dive.UI.Common;
using Newtonsoft.Json;
using System;
using Dive.UI.Common.UserInterface;

namespace Dive.UI.Pages.CapturePages
{
    /// <summary>
    /// Interaktionslogik für CaptureStep.xaml
    /// </summary>
    public partial class CaptureStep : System.Windows.Controls.UserControl
    {
        private BackgroundWorker captureBackgroundWorker;
        private string FullFilePath = Path.Combine(Common.CaptureInfo.PathToImage, Common.CaptureInfo.ImageFileName);
        bool IsCanceled = false;
        
        public CaptureStep()
        {
            InitializeComponent();

            if (CaptureContent.ContentWindow != null)
            {
                CaptureContent.ContentWindow.NextBtn.IsEnabled = false;
                CaptureContent.ContentWindow.BackBtn.IsEnabled = false;
                CaptureContent.ContentWindow.CancelBtn.IsEnabled = false;
            }

            // Set active Image to card
            ImageName.Text = Common.CaptureInfo.Name;
            ImageFile.Text = FullFilePath;
            ImageSourceConverter img = new ImageSourceConverter();
            try
            {
                ImageIcon.Source = (ImageSource)img.ConvertFromString("pack://application:,,,/assets/icon-hdd-40.png");
            }
            catch
            {
                // ignored
            }

            // Background worker for deployment
            captureBackgroundWorker = new BackgroundWorker();
            captureBackgroundWorker.WorkerReportsProgress = true;
            captureBackgroundWorker.WorkerSupportsCancellation = true;
            captureBackgroundWorker.DoWork += CaptureWim;
            captureBackgroundWorker.ProgressChanged += captureBackgroundWorker_ProgressChanged;
            captureBackgroundWorker.RunWorkerAsync();
        }

        private void captureBackgroundWorker_ProgressChanged(object? sender, ProgressChangedEventArgs e)
        {
            var responseJson = e.UserState as string;
            if (string.IsNullOrEmpty(responseJson)) return;

            var response = JsonConvert.DeserializeObject<ActionWorker>(responseJson);
            if (response.IsError)
            {
                ProgrText.Text = response.Message;
                ProgrBar.Value = e.ProgressPercentage;

                Debug.WriteLine(response.Message, ConsoleColor.Red);

                if (ApplyContent.ContentWindow != null)
                {
                    ApplyContent.ContentWindow.NextBtn.IsEnabled = false;
                    ApplyContent.ContentWindow.BackBtn.IsEnabled = false;
                    ApplyContent.ContentWindow.CancelBtn.IsEnabled = true;
                }
                if (CloudContent.ContentWindow != null)
                {
                    CloudContent.ContentWindow.NextBtn.IsEnabled = false;
                    CloudContent.ContentWindow.BackBtn.IsEnabled = false;
                    CloudContent.ContentWindow.CancelBtn.IsEnabled = true;
                }

                IsCanceled = true;
                return;
            }

            if (response.IsWarning)
            {
                Debug.WriteLine(response.Message, ConsoleColor.Yellow);
                return;
            }

            if (response.IsDebug)
            {
                if (response.Message == "Job Done.")
                {
                    ProgrText.Text = "Capturing completed. Press 'Next' to close Dive.";
                    ProgrBar.Value = 100;
                    if (ApplyContent.ContentWindow != null)
                        ApplyContent.ContentWindow.NextBtn.IsEnabled = true;
                    if (CloudContent.ContentWindow != null)
                        CloudContent.ContentWindow.NextBtn.IsEnabled = true;
                    return;
                }
                Debug.WriteLine(response.Message);
                return;
            }

            switch (response.Action)
            {
                case Progress.CaptureDisk:
                    {
                        ProgrText.Text = response.Message;
                        if (response.IsIndeterminate)
                            ProgrBar.IsIndeterminate = true;
                        else
                        {
                            ProgrBar.IsIndeterminate = false;
                            ProgrBar.Value = e.ProgressPercentage;
                        }
                        Debug.RefreshProgressBar(Progress.CaptureDisk, e.ProgressPercentage, response.Message);
                        break;
                    }
            }
        }

        private void CaptureWim(object? sender, DoWorkEventArgs e)
        {
            #region Environment definition

            var worker = sender as BackgroundWorker;

            #endregion

            // Capture disk
            Actions.CaptureToWim(Common.CaptureInfo.Name, Common.CaptureInfo.Description, Common.CaptureInfo.PathToCapture, FullFilePath, worker);
            if (IsCanceled)
            {
                e.Cancel = true;
                return;
            }

            // Installation complete
            worker?.ReportProgress(100, JsonConvert.SerializeObject(new ActionWorker
            {
                IsDebug = true,
                Message = "Job Done."
            }));
        }
    }
}
