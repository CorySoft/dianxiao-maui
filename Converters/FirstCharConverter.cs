using System.Globalization;
using Microsoft.Maui.Controls;

namespace DianxiaoMaui.Converters;

/// <summary>取字符串首字符（用于头像显示）</summary>
public class FirstCharConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string s && !string.IsNullOrWhiteSpace(s))
        {
            var c = s.Trim()[0];
            return char.IsLetterOrDigit(c) ? c.ToString().ToUpper() : "?";
        }
        return "?";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}