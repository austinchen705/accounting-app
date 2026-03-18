using System.Collections.ObjectModel;
using System.Threading;
using System.Windows.Input;
using AccountingApp.Core.Services;
using AccountingApp.Services;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace AccountingApp.ViewModels;

public class CategoryReportViewModel : BindableObject
{
    private readonly ILocalizedFormattingService _localizedFormattingService;
    private readonly ILocalizationService _localizationService;
    public class CategoryReportItem
    {
        public string CategoryName { get; set; } = string.Empty;
        public string TransactionCountText { get; set; } = "0";
        public string AmountText { get; set; } = "0";
        public string PercentageText { get; set; } = "0%";
        public Color DotColor { get; set; } = Colors.Gray;
    }

    private readonly StatisticsService _statisticsService;
    private readonly DataRefreshService _refreshService;
    private DateTime _anchorDate = DateTime.Today;
    private ExpenseCategoryReportRange _selectedRange = ExpenseCategoryReportRange.Month;
    private ISeries[] _pieSeries = Array.Empty<ISeries>();
    private bool _hasCategoryData;
    private string _periodLabel = string.Empty;
    private string _totalExpenseText = "0";
    private string _currencyText = "TWD";
    private bool _canNavigatePeriods = true;
    private Color _weekButtonBackgroundColor = Color.FromArgb("#007AFF");
    private Color _weekButtonTextColor = Colors.White;
    private Color _monthButtonBackgroundColor = Color.FromArgb("#007AFF");
    private Color _monthButtonTextColor = Colors.White;
    private Color _yearButtonBackgroundColor = Color.FromArgb("#F2F4F7");
    private Color _yearButtonTextColor = Color.FromArgb("#007AFF");
    private Color _allButtonBackgroundColor = Color.FromArgb("#F2F4F7");
    private Color _allButtonTextColor = Color.FromArgb("#007AFF");
    private int _loadVersion;

    public ObservableCollection<CategoryReportItem> CategoryItems { get; } = new();

    public ISeries[] PieSeries
    {
        get => _pieSeries;
        set { _pieSeries = value; OnPropertyChanged(); }
    }

    public bool HasCategoryData
    {
        get => _hasCategoryData;
        set { _hasCategoryData = value; OnPropertyChanged(); }
    }

    public string PeriodLabel
    {
        get => _periodLabel;
        set { _periodLabel = value; OnPropertyChanged(); }
    }

    public string TotalExpenseText
    {
        get => _totalExpenseText;
        set { _totalExpenseText = value; OnPropertyChanged(); }
    }

    public string CurrencyText
    {
        get => _currencyText;
        set { _currencyText = value; OnPropertyChanged(); }
    }

    public bool CanNavigatePeriods
    {
        get => _canNavigatePeriods;
        set { _canNavigatePeriods = value; OnPropertyChanged(); }
    }

    public Color WeekButtonBackgroundColor
    {
        get => _weekButtonBackgroundColor;
        set { _weekButtonBackgroundColor = value; OnPropertyChanged(); }
    }

    public Color WeekButtonTextColor
    {
        get => _weekButtonTextColor;
        set { _weekButtonTextColor = value; OnPropertyChanged(); }
    }

    public Color MonthButtonBackgroundColor
    {
        get => _monthButtonBackgroundColor;
        set { _monthButtonBackgroundColor = value; OnPropertyChanged(); }
    }

    public Color MonthButtonTextColor
    {
        get => _monthButtonTextColor;
        set { _monthButtonTextColor = value; OnPropertyChanged(); }
    }

    public Color YearButtonBackgroundColor
    {
        get => _yearButtonBackgroundColor;
        set { _yearButtonBackgroundColor = value; OnPropertyChanged(); }
    }

    public Color YearButtonTextColor
    {
        get => _yearButtonTextColor;
        set { _yearButtonTextColor = value; OnPropertyChanged(); }
    }

    public Color AllButtonBackgroundColor
    {
        get => _allButtonBackgroundColor;
        set { _allButtonBackgroundColor = value; OnPropertyChanged(); }
    }

    public Color AllButtonTextColor
    {
        get => _allButtonTextColor;
        set { _allButtonTextColor = value; OnPropertyChanged(); }
    }

    public ICommand PreviousPeriodCommand { get; }
    public ICommand NextPeriodCommand { get; }
    public ICommand SetRangeCommand { get; }

    public CategoryReportViewModel(
        StatisticsService statisticsService,
        ILocalizedFormattingService localizedFormattingService,
        ILocalizationService localizationService,
        DataRefreshService refreshService)
    {
        _statisticsService = statisticsService;
        _localizedFormattingService = localizedFormattingService;
        _localizationService = localizationService;
        _refreshService = refreshService;
        PreviousPeriodCommand = new Command(() => ChangePeriod(-1));
        NextPeriodCommand = new Command(() => ChangePeriod(1));
        SetRangeCommand = new Command<string>(SetRange);
        UpdateRangeState();
        _refreshService.DataChanged += OnDataChanged;
    }

