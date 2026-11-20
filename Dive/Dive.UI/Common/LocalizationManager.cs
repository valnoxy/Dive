using Dive.UI.Common.UserInterface;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;

namespace Dive.UI.Common
{
    internal class LocalizationManager
    {
        public sealed record Language(string Name, string Code, string ResourcePath);

        /// <summary>
        /// All languages offered in the language menu. The first entry is the fallback.
        /// </summary>
        public static IReadOnlyList<Language> Languages { get; } = new[]
        {
            new Language("English", "en-US", "/Dive;component/Localization/ResourceDictionary.xaml"),
            new Language("Deutsch", "de-DE", "/Dive;component/Localization/ResourceDictionary.de-DE.xaml")
        };

        public static class CurrentLanguage
        {
            public static string Code { get; set; } = Languages[0].Code;
        }

        /// <summary>
        /// Raised after a language has been loaded.
        /// </summary>
        public static event Action<Language>? LanguageChanged;

        private static ResourceDictionary? _activeDictionary;

        public static string LocalizeValue(string value)
        {
            try
            {
                var localizedValue = Application.Current.TryFindResource(value) as string;
                return string.IsNullOrEmpty(localizedValue) ? value : localizedValue;
            }
            catch
            {
                return value;
            }
        }

        /// <summary>
        /// Returns the language matching the system UI language, or English.
        /// </summary>
        public static Language GetSystemLanguage()
        {
            var culture = CultureInfo.CurrentUICulture;
            return Languages.FirstOrDefault(l => string.Equals(l.Code, culture.Name, StringComparison.OrdinalIgnoreCase))
                   ?? Languages.FirstOrDefault(l => l.Code.StartsWith(culture.TwoLetterISOLanguageName + "-", StringComparison.OrdinalIgnoreCase))
                   ?? Languages[0];
        }

        public static void LoadLanguage(string languageCode)
        {
            var language = Languages.FirstOrDefault(l => l.Code == languageCode) ?? Languages[0];

            try
            {
                var dict = new ResourceDictionary { Source = new Uri(language.ResourcePath, UriKind.Relative) };
                var merged = Application.Current.Resources.MergedDictionaries;

                // Replace the previously loaded language instead of stacking dictionaries.
                if (_activeDictionary != null)
                    merged.Remove(_activeDictionary);
                merged.Add(dict);
                _activeDictionary = dict;

                CurrentLanguage.Code = language.Code;
                Debug.WriteLine($"Language set to '{language.Name}' ({language.Code}).");
                LanguageChanged?.Invoke(language);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to load language '{languageCode}': {ex}", ConsoleColor.Red);
            }
        }
    }
}
