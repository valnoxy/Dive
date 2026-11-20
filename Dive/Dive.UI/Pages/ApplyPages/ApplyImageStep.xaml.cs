using Dive.Core.Common;
using Dive.UI.Common;
using Dive.UI.Common.Deployment;
using Dive.UI.Common.UserInterface;
using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Wpf.Ui.Controls;
using TextBlock = System.Windows.Controls.TextBlock;

namespace Dive.UI.Pages.ApplyPages
{
    /// <summary>
    /// Interaktionslogik für ApplyImageStep.xaml
    /// </summary>
    public partial class ApplyImageStep
    {
        private readonly BackgroundWorker _applyBackgroundWorker = new();
        private static readonly ApplyDetails ApplyDetailsInstance = ApplyDetails.Instance;
        private static readonly DeploymentInfo DeploymentInfoInstance = DeploymentInfo.Instance;
        private static readonly OemInfo OemInfoInstance = OemInfo.Instance;

        private static readonly Brush DoneBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#393d1b"));
        private static readonly Brush FailedBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4a1f1f"));

        private enum CardState { Running, Done, Failed }

        /// <summary>
        /// One status card of the page (e.g. "Copy Windows").
        /// </summary>
        private sealed record StatusCard(Card Card, TextBlock Percentage, UIElement ProgressRing, UIElement Check, UIElement Error);

        private readonly StatusCard[] _cards;
        private int _activeCard = -1;

        public ApplyImageStep()
        {
            InitializeComponent();

            _cards = new[]
            {
                new StatusCard(CopyWindowsCard, CopyWindowsPercentage, CopyWindowsProgRing, CopyWindowsCheck, CopyWindowsError),
                new StatusCard(FinalizeDiskCard, FinalizeDiskPercentage, FinalizeDiskProgRing, FinalizeDiskCheck, FinalizeDiskError),
                new StatusCard(FeaturesAndDriversCard, FeaturesAndDriversPercentage, FeaturesAndDriversProgRing, FeaturesAndDriversCheck, FeaturesAndDriversError)
            };

            SetNavigation(next: false, back: false, cancel: false);

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
            var img = new ImageSourceConverter();
            try
            {
                ImageIcon.Source = (ImageSource)img.ConvertFromString(ApplyDetailsInstance.IconPath)!;
            }
            catch
            {
                // ignored
            }

            // Background worker for deployment
            _applyBackgroundWorker.WorkerReportsProgress = true;
            _applyBackgroundWorker.DoWork += ApplyImage;
            _applyBackgroundWorker.ProgressChanged += ApplyBackgroundWorker_ProgressChanged;
            _applyBackgroundWorker.RunWorkerCompleted += ApplyBackgroundWorker_RunWorkerCompleted;
            _applyBackgroundWorker.RunWorkerAsync();
        }

        private void ApplyImage(object? sender, DoWorkEventArgs e) => DeployImage.ApplyWindowsImage(_applyBackgroundWorker);

        private void ApplyBackgroundWorker_ProgressChanged(object? sender, ProgressChangedEventArgs e)
        {
            if (e.UserState is not ActionWorker response) return;

            try
            {
                if (response.IsError)
                {
                    // Errors abort the deployment via DeploymentException; keep this only for logging.
                    Debug.WriteLine(response.Message, ConsoleColor.Red);
                    return;
                }

                if (response.IsWarning)
                {
                    Debug.WriteLine(response.Message, ConsoleColor.Yellow);
                    return;
                }

                if (response.IsDebug)
                {
                    Debug.WriteLine(response.Message);
                    return;
                }

                var cardIndex = GetCardIndex(response.Action);
                ActivateCard(cardIndex);

                ProgrText.Text = response.Message;
                if (response.IsIndeterminate)
                {
                    ProgrBar.IsIndeterminate = true;
                }
                else
                {
                    ProgrBar.IsIndeterminate = false;
                    ProgrBar.Value = e.ProgressPercentage;
                }

                if (response.Action == Progress.ApplyImage && !response.IsIndeterminate)
                {
                    _cards[cardIndex].Percentage.Visibility = Visibility.Visible;
                    _cards[cardIndex].Percentage.Text = $"{e.ProgressPercentage}%";
                }

                Debug.RefreshProgressBar(response.Action, e.ProgressPercentage, response.Message);
                if (Common.Configuration.FunConfig.WledCommunication)
                    _ = Common.Fun.WLED.IncreaseProgress(e.ProgressPercentage);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error while parsing response: {ex.Message}", ConsoleColor.Red);
            }
        }

