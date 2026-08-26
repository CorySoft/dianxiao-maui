using System.Globalization;
using Microsoft.Maui.Controls;

namespace DianxiaoMaui.Converters;

/// <summary>筛选按钮样式转换器</summary>
public class FilterStyleConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int current && parameter is string p && int.TryParse(p, out var target))
        {
            return current == target
                ? Application.Current!.Resources["PrimaryButton"]
                : Application.Current!.Resources["SecondaryButton"];
        }
        return Application.Current!.Resources["SecondaryButton"];
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}