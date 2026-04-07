namespace AccountingApp.Tests;

public class CalendarDatePickerLayoutTests
{
    [Fact]
    public void CalendarDatePicker_exposes_year_and_month_navigation_actions()
    {
        var xaml = ReadCalendarDatePickerXaml();
        var code = ReadCalendarDatePickerCodeBehind();

        Assert.Contains("Text=\"«\"", xaml);
        Assert.Contains("Text=\"‹\"", xaml);
        Assert.Contains("Text=\"›\"", xaml);
        Assert.Contains("Text=\"»\"", xaml);
        Assert.Contains("PreviousCalendarYearCommand", xaml);
        Assert.Contains("NextCalendarYearCommand", xaml);
        Assert.Contains("TextColor=\"{AppThemeBinding Light={StaticResource TextPrimary}, Dark={StaticResource DarkTextPrimary}}\"", xaml);

        Assert.Contains("PreviousCalendarYearCommand = new Command(() => ChangeCalendarMonth(-12));", code);
        Assert.Contains("NextCalendarYearCommand = new Command(() => ChangeCalendarMonth(12));", code);
        Assert.Contains("OpenCalendarCommand", code);
        Assert.Contains("CloseCalendarCommand", code);
        Assert.Contains("SelectCalendarDateCommand", code);
        Assert.Contains("public event EventHandler? CalendarOpened;", code);
        Assert.Contains("public event EventHandler? CalendarCompleted;", code);
        Assert.Contains("LineBreakMode=\"WordWrap\"", xaml);
        Assert.Contains("MaxLines=\"2\"", xaml);
        Assert.Contains("\\n{DateTime.Today:MM}", code);
        Assert.Contains("\\n{_calendarMonth:MM}", code);
    }

    private static string ReadCalendarDatePickerXaml()
    {
        var path = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "../../../../AccountingApp/Views/Controls/CalendarDatePicker.xaml"));

        return File.ReadAllText(path);
    }

    private static string ReadCalendarDatePickerCodeBehind()
    {
        var path = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "../../../../AccountingApp/Views/Controls/CalendarDatePicker.xaml.cs"));

        return File.ReadAllText(path);
    }
}
