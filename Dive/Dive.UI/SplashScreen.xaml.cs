using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Threading;

namespace Dive.UI
{
    /// <summary>
    /// Interaktionslogik für SplashScreen.xaml
    /// </summary>
    public partial class SplashScreen
    {
        public class Language
        {
            public string Name { get; set; }
            public string Code { get; set; }
        }

        private List<Language> languages;
        public SplashScreen()
        { 
            InitializeComponent();

            // Get application version
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            var version = fvi.FileVersion;
            VersionLabel.Text = $"Version {version}";
#if DEBUG
            VersionLabel.Text = $"Version {version} - Debug build";
#endif

            languages = new List<Language>()
            {
                new Language {Name = "English", Code = "en-US"}, // Default language
                new Language {Name = "Deutsch", Code = "de-DE"}
            };
            foreach (var language in languages)
            {
                LanguageDropDown.Items.Add(language.Name);
            }
            LanguageDropDown.SelectedIndex = 0;
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void LanguageDropDown_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Set current language model
            try
            {
                var language = LanguageDropDown.SelectedValue.ToString();
                if (string.IsNullOrEmpty(language)) return;

                var languageCode = languages.Find(x => x.Name == language)?.Code;
                Common.Debug.WriteLine($"Trying to load language '{language}' with code '{languageCode}' ...");
                Common.LocalizationManager.LoadLanguage(languageCode!);
            }
            catch (Exception ex) 
            {
                Common.Debug.WriteLine(ex.ToString(), ConsoleColor.Red);
            }
        }
    }
}