        private void ApplyBackgroundWorker_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                ShowFailure(e.Error);
                return;
            }

            for (var i = 0; i < _cards.Length; i++)
                SetCardState(_cards[i], CardState.Done);

            ProgrBar.IsIndeterminate = false;
            ProgrBar.Value = 100;
            ProgrText.Text = File.Exists("X:\\Windows\\System32\\wpeutil.exe")
                ? "Installation completed. Press 'Next' to restart your computer."
                : "Installation completed. Press 'Next' to close Dive.";

            Debug.WriteLine("Deployment completed successfully.", ConsoleColor.Green);
            SetNavigation(next: true, back: false, cancel: false);
        }

        private void ShowFailure(Exception error)
        {
            var step = error is DeploymentException deploymentError ? deploymentError.Step : (Progress?)null;
            var message = DeployImage.DescribeError(error);

            // Mark the affected card as failed (fall back to the card that was running).
            var cardIndex = step is { } failedStep ? GetCardIndex(failedStep) : Math.Max(_activeCard, 0);
            for (var i = 0; i < cardIndex; i++)
                SetCardState(_cards[i], CardState.Done);
            SetCardState(_cards[cardIndex], CardState.Failed);

            ProgrBar.IsIndeterminate = false;
            ProgrText.Text = step is null
                ? $"Installation failed: {message}"
                : $"Installation failed ({step}): {message}";

            Debug.WriteLine($"Deployment failed{(step is null ? string.Empty : $" at step {step}")}: {message}", ConsoleColor.Red);
            Debug.WriteLine(error.ToString(), ConsoleColor.DarkGray);
            Core.Common.Logging.Log($"Deployment failed: {error}", Core.Common.Logging.LogLevel.ERROR);

            SetNavigation(next: false, back: false, cancel: true);
        }

        private void ActivateCard(int cardIndex)
        {
            if (cardIndex == _activeCard) return;

            for (var i = 0; i < cardIndex; i++)
                SetCardState(_cards[i], CardState.Done);
            SetCardState(_cards[cardIndex], CardState.Running);
            _activeCard = cardIndex;
        }

        private static void SetCardState(StatusCard card, CardState state)
        {
            card.ProgressRing.Visibility = state == CardState.Running ? Visibility.Visible : Visibility.Hidden;
            card.Check.Visibility = state == CardState.Done ? Visibility.Visible : Visibility.Hidden;
            card.Error.Visibility = state == CardState.Failed ? Visibility.Visible : Visibility.Hidden;
            if (state != CardState.Running)
                card.Percentage.Visibility = Visibility.Hidden;

            card.Card.Background = state switch
            {
                CardState.Done => DoneBrush,
                CardState.Failed => FailedBrush,
                _ => card.Card.Background
            };
        }

        /// <summary>
        /// Maps a deployment step to the status card that represents it.
        /// </summary>
        private static int GetCardIndex(Progress step) => step switch
        {
            Progress.PrepareDisk or Progress.ApplyImage => 0,
            Progress.InstallBootloader or Progress.InstallRecovery => 1,
            _ => 2
        };

        private static void SetNavigation(bool next, bool back, bool cancel)
        {
            if (ApplyContent.ContentWindow != null)
            {
                ApplyContent.ContentWindow.NextBtn.IsEnabled = next;
                ApplyContent.ContentWindow.BackBtn.IsEnabled = back;
                ApplyContent.ContentWindow.CancelBtn.IsEnabled = cancel;
            }

            if (CloudContent.ContentWindow != null)
            {
                CloudContent.ContentWindow.NextBtn.IsEnabled = next;
                CloudContent.ContentWindow.BackBtn.IsEnabled = back;
                CloudContent.ContentWindow.CancelBtn.IsEnabled = cancel;
            }
        }
    }
}
