using System.Globalization;
using AccountingApp.Core.Services;

namespace AccountingApp.Services;

public interface ILocalizedFormattingService
{
    string FormatMonthYear(DateTime date);
    string FormatYear(DateTime date);
    string FormatCategoryReportPeriod(ExpenseCategoryReportRange range, DateTime anchorDate);
}

public sealed class LocalizedFormattingService : ILocalizedFormattingService
{
    private readonly ILocalizationService _localizationService;

    public LocalizedFormattingService(ILocalizationService localizationService)
    {
        _localizationService = localizationService;
    }

    public string FormatMonthYear(DateTime date)
    {
        return IsTraditionalChinese()
            ? date.ToString("yyyy年MM月", CultureInfo.CurrentUICulture)
            : date.ToString("MMMM yyyy", CultureInfo.CurrentUICulture);
    }

    public string FormatYear(DateTime date)
    {
        return date.ToString("yyyy", CultureInfo.CurrentUICulture);
    }

    public string FormatCategoryReportPeriod(ExpenseCategoryReportRange range, DateTime anchorDate)
    {
        if (range == ExpenseCategoryReportRange.All)
        {
            return _localizationService.GetString("AllPeriodLabel");
        }

        if (range == ExpenseCategoryReportRange.Month)
        {
            return FormatMonthYear(anchorDate);
        }

        if (range == ExpenseCategoryReportRange.Year)
        {
            return FormatYear(anchorDate);
        }

        var window = ExpenseCategoryReport.GetDateWindow(range, anchorDate);
        var endDate = window.EndExclusive!.Value.AddDays(-1);
        return IsTraditionalChinese()
            ? $"{window.Start:yyyy/MM/dd} - {endDate:MM/dd}"
            : $"{window.Start:MMM d, yyyy} - {endDate:MMM d, yyyy}";
    }

    private static bool IsTraditionalChinese()
    {
        return string.Equals(CultureInfo.CurrentUICulture.Name, LocalizationService.DefaultLanguage, StringComparison.OrdinalIgnoreCase)
            || string.Equals(CultureInfo.CurrentUICulture.TwoLetterISOLanguageName, "zh", StringComparison.OrdinalIgnoreCase);
    }
}
