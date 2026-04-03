using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace SmrtDoodle.Helpers;

public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        bool invert = parameter is string s && s.Equals("Invert", StringComparison.OrdinalIgnoreCase);
        bool boolValue = value is bool b && b;
        if (invert) boolValue = !boolValue;
        return boolValue ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        value is Visibility v && v == Visibility.Visible;
}

public class PercentageConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is float f) return $"{f * 100:0}%";
        if (value is double d) return $"{d * 100:0}%";
        return "100%";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is string s && s.TrimEnd('%') is var trimmed && float.TryParse(trimmed, out var result))
            return result / 100f;
        return 1.0f;
    }
}
