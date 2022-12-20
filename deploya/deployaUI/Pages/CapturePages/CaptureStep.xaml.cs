using deploya_core;
using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Media;
using deployaUI.Common;
using System.Text.RegularExpressions;

namespace deployaUI.Pages.CapturePages
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
            //
            // Value list
            // --------------------------
            // Progressbar handling
            //   101: Progressbar is not Indeterminate
            //   102: Progressbar is Indeterminate
            //
            // Standard Message handling
            //   201: ProgText -> Capture Disk
            //   250: Capturing complete
            //
            // Error message handling
            //   301: Failed at capturing disk
            //
            // Range 0-100 -> Progressbar percentage
            //

            // Progress bar handling
            switch (e.ProgressPercentage)
            {
                #region Progress bar settings
                case 101:                           // 101: Progressbar is not Indeterminate
                    ProgrBar.IsIndeterminate = false;
                    break;
                case 102:                           // 102: Progressbar is Indeterminate
                    ProgrBar.IsIndeterminate = true;
                    break;
                #endregion

                #region Standard message handling
                case 201:                           // 201: ProgText -> Capturing disk (Generic; Disabled by default)
                    ProgrText.Text = $"Capturing disk ({ProgrBar.Value}%) ...";
                    break;
                case 202:                           // 202: ProgText -> Capturing disk
                    ProgrText.Text = $"Copying files to image ({ProgrBar.Value}%) ...";
                    break;
                case 203:                           // 203: ProgText -> Capturing disk
                    ProgrText.Text = $"Scanning disk ({ProgrBar.Value}%) ...";
                    break;
                case 204:                           // 204: ProgText -> Capturing disk
                    ProgrText.Text = $"Compressing image ({ProgrBar.Value}%) ...";
                    break;
                case 250:                           // 250: Installation complete
                    ProgrText.Text = "Capturing completed. Press 'Next' to exit Dive.";
                    ProgrBar.Value = 100;
                    if (CaptureContent.ContentWindow != null)
                        CaptureContent.ContentWindow.NextBtn.IsEnabled = true;
                    break;
                #endregion

                #region Error message handling
                case 301:                           // 301: Failed at capturing disk
                    ProgrText.Text = "Failed at capturing disk. Please check your disk and try again.";
                    ProgrBar.Value = 0;
                    if (CaptureContent.ContentWindow != null)
                    {
                        CaptureContent.ContentWindow.NextBtn.IsEnabled = false;
                        CaptureContent.ContentWindow.BackBtn.IsEnabled = false;
                        CaptureContent.ContentWindow.CancelBtn.IsEnabled = true;
                    }
                    IsCanceled = true;
                    break;
                #endregion
            }

            // Progressbar percentage
            if (e.ProgressPercentage <= 100)
            {
                this.ProgrBar.Value = e.ProgressPercentage;

                string currentAction = ProgrText.Text;
                Match match = Regex.Match(currentAction, @"\((\d+)%\)");

                if (match.Success)
                {
                    int value = int.Parse(match.Groups[1].Value);

                    value = e.ProgressPercentage;
                    ProgrText.Text = currentAction.Replace(match.Value, "(" + value + "%)");
                }
            }
                
        }

        private void CaptureWim(object? sender, DoWorkEventArgs e)
        {
            #region Environment definition

            BackgroundWorker worker = sender as BackgroundWorker;
            Entities.UI ui = Entities.UI.Graphical;

            #endregion

            // Initialize worker progress
            worker.ReportProgress(0, "");       // Value 0

            // Capture disk
            worker.ReportProgress(101, "");     // not Indeterminate
            worker.ReportProgress(201, "");     // Capture Disk Text
            Actions.CaptureToWIM(ui, Common.CaptureInfo.Name, Common.CaptureInfo.Description, Common.CaptureInfo.PathToCapture, FullFilePath, worker);
            if (IsCanceled)
            {
                e.Cancel = true;
                return;
            }

            // Installation complete
            worker.ReportProgress(250, "");     // Capturing complete Text
        }
    }
}
