using System;
using System.Windows;
using Dive.UI.Common.UserInterface;

namespace Dive.UI.Common
{
    internal class LocalizationManager
    {
        public static class CurrentLanguage
        {
            public static string Code { get; set; }
        }

        public static string LocalizeValue(string value)
        {
            try
            {
                var localizedValue = (string)Application.Current.MainWindow.FindResource(value);
                return string.IsNullOrEmpty(localizedValue) ? value : localizedValue;
            }
            catch
            {
                return value;
            }

        }

        public static void LoadLanguage(string languageCode)
        {
            // Set current language model
            try
            {
                var dict = new ResourceDictionary();
                var isValid = false;
                switch (languageCode)
                {
                    default:
                    case "en-US":
                        dict.Source = new Uri(@"/Dive;component/Localization/ResourceDictionary.xaml",
                            UriKind.Relative);
                        break;
                    case "de-DE":
                        dict.Source = new Uri(@"/Dive;component/Localization/ResourceDictionary.de-DE.xaml",
                            UriKind.Relative);
                        isValid = true;
                        break;
                }

                Application.Current.Resources.MergedDictionaries.Add(dict);
                CurrentLanguage.Code = isValid ? languageCode : "en-US";
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString(), ConsoleColor.Red);
            }
        }
    }
}
