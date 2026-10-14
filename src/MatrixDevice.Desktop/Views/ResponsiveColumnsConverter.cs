using System.Globalization;
using System.Windows.Data;

namespace MatrixDevice.Desktop;

public sealed class ResponsiveColumnsConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        var preferred = values.Length > 0 && values[0] is int count ? count : 5;
        var width = values.Length > 1 && values[1] is double available ? available : 700;
        return Math.Max(1, Math.Min(preferred, (int)Math.Floor(width / 135)));
    }
    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotSupportedException();
}
