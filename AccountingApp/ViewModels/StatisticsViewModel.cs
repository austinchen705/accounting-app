using System.Collections.ObjectModel;
using System.Threading;
using System.Windows.Input;
using AccountingApp.Core.Services;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using AccountingApp.Services;

namespace AccountingApp.ViewModels;

public class StatisticsViewModel : BindableObject
{
    private readonly ILocalizedFormattingService _localizedFormattingService;
    private readonly ILocalizationService _localizationService;
    public enum CategoryTrendMode
    {
        Top5,
        SingleCategory
    }

    public class ExpenseCategoryOption
    {
        public int Id { get; init; }
        public string Name { get; init; } = string.Empty;

        public override string ToString() => Name;
    }

    public class ChartLegendItem
    {
        public string Name { get; set; } = string.Empty;
        public string PercentageText { get; set; } = string.Empty;
        public Color DotColor { get; set; } = Colors.Gray;
    }

    private readonly StatisticsService _statisticsService;
    private readonly DataRefreshService _refreshService;
    private DateTime _selectedMonth = new(DateTime.Today.Year, DateTime.Today.Month, 1);
    private ISeries[] _pieSeries = Array.Empty<ISeries>();
    private ISeries[] _trendSeries = Array.Empty<ISeries>();
    private Axis[] _trendXAxes = Array.Empty<Axis>();
    private Axis[] _trendYAxes = Array.Empty<Axis>();
    private ISeries[] _categoryTrendSeries = Array.Empty<ISeries>();
    private Axis[] _categoryTrendXAxes = Array.Empty<Axis>();
    private Axis[] _categoryTrendYAxes = Array.Empty<Axis>();
    private bool _hasPieData;
    private bool _hasBarData;
    private bool _hasCategoryTrendData;
    private int _loadVersion;
    private string _barChartUnitText = "單位：TWD";
    private string _incomeMoMText = "--";
    private string _expenseMoMText = "--";
    private string _maxExpenseText = "--";
    private string _minNetText = "--";
    private string _categoryTrendEmptyStateText = string.Empty;
    private CategoryTrendMode _selectedCategoryTrendMode = CategoryTrendMode.Top5;
    private ExpenseCategoryOption? _selectedExpenseCategory;
    public ObservableCollection<ChartLegendItem> PieLegends { get; } = new();
    public ObservableCollection<ExpenseCategoryOption> AvailableExpenseCategories { get; } = new();
    public IReadOnlyList<CategoryTrendMode> CategoryTrendModes { get; } = Enum.GetValues<CategoryTrendMode>();

    public DateTime SelectedMonth
    {
        get => _selectedMonth;
        set { _selectedMonth = value; OnPropertyChanged(); OnPropertyChanged(nameof(SelectedMonthLabel)); _ = LoadAsync(); }
    }

    public ISeries[] PieSeries
    {
        get => _pieSeries;
        set { _pieSeries = value; OnPropertyChanged(); }
    }

    public ISeries[] TrendSeries
    {
        get => _trendSeries;
        set { _trendSeries = value; OnPropertyChanged(); }
    }

    public Axis[] TrendXAxes
    {
        get => _trendXAxes;
        set { _trendXAxes = value; OnPropertyChanged(); }
    }

    public Axis[] TrendYAxes
    {
        get => _trendYAxes;
        set { _trendYAxes = value; OnPropertyChanged(); }
    }

    public ISeries[] CategoryTrendSeries
    {
        get => _categoryTrendSeries;
        set { _categoryTrendSeries = value; OnPropertyChanged(); }
    }

    public Axis[] CategoryTrendXAxes
    {
        get => _categoryTrendXAxes;
        set { _categoryTrendXAxes = value; OnPropertyChanged(); }
    }

    public Axis[] CategoryTrendYAxes
    {
        get => _categoryTrendYAxes;
        set { _categoryTrendYAxes = value; OnPropertyChanged(); }
    }

    public bool HasPieData
    {
        get => _hasPieData;
        set { _hasPieData = value; OnPropertyChanged(); }
    }

    public bool HasBarData
    {
        get => _hasBarData;
        set { _hasBarData = value; OnPropertyChanged(); }
    }

    public bool HasCategoryTrendData
    {
        get => _hasCategoryTrendData;
        set { _hasCategoryTrendData = value; OnPropertyChanged(); }
    }

    public string BarChartUnitText
    {
        get => _barChartUnitText;
        set { _barChartUnitText = value; OnPropertyChanged(); }
    }

