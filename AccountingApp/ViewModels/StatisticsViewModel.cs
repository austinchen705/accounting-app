using System.Collections.ObjectModel;
using System.Threading;
using System.Windows.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using AccountingApp.Services;

namespace AccountingApp.ViewModels;

public class StatisticsViewModel : BindableObject
{
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
    private bool _hasPieData;
    private bool _hasBarData;
    private int _loadVersion;
    private string _barChartUnitText = "單位：TWD";
    private string _incomeMoMText = "--";
    private string _expenseMoMText = "--";
    private string _maxExpenseText = "--";
    private string _minNetText = "--";
    public ObservableCollection<ChartLegendItem> PieLegends { get; } = new();

    public DateTime SelectedMonth
    {
        get => _selectedMonth;
        set { _selectedMonth = value; OnPropertyChanged(); _ = LoadAsync(); }
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

    public ICommand PreviousMonthCommand { get; }
    public ICommand NextMonthCommand { get; }

    private static readonly SKColor[] ChartColors = [
        SKColor.Parse("#2196F3"), SKColor.Parse("#4CAF50"), SKColor.Parse("#FF5722"),
        SKColor.Parse("#9C27B0"), SKColor.Parse("#FF9800"), SKColor.Parse("#00BCD4")
    ];

    public StatisticsViewModel(StatisticsService statisticsService, DataRefreshService refreshService)
    {
        _statisticsService = statisticsService;
        _refreshService = refreshService;
        PreviousMonthCommand = new Command(() => SelectedMonth = SelectedMonth.AddMonths(-1));
        NextMonthCommand = new Command(() => SelectedMonth = SelectedMonth.AddMonths(1));
        _refreshService.DataChanged += OnDataChanged;
    }

    public async Task LoadAsync()
    {
        var version = Interlocked.Increment(ref _loadVersion);
        var selectedMonth = SelectedMonth;
        await LoadPieChartAsync(selectedMonth, version);
        await LoadTrendChartAsync(version);
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

        var total = stats.Sum(s => s.Amount);
        PieLegends.Clear();
        foreach (var (stat, i) in stats.Select((s, i) => (s, i)))
        {
            var c = ChartColors[i % ChartColors.Length];
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

    private async Task LoadTrendChartAsync(int loadVersion)
    {
        var stats = await _statisticsService.GetLast6MonthsStatsAsync();
        if (loadVersion != _loadVersion) return;
        var baseCurrency = Preferences.Get("base_currency", "TWD");
        BarChartUnitText = $"單位：{baseCurrency}";
        HasBarData = stats.Any(s => s.Income > 0 || s.Expense > 0);

        var months = stats.Select(s => s.Month.Replace("月", "")).ToArray();
        var incomeValues = stats.Select(s => (double)s.Income).ToArray();
        var expenseValues = stats.Select(s => (double)s.Expense).ToArray();

        TrendSeries =
        [
            new LineSeries<double>
            {
                Name = "收入",
                Values = incomeValues,
                Fill = null,
                GeometrySize = 8,
                LineSmoothness = 0,
                Stroke = new SolidColorPaint(SKColor.Parse("#4CAF50")) { StrokeThickness = 3 },
                GeometryFill = new SolidColorPaint(SKColor.Parse("#4CAF50"))
            },
            new LineSeries<double>
            {
                Name = "支出",
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
                TextSize = 12,
                Name = baseCurrency,
                Labeler = value => FormatAxisValue(value),
                SeparatorsPaint = new SolidColorPaint(SKColor.Parse("#E5E7EB")) { StrokeThickness = 1 }
            }
        ];

        ApplyTrendInsights(stats);
    }

    private void ApplyTrendInsights(IReadOnlyList<MonthStat> stats)
    {
        if (stats.Count == 0)
        {
            IncomeMoMText = "--";
            ExpenseMoMText = "--";
            MaxExpenseText = "--";
            MinNetText = "--";
            return;
        }

        var latest = stats[^1];
        if (stats.Count >= 2)
        {
            var prev = stats[^2];
            IncomeMoMText = FormatMoM(prev.Income, latest.Income);
            ExpenseMoMText = FormatMoM(prev.Expense, latest.Expense);
        }
        else
        {
            IncomeMoMText = "--";
            ExpenseMoMText = "--";
        }

        var maxExpense = stats.MaxBy(s => s.Expense)!;
        MaxExpenseText = $"最高支出月：{maxExpense.Month} ({maxExpense.Expense:N0})";

        var minNet = stats.MinBy(s => s.Income - s.Expense)!;
        var net = minNet.Income - minNet.Expense;
        MinNetText = $"最低淨額月：{minNet.Month} ({net:N0})";
    }

    private static string FormatMoM(decimal previous, decimal current)
    {
        if (previous == 0) return "--";
        var ratio = (current - previous) / previous;
        return ratio >= 0 ? $"+{ratio:P1}" : $"{ratio:P1}";
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
