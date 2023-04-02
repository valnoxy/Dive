using System.Windows;

namespace deployaUI.Common
{
    internal class LocalizationManager
    {
        public static string LocalizeValue(string value)
        {
            var localizedValue = (string)Application.Current.MainWindow.FindResource(value);
            return string.IsNullOrEmpty(localizedValue) ? value : localizedValue;
        }
    }
}
