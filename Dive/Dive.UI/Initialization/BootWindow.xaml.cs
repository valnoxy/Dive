using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using static Dive.AutoInit.AppxManagement;

namespace Dive.UI.Initialization
{
    /// <summary>
    /// Interaktionslogik für BootWindow.xaml
    /// </summary>
    public partial class BootWindow : Window
    {
        public Configuration? Config { get; set; }
        private BackgroundWorker? _applyBackgroundWorker;
        private string? _currentAction;

        public BootWindow(Configuration? config)
        {
            InitializeComponent();

            if (config == null)
            {
                const string message = "This playbook contains an invalid configuration. ";
                const string title = "Dive - AutoInit Module";
                const string btn1 = "OK";

                var w = new MessageUI(title, message, btn1, null, true);
                w.ShowDialog();
                Environment.Exit(1);
            }

            Config = config;

            _applyBackgroundWorker = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };
            _applyBackgroundWorker.DoWork += CounterProgr;
            _applyBackgroundWorker.ProgressChanged += ProgressChanged;
            // _applyBackgroundWorker.RunWorkerAsync();
        }

        private bool _easterEggIsEnabled = false;

        private void MainWindow_OnKeyDown(object sender, KeyEventArgs e)
        {
            // Check name of Author
            if (!new[] { "exploitox", "wolkenhof" }.Any(c => Config!.Author.ToLower().Contains(c)))
                return;

            if (Keyboard.Modifiers != ModifierKeys.Control || e.Key != Key.F) return;
            if (!_easterEggIsEnabled)
            {
                EasterEgg.Visibility = Visibility.Visible;
                ProgressRing.Visibility = Visibility.Hidden;
                _easterEggIsEnabled = true;
            }
            else
            {
                EasterEgg.Visibility = Visibility.Hidden;
                ProgressRing.Visibility = Visibility.Visible;
                _easterEggIsEnabled = false;
            }
        }

        private void ProgressChanged(object? sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage < 100)
            {
                if (!string.IsNullOrEmpty((string?)e.UserState))
                {
                    TxtStatus.Text = $"{e.UserState} {e.ProgressPercentage}%\nBitte lassen Sie Ihren PC eingeschaltet.";
                    _currentAction = (string)e.UserState;
                }
                else
                {
                    TxtStatus.Text = $"{_currentAction} {e.ProgressPercentage}%\nBitte lassen Sie Ihren PC eingeschaltet.";
                }
            }
            else
            {
                switch (e.ProgressPercentage)
                {
                    case 101:
                        return;
                }
            }
        }

        private void CounterProgr(object? sender, DoWorkEventArgs e)
        {
            var worker = sender as BackgroundWorker;
            var config = e.Argument as Configuration;
            if (worker == null || config == null)
                return;

            var total = config.Removal.Count
                        + 1 // Telemetry
                        + 1 // Tweaks
                        + 1;

            var current = 0;

            // Remove Appx Provision Packages
            foreach (var removal in config.Removal)
            {
                worker.ReportProgress((int)Math.Round((double)current / total * 100), $"Bereitstellungspaket der App \"{removal.Name}\" wird entfernt");
                try
                {
                    RemoveAppXProvisionedPackage(removal.PackageId);
                }
                catch (Exception ex)
                {
                    //Core.Logger.Log($"Failed to remove app '{removal.Name}' with ID '{removal.PackageId}': {ex.Message}");
                }
                current++;
            }

            // Apply Telemetry settings
            worker.ReportProgress((int)Math.Round((double)current / total * 100), "Anpassen der Telemetrieeinstellungen");
            Action.Telemetry(config);
            current++;

            // Apply Tweaks
            try
            {
                worker.ReportProgress((int)Math.Round((double)current / total * 100), "Update ");
                if (config.Tweaks.CleanupStartPins)
                    AutoInit.Tweaks.CleanupStartPins();
                if (config.Tweaks.DisableAutoRebootOnBsod)
                    AutoInit.Tweaks.DisableAutoRebootOnBSOD();
                if (config.Tweaks.DisableFastBoot)
                    AutoInit.Tweaks.DisableFastBoot();
                if (config.Tweaks.SetMaxPerformance)
                    AutoInit.Tweaks.SetMaxPerformance();
                if (config.Tweaks.SetMaxMemoryDumpSize != null)
                    AutoInit.Tweaks.SetMaxMemDump(config.Tweaks.SetMaxMemoryDumpSize);
                if (config.Tweaks.ShadowStorage != 0)
                    AutoInit.Tweaks.ShadowStorage(config.Tweaks.ShadowStorage);
            }
            catch
            {
                // log error
            }
            finally
            {
                current++;
            }
        }
    }
}
