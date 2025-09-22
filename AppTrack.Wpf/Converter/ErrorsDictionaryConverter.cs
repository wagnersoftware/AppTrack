using System.Globalization;
using System.Windows.Data;

namespace AppTrack.WpfUi.Converter;

/// <summary>
/// Prevents Data Error when no data is found in Error dictionary.
/// </summary>
public class ErrorsDictionaryConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is IDictionary<string, List<string>> dict && parameter is string key)
        {
            if (dict.TryGetValue(key, out var errors))
            {
                return errors;
            }

            return new List<string>();
        }
        return new List<string>();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
