using System.Globalization;
using System.Windows.Data;

namespace AppTrack.WpfUi.Converter;

public class InverseBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool b)
            return !b;
        return value;
    }

#pragma warning disable S4144 // Methods should not have identical implementations
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
#pragma warning restore S4144 // Methods should not have identical implementations
    {
        if (value is bool b)
            return !b;
        return value;
    }
}
