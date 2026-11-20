using Dive.Core.Common;
using Dive.UI.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Debug = Dive.UI.Common.UserInterface.Debug;

namespace Dive.UI
{
    /// <summary>
    /// Splash screen shown during startup. Runs all initialization steps
    /// (language, plugins, core, license, hardware check) before the main window opens.
    /// </summary>
    public partial class SplashScreen
    {
        private static readonly TimeSpan MinimumDisplayTime = TimeSpan.FromMilliseconds(1200);

        private bool _finished;

        /// <summary>
        /// True if all required initialization steps succeeded.
        /// </summary>
        public bool Succeeded { get; private set; }

        private sealed record Step(string StatusKey, Func<Task> Action, bool Required);

        public SplashScreen()
        {
            // Load the language first, so the splash screen itself is already localized.
            LocalizationManager.LoadLanguage(LocalizationManager.GetSystemLanguage().Code);

            InitializeComponent();

            var fvi = FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly().Location);
            VersionLabel.Text = $"Version {fvi.FileVersion}";
#if DEBUG
            BranchLabel.Text = "Development build";
#endif
            Closing += SplashScreen_OnClosing;
        }

        private async void SplashScreen_OnLoaded(object sender, RoutedEventArgs e)
        {
            var stopwatch = Stopwatch.StartNew();
            var steps = new List<Step>
            {
                new("SplashLoadingPlugins", LoadPluginsAsync, Required: false),
                new("SplashInitializingCore", () => Task.Run(Core.Actions.Initialize), Required: true),
                new("SplashCheckingLicense", () => Task.Run(CheckLicense), Required: false),
                new("SplashCheckingHardware", () => Task.Run(() => CPUCompatibility.IsCpuCompatible()), Required: false)
            };

            StatusProgress.IsIndeterminate = false;
            StatusProgress.Maximum = steps.Count;

            for (var i = 0; i < steps.Count; i++)
            {
                var step = steps[i];
                StatusText.SetResourceReference(System.Windows.Controls.TextBlock.TextProperty, step.StatusKey);
                Debug.WriteLine(LocalizationManager.LocalizeValue(step.StatusKey));

                // Let the UI render the new status before the step starts.
                await Dispatcher.Yield(DispatcherPriority.Background);

                try
                {
                    await step.Action();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Startup step '{step.StatusKey}' failed: {ex}", ConsoleColor.Red);
                    Logging.Log($"Startup step '{step.StatusKey}' failed: {ex}", Logging.LogLevel.ERROR);

                    if (step.Required)
                    {
                        System.Windows.MessageBox.Show(
                            $"{LocalizationManager.LocalizeValue("SplashFailed")}\n\n{ex.Message}",
                            "Dive", MessageBoxButton.OK, MessageBoxImage.Error);
                        Finish(success: false);
                        return;
                    }
                }

                StatusProgress.Value = i + 1;
            }

            StatusText.SetResourceReference(System.Windows.Controls.TextBlock.TextProperty, "SplashStarting");

            // Avoid a splash screen that only flashes up for a split second.
            var remaining = MinimumDisplayTime - stopwatch.Elapsed;
            if (remaining > TimeSpan.Zero)
                await Task.Delay(remaining);

            Finish(success: true);
        }

        private static async Task LoadPluginsAsync()
        {
            // Plugins may create UI objects, so they are loaded on the UI thread.
            var loadedPlugins = Plugin.PluginManager.LoadPlugins(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins"));
            Debug.WriteLine($"Loaded {loadedPlugins} plugin(s).");
            await Task.Yield();
            Plugin.PluginManager.InitPlugins();
            Debug.WriteLine("Initialized all plugins.");
        }

        private static void CheckLicense()
        {
            Common.Licensing.Validation.FindAndValidate();
        }

        private void Finish(bool success)
        {
            Succeeded = success;
            _finished = true;
            Close();
        }

        private void SplashScreen_OnClosing(object? sender, CancelEventArgs e)
        {
            // The splash screen cannot be closed while the initialization is running.
            if (!_finished)
                e.Cancel = true;
        }
    }
}
