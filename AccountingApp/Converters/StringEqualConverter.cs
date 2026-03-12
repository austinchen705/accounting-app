using System.Globalization;

namespace AccountingApp.Converters;

public class StringEqualConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (parameter is null)
        {
            return false;
        }

        var current = value?.ToString();
        var expected = parameter.ToString();
        return string.Equals(current, expected, StringComparison.OrdinalIgnoreCase);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isChecked && isChecked && parameter is not null)
        {
            return parameter.ToString() ?? string.Empty;
        }

        return BindableProperty.UnsetValue;
    }
}
