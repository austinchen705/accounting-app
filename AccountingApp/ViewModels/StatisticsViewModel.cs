using System.Windows.Input;
using Microcharts;
using SkiaSharp;
using AccountingApp.Services;

namespace AccountingApp.ViewModels;

public class StatisticsViewModel : BindableObject
{
    private readonly StatisticsService _statisticsService;
    private DateTime _selectedMonth = new(DateTime.Today.Year, DateTime.Today.Month, 1);
    private Chart? _pieChart;
    private Chart? _barChart;
    private bool _hasPieData;
    private bool _hasBarData;

    public DateTime SelectedMonth
    {
        get => _selectedMonth;
        set { _selectedMonth = value; OnPropertyChanged(); _ = LoadAsync(); }
    }

    public Chart? PieChart
    {
        get => _pieChart;
        set { _pieChart = value; OnPropertyChanged(); }
    }

    public Chart? BarChart
    {
        get => _barChart;
        set { _barChart = value; OnPropertyChanged(); }
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

    public ICommand PreviousMonthCommand { get; }
    public ICommand NextMonthCommand { get; }

    private static readonly SKColor[] ChartColors = [
        SKColor.Parse("#2196F3"), SKColor.Parse("#4CAF50"), SKColor.Parse("#FF5722"),
        SKColor.Parse("#9C27B0"), SKColor.Parse("#FF9800"), SKColor.Parse("#00BCD4")
    ];

    public StatisticsViewModel(StatisticsService statisticsService)
    {
        _statisticsService = statisticsService;
        PreviousMonthCommand = new Command(() => SelectedMonth = SelectedMonth.AddMonths(-1));
        NextMonthCommand = new Command(() => SelectedMonth = SelectedMonth.AddMonths(1));
    }

    public async Task LoadAsync()
    {
        await LoadPieChartAsync();
        await LoadBarChartAsync();
    }

    private async Task LoadPieChartAsync()
    {
        var month = SelectedMonth.ToString("yyyy-MM");
        var stats = await _statisticsService.GetMonthCategoryStatsAsync(month);

        if (stats.Count == 0 || stats.All(s => s.Amount == 0))
        {
            HasPieData = false;
            return;
        }

        HasPieData = true;
        var entries = stats.Select((s, i) => new ChartEntry((float)s.Amount)
        {
            Label = s.CategoryName,
            ValueLabel = s.Amount.ToString("N0"),
            Color = ChartColors[i % ChartColors.Length]
        }).ToList();

        PieChart = new DonutChart { Entries = entries, LabelTextSize = 30 };
    }

    private async Task LoadBarChartAsync()
    {
        var stats = await _statisticsService.GetLast6MonthsStatsAsync();
        HasBarData = stats.Any(s => s.Income > 0 || s.Expense > 0);

        var entries = stats.SelectMany(s => new[]
        {
            new ChartEntry((float)s.Income) { Label = s.Month, Color = SKColor.Parse("#4CAF50") },
            new ChartEntry((float)s.Expense) { Label = "", Color = SKColor.Parse("#F44336") }
        }).ToList();

        BarChart = new BarChart { Entries = entries, LabelTextSize = 28 };
    }
}
