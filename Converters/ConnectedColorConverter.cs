using System.Globalization;
using Microsoft.Maui.Controls;

namespace DianxiaoMaui.Converters;

/// <summary>通话接通状态转颜色</summary>
public class ConnectedColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool connected)
        {
            if (Application.Current?.Resources is not null)
            {
                return connected
                    ? (Color)Application.Current.Resources["SuccessColor"]!
                    : (Color)Application.Current.Resources["DangerColor"]!;
            }
        }
        return Colors.Gray;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}