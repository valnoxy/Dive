using Dive.Core.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Debug = Dive.UI.Common.UserInterface.Debug;

namespace Dive.UI
{
    /// <summary>
    /// Interaktionslogik für SplashScreen.xaml
    /// </summary>
    public partial class SplashScreen
    {
        private static bool _switchToDashboard = false;

        public new class Language
        {
            public string? Name { get; set; }
            public string? Code { get; set; }
        }

        private readonly List<Language> _languages = new()
        {
            new Language { Name = "English", Code = "en-US" }, // Default language
            new Language { Name = "Deutsch", Code = "de-DE" }
        };

        public SplashScreen()
        {
            InitializeComponent();

            // Get application version
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            var version = fvi.FileVersion;
            VersionLabel.Text = $"Version {version}";
#if DEBUG
            VersionLabel.Text = $"Version {version}\nDebug build";
#endif

            foreach (var language in _languages)
            {
                LanguageDropDown.Items.Add(language.Name);
            }
            LanguageDropDown.SelectedIndex = 0;
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            _switchToDashboard = true;
            Close();
        }

        private void LanguageDropDown_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Set current language model
            try
            {
                var language = LanguageDropDown.SelectedValue.ToString();
                if (string.IsNullOrEmpty(language)) return;

                var languageCode = _languages.Find(x => x.Name == language)?.Code;
                Debug.WriteLine($"Trying to load language '{language}' with code '{languageCode}' ...");
                Logging.Log($"Trying to load language '{language}' with code '{languageCode}' ...");
                Common.LocalizationManager.LoadLanguage(languageCode!);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString(), ConsoleColor.Red);
            }
        }

        private void SplashScreen_OnClosing(object? sender, CancelEventArgs e)
        {
            if (!_switchToDashboard)
                Environment.Exit(0);
        }
    }
}