    public async Task LoadAsync()
    {
        var version = Interlocked.Increment(ref _loadVersion);
        var summary = await _statisticsService.GetExpenseCategoryReportAsync(_selectedRange, _anchorDate);
        if (version != _loadVersion)
        {
            return;
        }

        CurrencyText = string.Format(_localizationService.GetString("UnitPrefix"), Preferences.Get("base_currency", "TWD"));
        TotalExpenseText = summary.TotalExpense.ToString("N0");
        HasCategoryData = summary.Categories.Count > 0;

        if (!HasCategoryData)
        {
            PieSeries = Array.Empty<ISeries>();
            CategoryItems.Clear();
            return;
        }

        var total = summary.TotalExpense;
        var series = new List<ISeries>(summary.Categories.Count);
        var colorByCategory = CategoryColorPalette.BuildDistinctHexColors(
            summary.Categories.Select(category => category.CategoryName));
        CategoryItems.Clear();

        foreach (var category in summary.Categories)
        {
            var color = SKColor.Parse(colorByCategory[category.CategoryName]);
            var ratio = total <= 0 ? 0 : category.Amount / total;
            series.Add(new PieSeries<double>
            {
                Values = [Convert.ToDouble(category.Amount)],
                Name = category.CategoryName,
                InnerRadius = 75,
                Fill = new SolidColorPaint(color),
                Stroke = new SolidColorPaint(SKColors.White) { StrokeThickness = 1 },
                DataLabelsPaint = new SolidColorPaint(SKColors.Transparent),
                HoverPushout = 4
            });

            CategoryItems.Add(new CategoryReportItem
            {
                CategoryName = category.CategoryName,
                TransactionCountText = category.TransactionCount.ToString(),
                AmountText = category.Amount.ToString("N0"),
                PercentageText = ratio.ToString("P0"),
                DotColor = Color.FromRgb(color.Red, color.Green, color.Blue)
            });
        }

        PieSeries = series.ToArray();
    }

    private void SetRange(string? rangeText)
    {
        if (!Enum.TryParse<ExpenseCategoryReportRange>(rangeText, ignoreCase: true, out var range))
        {
            return;
        }

        if (_selectedRange == range)
        {
            return;
        }

        _selectedRange = range;
        if (_selectedRange is ExpenseCategoryReportRange.Month)
        {
            _anchorDate = new DateTime(_anchorDate.Year, _anchorDate.Month, 1);
        }
        else if (_selectedRange is ExpenseCategoryReportRange.Year)
        {
            _anchorDate = new DateTime(_anchorDate.Year, 1, 1);
        }

        UpdateRangeState();
        _ = LoadAsync();
    }

    private void ChangePeriod(int delta)
    {
        if (!CanNavigatePeriods)
        {
            return;
        }

        _anchorDate = _selectedRange switch
        {
            ExpenseCategoryReportRange.Week => _anchorDate.AddDays(7 * delta),
            ExpenseCategoryReportRange.Month => _anchorDate.AddMonths(delta),
            ExpenseCategoryReportRange.Year => _anchorDate.AddYears(delta),
            _ => _anchorDate
        };

        UpdateRangeState();
        _ = LoadAsync();
    }

    private void UpdateRangeState()
    {
        CanNavigatePeriods = _selectedRange != ExpenseCategoryReportRange.All;
        PeriodLabel = BuildPeriodLabel();
        ApplyFilterButtonState();
    }

    private string BuildPeriodLabel()
    {
        return _localizedFormattingService.FormatCategoryReportPeriod(_selectedRange, _anchorDate);
    }

    private void ApplyFilterButtonState()
    {
        WeekButtonBackgroundColor = GetButtonBackground(ExpenseCategoryReportRange.Week);
        WeekButtonTextColor = GetButtonText(ExpenseCategoryReportRange.Week);
        MonthButtonBackgroundColor = GetButtonBackground(ExpenseCategoryReportRange.Month);
        MonthButtonTextColor = GetButtonText(ExpenseCategoryReportRange.Month);
        YearButtonBackgroundColor = GetButtonBackground(ExpenseCategoryReportRange.Year);
        YearButtonTextColor = GetButtonText(ExpenseCategoryReportRange.Year);
        AllButtonBackgroundColor = GetButtonBackground(ExpenseCategoryReportRange.All);
        AllButtonTextColor = GetButtonText(ExpenseCategoryReportRange.All);
    }

    private Color GetButtonBackground(ExpenseCategoryReportRange range) =>
        _selectedRange == range ? Color.FromArgb("#007AFF") : Color.FromArgb("#F2F4F7");

    private Color GetButtonText(ExpenseCategoryReportRange range) =>
        _selectedRange == range ? Colors.White : Color.FromArgb("#007AFF");

    private async void OnDataChanged()
    {
        await MainThread.InvokeOnMainThreadAsync(LoadAsync);
    }
}
