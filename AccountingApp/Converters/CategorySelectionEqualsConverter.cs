using System.Globalization;
using AccountingApp.Core.Abstractions;

namespace AccountingApp.Converters;

public class CategorySelectionEqualsConverter : IMultiValueConverter
{
    public object Convert(object?[] values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Length < 2)
        {
            return false;
        }

        if (values[0] is not IFrequentCategorySourceCategory category ||
            values[1] is not IFrequentCategorySourceCategory selectedCategory)
        {
            return false;
        }

        return category.Id == selectedCategory.Id;
    }

    public object[] ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
