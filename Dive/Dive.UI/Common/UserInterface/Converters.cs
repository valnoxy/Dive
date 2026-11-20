using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Dive.UI.Common.UserInterface
{
    /// <summary>
    /// True if the bound enum value equals the converter parameter (enum member name).
    /// ConvertBack sets the enum to the parameter when a radio button is checked.
    /// </summary>
    public sealed class EnumEqualsConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            value != null && parameter != null && string.Equals(value.ToString(), parameter.ToString(), StringComparison.Ordinal);

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            value is true && parameter != null ? Enum.Parse(targetType, parameter.ToString()!) : Binding.DoNothing;
    }

    /// <summary>
    /// Visible if the bound enum value is one of the comma separated names in the converter parameter.
    /// </summary>
    public sealed class EnumToVisibilityConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null || parameter == null) return Visibility.Collapsed;
            foreach (var name in parameter.ToString()!.Split(','))
            {
                if (string.Equals(value.ToString(), name.Trim(), StringComparison.Ordinal))
                    return Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            throw new NotSupportedException();
    }
}