    public string IncomeMoMText
    {
        get => _incomeMoMText;
        set { _incomeMoMText = value; OnPropertyChanged(); }
    }

    public string ExpenseMoMText
    {
        get => _expenseMoMText;
        set { _expenseMoMText = value; OnPropertyChanged(); }
    }

    public string MaxExpenseText
    {
        get => _maxExpenseText;
        set { _maxExpenseText = value; OnPropertyChanged(); }
    }

    public string MinNetText
    {
        get => _minNetText;
        set { _minNetText = value; OnPropertyChanged(); }
    }

    public string SelectedMonthLabel => _localizedFormattingService.FormatMonthYear(SelectedMonth);

    public CategoryTrendMode SelectedCategoryTrendMode
    {
        get => _selectedCategoryTrendMode;
        set
        {
            if (_selectedCategoryTrendMode == value) return;
            _selectedCategoryTrendMode = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsTop5Mode));
            OnPropertyChanged(nameof(IsSingleCategoryMode));
            OnPropertyChanged(nameof(ShowCategoryPicker));
            OnPropertyChanged(nameof(CategoryTrendEmptyStateText));
            _ = ReloadCategoryTrendOnlyAsync();
        }
    }

    public ExpenseCategoryOption? SelectedExpenseCategory
    {
        get => _selectedExpenseCategory;
        set
        {
            if (_selectedExpenseCategory == value) return;
            _selectedExpenseCategory = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CategoryTrendEmptyStateText));
            if (IsSingleCategoryMode)
            {
                _ = ReloadCategoryTrendOnlyAsync();
            }
        }
    }

    public bool IsTop5Mode => SelectedCategoryTrendMode == CategoryTrendMode.Top5;
    public bool IsSingleCategoryMode => SelectedCategoryTrendMode == CategoryTrendMode.SingleCategory;
    public bool ShowCategoryPicker => IsSingleCategoryMode;

    public string CategoryTrendEmptyStateText
    {
        get => _categoryTrendEmptyStateText;
        private set { _categoryTrendEmptyStateText = value; OnPropertyChanged(); }
    }

    public ICommand PreviousMonthCommand { get; }
    public ICommand NextMonthCommand { get; }
    public ICommand SelectTop5CategoryTrendModeCommand { get; }
    public ICommand SelectSingleCategoryTrendModeCommand { get; }

    public StatisticsViewModel(
        StatisticsService statisticsService,
        ILocalizedFormattingService localizedFormattingService,
        ILocalizationService localizationService,
        DataRefreshService refreshService)
    {
        _statisticsService = statisticsService;
        _localizedFormattingService = localizedFormattingService;
        _localizationService = localizationService;
        _refreshService = refreshService;
        PreviousMonthCommand = new Command(() => SelectedMonth = SelectedMonth.AddMonths(-1));
        NextMonthCommand = new Command(() => SelectedMonth = SelectedMonth.AddMonths(1));
        SelectTop5CategoryTrendModeCommand = new Command(() => SelectedCategoryTrendMode = CategoryTrendMode.Top5);
        SelectSingleCategoryTrendModeCommand = new Command(() => SelectedCategoryTrendMode = CategoryTrendMode.SingleCategory);
        _refreshService.DataChanged += OnDataChanged;
        CategoryTrendEmptyStateText = _localizationService.GetString("StatisticsCategoryTrendEmptyStateText");
    }

    public async Task LoadAsync()
    {
        var version = Interlocked.Increment(ref _loadVersion);
        var selectedMonth = SelectedMonth;
        await LoadPieChartAsync(selectedMonth, version);
        await LoadTrendChartAsync(selectedMonth, version);
        await LoadCategoryTrendChartAsync(selectedMonth, version);
    }

    private async Task LoadPieChartAsync(DateTime monthDate, int loadVersion)
    {
        var month = monthDate.ToString("yyyy-MM");
        var stats = await _statisticsService.GetMonthCategoryStatsAsync(month);
        if (loadVersion != _loadVersion) return;

        if (stats.Count == 0 || stats.All(s => s.Amount == 0))
        {
            HasPieData = false;
            PieSeries = Array.Empty<ISeries>();
            PieLegends.Clear();
            return;
        }

        HasPieData = true;
        var series = new List<ISeries>(stats.Count);
        var colorByCategory = CategoryColorPalette.BuildDistinctHexColors(
            stats.Select(stat => stat.CategoryName));

        var total = stats.Sum(s => s.Amount);
        PieLegends.Clear();
        foreach (var stat in stats)
        {
            var c = SKColor.Parse(colorByCategory[stat.CategoryName]);
            var ratio = total <= 0 ? 0 : stat.Amount / total;
            series.Add(new PieSeries<double>
            {
                Values = [Convert.ToDouble(stat.Amount)],
                Name = stat.CategoryName,
                Fill = new SolidColorPaint(c),
                Stroke = new SolidColorPaint(SKColors.White) { StrokeThickness = 1 },
                DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
                DataLabelsPaint = new SolidColorPaint(SKColors.Transparent)
            });
            PieLegends.Add(new ChartLegendItem
            {
                Name = stat.CategoryName,
                PercentageText = ratio.ToString("P0"),
                DotColor = Color.FromRgb(c.Red, c.Green, c.Blue)
            });
        }

        PieSeries = series.ToArray();
    }

    private async Task LoadTrendChartAsync(DateTime monthDate, int loadVersion)
    {
        var stats = await _statisticsService.GetLast12MonthsStatsAsync(monthDate);
        if (loadVersion != _loadVersion) return;
        var baseCurrency = Preferences.Get("base_currency", "TWD");
        BarChartUnitText = string.Format(_localizationService.GetString("UnitPrefix"), baseCurrency);
        HasBarData = stats.Any(s => s.Income > 0 || s.Expense > 0);

        var months = stats.Select(s => s.Month.Replace("月", "")).ToArray();
        var incomeValues = stats.Select(s => (double)s.Income).ToArray();
        var expenseValues = stats.Select(s => (double)s.Expense).ToArray();

        TrendSeries =
        [
            new LineSeries<double>
            {
                Name = _localizationService.GetString("StatisticsIncomeSeriesName"),
                Values = incomeValues,
                Fill = null,
                GeometrySize = 8,
                LineSmoothness = 0,
                Stroke = new SolidColorPaint(SKColor.Parse("#4CAF50")) { StrokeThickness = 3 },
                GeometryFill = new SolidColorPaint(SKColor.Parse("#4CAF50")),
                GeometryStroke = new SolidColorPaint(SKColor.Parse("#4CAF50")) { StrokeThickness = 3 }
            },
            new LineSeries<double>
            {
                Name = _localizationService.GetString("StatisticsExpenseSeriesName"),
                Values = expenseValues,
                Fill = null,
                GeometrySize = 8,
                LineSmoothness = 0,
                Stroke = new SolidColorPaint(SKColor.Parse("#F44336")) { StrokeThickness = 3 },
                GeometryFill = new SolidColorPaint(SKColor.Parse("#F44336"))
            }
        ];

        TrendXAxes =
        [
            new Axis
            {
                Labels = months,
                TextSize = 12
            }
        ];

        TrendYAxes =
        [
            new Axis
            {
                MinLimit = 0,
                MinStep = 50_000,
                ForceStepToMin = true,
                TextSize = 12,
                Name = baseCurrency,
                Labeler = value => FormatAxisValue(value),
                SeparatorsPaint = new SolidColorPaint(SKColor.Parse("#E5E7EB")) { StrokeThickness = 1 }
            }
        ];

        ApplyTrendInsights(stats);
    }

    private async Task LoadCategoryTrendChartAsync(DateTime monthDate, int loadVersion)
    {
        await EnsureAvailableExpenseCategoriesAsync(loadVersion);
        if (loadVersion != _loadVersion) return;

        if (!IsSingleCategoryMode)
        {
            await LoadTopCategoryTrendChartAsync(monthDate, loadVersion);
            return;
        }

        await LoadSingleCategoryTrendChartAsync(monthDate, loadVersion);
    }

    private async Task EnsureAvailableExpenseCategoriesAsync(int loadVersion)
    {
        var categories = await _statisticsService.GetExpenseCategoriesAsync();
        if (loadVersion != _loadVersion) return;

        var selectedCategoryId = SelectedExpenseCategory?.Id;
        AvailableExpenseCategories.Clear();
        foreach (var category in categories)
        {
            AvailableExpenseCategories.Add(new ExpenseCategoryOption
            {
                Id = category.CategoryId,
                Name = category.CategoryName
            });
        }

        if (selectedCategoryId is null) return;

        var existing = AvailableExpenseCategories.FirstOrDefault(category => category.Id == selectedCategoryId.Value);
        if (existing is null)
        {
            SelectedExpenseCategory = null;
            return;
        }

        if (SelectedExpenseCategory?.Id != existing.Id)
        {
            SelectedExpenseCategory = existing;
        }
    }

    private async Task LoadTopCategoryTrendChartAsync(DateTime monthDate, int loadVersion)
    {
        var stats = await _statisticsService.GetTopExpenseCategoryTrendAsync(monthDate);
        if (loadVersion != _loadVersion) return;

        HasCategoryTrendData = stats.Count > 0;
        CategoryTrendEmptyStateText = _localizationService.GetString("StatisticsCategoryTrendTop5EmptyStateText");
        if (!HasCategoryTrendData)
        {
            ClearCategoryTrendChart();
            return;
        }

        ApplyCategoryTrendChart(stats, monthDate);
    }

    private async Task LoadSingleCategoryTrendChartAsync(DateTime monthDate, int loadVersion)
    {
        if (SelectedExpenseCategory is null)
        {
            HasCategoryTrendData = false;
            CategoryTrendEmptyStateText = _localizationService.GetString("StatisticsCategoryTrendSelectCategoryPrompt");
            ClearCategoryTrendChart();
            return;
        }

        var stat = await _statisticsService.GetExpenseCategoryTrendAsync(monthDate, SelectedExpenseCategory.Id);
        if (loadVersion != _loadVersion) return;

        if (stat is null || stat.Values.All(value => value == 0))
        {
            HasCategoryTrendData = false;
            CategoryTrendEmptyStateText = _localizationService.GetString("StatisticsCategoryTrendSingleCategoryEmptyStateText");
            ClearCategoryTrendChart();
            return;
        }

        HasCategoryTrendData = true;
        CategoryTrendEmptyStateText = _localizationService.GetString("StatisticsCategoryTrendSingleCategoryEmptyStateText");
        ApplyCategoryTrendChart([stat], monthDate);
    }

    private void ApplyCategoryTrendChart(IReadOnlyList<ExpenseCategoryTrendStat> stats, DateTime monthDate)
    {
        var months = StatisticsTrendWindow.GetTwelveMonthWindow(monthDate)
            .Select(date => date.ToString("MM"))
            .ToArray();
        var colorByCategory = CategoryColorPalette.BuildDistinctHexColors(
            stats.Select(stat => stat.CategoryName));

        CategoryTrendSeries = stats
            .Select(stat =>
            {
                var color = SKColor.Parse(colorByCategory[stat.CategoryName]);
                return (ISeries)new LineSeries<double>
                {
                    Name = stat.CategoryName,
                    Values = stat.Values.Select(value => (double)value).ToArray(),
                    Fill = null,
                    GeometrySize = 7,
                    LineSmoothness = 0,
                    Stroke = new SolidColorPaint(color) { StrokeThickness = 3 },
                    GeometryFill = new SolidColorPaint(color),
                    GeometryStroke = new SolidColorPaint(color) { StrokeThickness = 3 }
                };
            })
            .ToArray();

        CategoryTrendXAxes =
        [
            new Axis
            {
                Labels = months,
                TextSize = 12
            }
        ];

        CategoryTrendYAxes =
        [
            new Axis
            {
                MinLimit = 0,
                MinStep = 50_000,
                ForceStepToMin = true,
                TextSize = 12,
                Name = Preferences.Get("base_currency", "TWD"),
                Labeler = value => FormatAxisValue(value),
                SeparatorsPaint = new SolidColorPaint(SKColor.Parse("#E5E7EB")) { StrokeThickness = 1 }
            }
        ];
    }

    private void ClearCategoryTrendChart()
    {
        CategoryTrendSeries = Array.Empty<ISeries>();
        CategoryTrendXAxes = Array.Empty<Axis>();
        CategoryTrendYAxes = Array.Empty<Axis>();
    }

    private async Task ReloadCategoryTrendOnlyAsync()
    {
        var version = Interlocked.Increment(ref _loadVersion);
        await LoadCategoryTrendChartAsync(SelectedMonth, version);
    }

    private void ApplyTrendInsights(IReadOnlyList<MonthStat> stats)
    {
        var summary = StatisticsTrendInsights.Build(
            stats.Select(s => (s.Month, s.Income, s.Expense)).ToArray());
        IncomeMoMText = summary.IncomeMoMText;
        ExpenseMoMText = summary.ExpenseMoMText;
        MaxExpenseText = summary.MaxExpenseText;
        MinNetText = summary.MinNetText;
    }

    private static string FormatAxisValue(double value)
    {
        var abs = Math.Abs(value);
        if (abs >= 1_000_000) return $"{value / 1_000_000:0.#}M";
        if (abs >= 1_000) return $"{value / 1_000:0.#}K";
        return $"{value:0}";
    }

    private async void OnDataChanged()
    {
        await MainThread.InvokeOnMainThreadAsync(LoadAsync);
    }
}
