using System.Collections.ObjectModel;
using System.Windows.Input;
using CalendarMonth = AccountingApp.Core.Services.CalendarMonth;

namespace AccountingApp.Views.Controls;

public partial class CalendarDatePicker : ContentView
{
    public class CalendarDayItem
    {
        public DateTime Date { get; init; }
        public string Label { get; init; } = string.Empty;
        public Color TextColor { get; init; } = Colors.Black;
        public Color BackgroundColor { get; init; } = Colors.Transparent;
        public Color BorderColor { get; init; } = Colors.Transparent;
    }

    public static readonly BindableProperty DateProperty = BindableProperty.Create(
        nameof(Date),
        typeof(DateTime),
        typeof(CalendarDatePicker),
        DateTime.Today,
        BindingMode.TwoWay,
        propertyChanged: OnDatePropertyChanged);

    public static readonly BindableProperty ShowTriggerProperty = BindableProperty.Create(
        nameof(ShowTrigger),
        typeof(bool),
        typeof(CalendarDatePicker),
        true);

    private bool _isCalendarVisible;
    private DateTime _calendarMonth = new(DateTime.Today.Year, DateTime.Today.Month, 1);
    private string _calendarMonthText = $"{DateTime.Today:yyyy}\n{DateTime.Today:MM}";

    public CalendarDatePicker()
    {
        OpenCalendarCommand = new Command(OpenCalendar);
        CloseCalendarCommand = new Command(CloseCalendar);
        PreviousCalendarYearCommand = new Command(() => ChangeCalendarMonth(-12));
        PreviousCalendarMonthCommand = new Command(() => ChangeCalendarMonth(-1));
        NextCalendarMonthCommand = new Command(() => ChangeCalendarMonth(1));
        NextCalendarYearCommand = new Command(() => ChangeCalendarMonth(12));
        SelectCalendarDateCommand = new Command<CalendarDayItem>(SelectCalendarDate);
        InitializeComponent();
        SyncCalendarMonth(Date);
        RefreshCalendarDays();
    }

    public DateTime Date
    {
        get => (DateTime)GetValue(DateProperty);
        set => SetValue(DateProperty, value.Date);
    }

    public bool ShowTrigger
    {
        get => (bool)GetValue(ShowTriggerProperty);
        set => SetValue(ShowTriggerProperty, value);
    }

    public string DateDisplayText => Date.ToString("yyyy/MM/dd");

    public bool IsCalendarVisible
    {
        get => _isCalendarVisible;
        private set
        {
            if (_isCalendarVisible == value)
            {
                return;
            }

            _isCalendarVisible = value;
            OnPropertyChanged();
        }
    }

    public string CalendarMonthText
    {
        get => _calendarMonthText;
        private set
        {
            if (_calendarMonthText == value)
            {
                return;
            }

            _calendarMonthText = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<CalendarDayItem> CalendarDays { get; } = new();

    public ICommand OpenCalendarCommand { get; }
    public ICommand CloseCalendarCommand { get; }
    public ICommand PreviousCalendarYearCommand { get; }
    public ICommand PreviousCalendarMonthCommand { get; }
    public ICommand NextCalendarMonthCommand { get; }
    public ICommand NextCalendarYearCommand { get; }
    public ICommand SelectCalendarDateCommand { get; }
    public event EventHandler? CalendarOpened;
    public event EventHandler? CalendarCompleted;

    private static void OnDatePropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (CalendarDatePicker)bindable;
        control.OnPropertyChanged(nameof(DateDisplayText));
        control.SyncCalendarMonth((DateTime)newValue);
        control.RefreshCalendarDays();
    }

    private void OpenCalendar()
    {
        SyncCalendarMonth(Date);
        RefreshCalendarDays();
        IsCalendarVisible = true;
        CalendarOpened?.Invoke(this, EventArgs.Empty);
    }

    private void CloseCalendar()
    {
        IsCalendarVisible = false;
        CalendarCompleted?.Invoke(this, EventArgs.Empty);
    }

    private void ChangeCalendarMonth(int offset)
    {
        _calendarMonth = _calendarMonth.AddMonths(offset);
        UpdateCalendarMonthText();
        RefreshCalendarDays();
    }

    private void SelectCalendarDate(CalendarDayItem? day)
    {
        if (day is null)
        {
            return;
        }

        Date = day.Date;
        CloseCalendar();
    }

    private void SyncCalendarMonth(DateTime date)
    {
        _calendarMonth = new DateTime(date.Year, date.Month, 1);
        UpdateCalendarMonthText();
    }

    private void UpdateCalendarMonthText()
    {
        CalendarMonthText = $"{_calendarMonth:yyyy}\n{_calendarMonth:MM}";
    }

    private void RefreshCalendarDays()
    {
        var cells = CalendarMonth.BuildGrid(_calendarMonth);
        CalendarDays.Clear();

        foreach (var cell in cells)
        {
            var isSelected = cell.Date.Date == Date.Date;
            var isToday = cell.Date.Date == DateTime.Today;
            CalendarDays.Add(new CalendarDayItem
            {
                Date = cell.Date,
                Label = cell.Date.Day.ToString(),
                TextColor = isSelected
                    ? Colors.White
                    : cell.IsCurrentMonth ? Colors.Black : Colors.Gray,
                BackgroundColor = isSelected
                    ? Color.FromArgb("#0F766E")
                    : Colors.Transparent,
                BorderColor = isToday && !isSelected
                    ? Color.FromArgb("#0F766E")
                    : Colors.Transparent
            });
        }
    }
}
