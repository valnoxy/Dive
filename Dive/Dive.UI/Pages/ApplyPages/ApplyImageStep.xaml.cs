using Dive.UI.Common;
using System.Windows.Media;

namespace Dive.UI.Pages.ApplyPages
{
    /// <summary>
    /// Interaktionslogik für ApplyImageStep.xaml
    /// </summary>
    public partial class ApplyImageStep
    {
        //private readonly BackgroundWorker _applyBackgroundWorker = new();
        private static readonly ApplyDetails ApplyDetailsInstance = ApplyDetails.Instance;
        private static readonly DeploymentInfo DeploymentInfoInstance = DeploymentInfo.Instance;
        private static readonly OemInfo OemInfoInstance = OemInfo.Instance;

        public ApplyImageStep()
        {
            InitializeComponent();

            if (ApplyContent.ContentWindow != null)
            {
                ApplyContent.ContentWindow.NextBtn.IsEnabled = false;
                ApplyContent.ContentWindow.BackBtn.IsEnabled = false;
                ApplyContent.ContentWindow.CancelBtn.IsEnabled = false;
            }
            if (CloudContent.ContentWindow != null)
            {
                CloudContent.ContentWindow.NextBtn.IsEnabled = false;
                CloudContent.ContentWindow.BackBtn.IsEnabled = false;
                CloudContent.ContentWindow.CancelBtn.IsEnabled = false;
            }

            ApplyDetailsInstance.UseNTLDR = DiskSelectStep.ContentWindow!.IsNTLDRChecked();
            ApplyDetailsInstance.UseRecovery = DiskSelectStep.ContentWindow.IsRecoveryChecked();

            // Validate deployment settings
            if (OemInfoInstance.UseOemInfo)
            {
                if (string.IsNullOrEmpty(OemInfoInstance.SupportPhone)
                    && string.IsNullOrEmpty(OemInfoInstance.LogoPath)
                    && string.IsNullOrEmpty(OemInfoInstance.Manufacturer)
                    && string.IsNullOrEmpty(OemInfoInstance.Model)
                    && string.IsNullOrEmpty(OemInfoInstance.SupportHours)
                    && string.IsNullOrEmpty(OemInfoInstance.SupportURL))
                {
                    OemInfoInstance.UseOemInfo = false;
                }
            }

            if (DeploymentInfoInstance.UseUserInfo)
            {
                if (string.IsNullOrEmpty(DeploymentInfoInstance.Username)
                    && string.IsNullOrEmpty(DeploymentInfoInstance.Password))
                {
                    DeploymentInfoInstance.UseUserInfo = false;
                }
            }

            // Set active Image to card
            ImageName.Text = $"Installing {ApplyDetailsInstance.Name}";
            //ImageFile.Text = ApplyDetailsInstance.FileName;
            var img = new ImageSourceConverter();
            try
            {
                ImageIcon.Source = (ImageSource)img.ConvertFromString(ApplyDetailsInstance.IconPath)!;
            }
            catch
            {
                // ignored
            }
        }
    }
}
