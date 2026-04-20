using System.Collections.ObjectModel;
using System.Windows.Input;
using AccountingApp.Core.Models;
using AccountingApp.Services;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace AccountingApp.ViewModels;

public class AssetTrendViewModel : BindableObject
{
    private readonly AssetSnapshotService _assetSnapshotService;
    private readonly ILocalizationService _localizationService;
    private DateTime _snapshotDate = DateTime.Today;
    private decimal _stock;
    private decimal _cash;
    private decimal _firstTrade;
    private decimal _property;
    private ISeries[] _summaryTrendSeries = Array.Empty<ISeries>();
    private Axis[] _summaryTrendXAxes = Array.Empty<Axis>();
    private Axis[] _summaryTrendYAxes = Array.Empty<Axis>();
    private ISeries[] _detailTrendSeries = Array.Empty<ISeries>();
    private Axis[] _detailTrendXAxes = Array.Empty<Axis>();
    private Axis[] _detailTrendYAxes = Array.Empty<Axis>();
    private string _errorMessage = string.Empty;
    private bool _hasError;
    private bool _hasSnapshots;
    private string _latestTotalCaptionText = string.Empty;
    private string _latestTotalAmountText = string.Empty;
    private int _importedCount;
    private int _skippedCount;
    private int? _editingSnapshotId;

    public event EventHandler? EditRequested;
    public event EventHandler? FullscreenChartRequested;

    public AssetTrendViewModel(AssetSnapshotService assetSnapshotService, ILocalizationService localizationService)
    {
        _assetSnapshotService = assetSnapshotService;
        _localizationService = localizationService;
        AddSnapshotCommand = new Command(async () => await AddSnapshotAsync());
        EditSnapshotCommand = new Command<AssetSnapshot>(BeginEditSnapshot);
        CancelEditCommand = new Command(CancelEdit);
        UpdateSnapshotCommand = new Command<AssetSnapshot>(async snapshot => await UpdateSnapshotAsync(snapshot));
        DeleteSnapshotCommand = new Command<AssetSnapshot>(async snapshot => await DeleteSnapshotAsync(snapshot));
        ImportCsvCommand = new Command(async () => await ImportCsvAsync());
        OpenFullscreenChartCommand = new Command(() => FullscreenChartRequested?.Invoke(this, EventArgs.Empty));
    }

    public DateTime SnapshotDate
    {
        get => _snapshotDate;
        set
        {
            _snapshotDate = value.Date;
            OnPropertyChanged();
            OnPropertyChanged(nameof(EditingSnapshotDisplayText));
        }
    }

    public decimal Stock
    {
        get => _stock;
        set { _stock = value; OnPropertyChanged(); }
    }

    public decimal Cash
    {
        get => _cash;
        set { _cash = value; OnPropertyChanged(); }
    }

    public decimal FirstTrade
    {
        get => _firstTrade;
        set { _firstTrade = value; OnPropertyChanged(); }
    }

    public decimal Property
    {
        get => _property;
        set { _property = value; OnPropertyChanged(); }
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set { _errorMessage = value; OnPropertyChanged(); }
    }

    public bool HasError
    {
        get => _hasError;
        set { _hasError = value; OnPropertyChanged(); }
    }

    public bool HasSnapshots
    {
        get => _hasSnapshots;
        set { _hasSnapshots = value; OnPropertyChanged(); }
    }

    public string LatestTotalCaptionText
    {
        get => _latestTotalCaptionText;
        set { _latestTotalCaptionText = value; OnPropertyChanged(); }
    }

    public string LatestTotalAmountText
    {
        get => _latestTotalAmountText;
        set { _latestTotalAmountText = value; OnPropertyChanged(); }
    }

    public int ImportedCount
    {
        get => _importedCount;
        set { _importedCount = value; OnPropertyChanged(); OnPropertyChanged(nameof(ImportSummaryText)); OnPropertyChanged(nameof(HasImportSummary)); }
    }

    public int SkippedCount
    {
        get => _skippedCount;
        set { _skippedCount = value; OnPropertyChanged(); OnPropertyChanged(nameof(ImportSummaryText)); OnPropertyChanged(nameof(HasImportSummary)); }
    }

    public string ImportSummaryText => string.Format(
        _localizationService.GetString("AssetTrendImportSummaryFormat"),
        ImportedCount,
        SkippedCount);

    public bool HasImportSummary => ImportedCount > 0 || SkippedCount > 0;

    public string ImportErrorDetailsText => string.Join(Environment.NewLine, ImportErrorDetails);

    public bool HasImportErrorDetails => ImportErrorDetails.Count > 0;

    public bool IsEditing
    {
        get => _editingSnapshotId.HasValue;
    }

    public string FormTitleText => IsEditing
        ? _localizationService.GetString("AssetTrendEditTitle")
        : _localizationService.GetString("AssetTrendCreateTitle");

    public string EditingSnapshotDisplayText => IsEditing
        ? string.Format(_localizationService.GetString("AssetTrendEditingSnapshotFormat"), SnapshotDate)
        : string.Empty;

    public string SubmitButtonText => IsEditing
        ? _localizationService.GetString("AssetTrendUpdateButton")
        : _localizationService.GetString("AssetTrendCreateButton");

    public ObservableCollection<AssetSnapshot> Snapshots { get; } = new();
    public ObservableCollection<string> ImportErrorDetails { get; } = new();

    public ISeries[] SummaryTrendSeries
    {
        get => _summaryTrendSeries;
        set
        {
            _summaryTrendSeries = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(TrendSeries));
        }
    }

    public Axis[] SummaryTrendXAxes
    {
        get => _summaryTrendXAxes;
        set
        {
            _summaryTrendXAxes = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(TrendXAxes));
        }
    }

    public Axis[] SummaryTrendYAxes
    {
        get => _summaryTrendYAxes;
        set
        {
            _summaryTrendYAxes = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(TrendYAxes));
        }
    }

    public ISeries[] DetailTrendSeries
    {
        get => _detailTrendSeries;
        set { _detailTrendSeries = value; OnPropertyChanged(); }
    }

    public Axis[] DetailTrendXAxes
    {
        get => _detailTrendXAxes;
        set { _detailTrendXAxes = value; OnPropertyChanged(); }
    }

    public Axis[] DetailTrendYAxes
    {
        get => _detailTrendYAxes;
        set { _detailTrendYAxes = value; OnPropertyChanged(); }
    }

    public ISeries[] TrendSeries => SummaryTrendSeries;
    public Axis[] TrendXAxes => SummaryTrendXAxes;
    public Axis[] TrendYAxes => SummaryTrendYAxes;

    public ICommand AddSnapshotCommand { get; }
    public ICommand EditSnapshotCommand { get; }
    public ICommand CancelEditCommand { get; }
    public ICommand UpdateSnapshotCommand { get; }
    public ICommand DeleteSnapshotCommand { get; }
    public ICommand ImportCsvCommand { get; }
    public ICommand OpenFullscreenChartCommand { get; }

    public async Task LoadAsync()
    {
        var snapshots = await _assetSnapshotService.GetAllAsync();
        Snapshots.Clear();
        foreach (var snapshot in snapshots)
        {
            Snapshots.Add(snapshot);
        }

        HasSnapshots = snapshots.Count > 0;
        RefreshChart(snapshots);
    }

    private async Task AddSnapshotAsync()
    {
        try
        {
            var snapshot = new AssetSnapshot
            {
                Id = _editingSnapshotId ?? 0,
                Date = SnapshotDate,
                Stock = Stock,
                Cash = Cash,
                FirstTrade = FirstTrade,
                Property = Property
            };

            if (IsEditing)
            {
                await _assetSnapshotService.UpdateAsync(snapshot);
            }
            else
            {
                await _assetSnapshotService.AddAsync(snapshot);
            }

            HasError = false;
            ErrorMessage = string.Empty;
            CancelEdit();
            await LoadAsync();
        }
        catch (ArgumentException ex)
        {
            ErrorMessage = ex.Message;
            HasError = true;
        }
    }

    private async Task UpdateSnapshotAsync(AssetSnapshot? snapshot)
    {
        if (snapshot is null) return;
        await _assetSnapshotService.UpdateAsync(snapshot);
        await LoadAsync();
    }

    private async Task DeleteSnapshotAsync(AssetSnapshot? snapshot)
    {
        if (snapshot is null) return;
        if (_editingSnapshotId == snapshot.Id)
        {
            CancelEdit();
        }
        await _assetSnapshotService.DeleteAsync(snapshot.Id);
        await LoadAsync();
    }

    private async Task ImportCsvAsync()
    {
        try
        {
            var result = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = _localizationService.GetString("AssetTrendImportPickerTitle"),
                FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.iOS, ["public.comma-separated-values-text"] },
                    { DevicePlatform.Android, ["text/csv", "text/comma-separated-values"] },
                    { DevicePlatform.WinUI, [".csv"] },
                    { DevicePlatform.macOS, ["csv"] }
                })
            });

            if (result is null)
            {
                return;
            }

            var confirm = await Application.Current!.Windows[0].Page!
                .DisplayAlert(
                    _localizationService.GetString("AssetTrendImportConfirmTitle"),
                    _localizationService.GetString("AssetTrendImportConfirmMessage"),
                    _localizationService.GetString("ImportButtonText"),
                    _localizationService.GetString("CancelButtonText"));
            if (!confirm)
            {
                return;
            }

            var csvContent = await File.ReadAllTextAsync(result.FullPath);
            var importResult = await _assetSnapshotService.ReplaceImportCsvAsync(csvContent);
            ImportedCount = importResult.ImportedCount;
            SkippedCount = importResult.SkippedCount;
            ImportErrorDetails.Clear();
            foreach (var error in importResult.ErrorDetails)
            {
                ImportErrorDetails.Add(error);
            }
            OnPropertyChanged(nameof(ImportErrorDetailsText));
            OnPropertyChanged(nameof(HasImportErrorDetails));
            HasError = false;
            ErrorMessage = string.Empty;
            await LoadAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            HasError = true;
        }
    }

    private void ApplyImportResult(AccountingApp.Core.Services.AssetSnapshotImportResult importResult)
    {
        ImportedCount = importResult.ImportedCount;
        SkippedCount = importResult.SkippedCount;
        ImportErrorDetails.Clear();
        foreach (var error in importResult.ErrorDetails)
        {
            ImportErrorDetails.Add(error);
        }

        OnPropertyChanged(nameof(ImportErrorDetailsText));
        OnPropertyChanged(nameof(HasImportErrorDetails));
        HasError = false;
        ErrorMessage = string.Empty;
    }

    private void BeginEditSnapshot(AssetSnapshot? snapshot)
    {
        if (snapshot is null) return;

        _editingSnapshotId = snapshot.Id;
        SnapshotDate = snapshot.Date;
        Stock = snapshot.Stock;
        Cash = snapshot.Cash;
        FirstTrade = snapshot.FirstTrade;
        Property = snapshot.Property;
        HasError = false;
        ErrorMessage = string.Empty;
        OnPropertyChanged(nameof(IsEditing));
        OnPropertyChanged(nameof(FormTitleText));
        OnPropertyChanged(nameof(EditingSnapshotDisplayText));
        OnPropertyChanged(nameof(SubmitButtonText));
        EditRequested?.Invoke(this, EventArgs.Empty);
    }

    private void CancelEdit()
    {
        _editingSnapshotId = null;
        SnapshotDate = DateTime.Today;
        Stock = 0;
        Cash = 0;
        FirstTrade = 0;
        Property = 0;
        OnPropertyChanged(nameof(IsEditing));
        OnPropertyChanged(nameof(FormTitleText));
        OnPropertyChanged(nameof(EditingSnapshotDisplayText));
        OnPropertyChanged(nameof(SubmitButtonText));
    }

    private void RefreshChart(IReadOnlyList<AssetSnapshot> snapshots)
    {
        var trend = _assetSnapshotService.BuildTrendSeries(snapshots);
        var series = BuildTrendSeries(trend);
        ApplyLatestTotalSummary(snapshots, trend.Totals);

        SummaryTrendSeries = series;
        DetailTrendSeries = series;
        SummaryTrendXAxes = CreateXAxis(BuildCondensedDateLabels(trend.Labels));
        DetailTrendXAxes = CreateXAxis(trend.Labels.Select(ShortenDateLabel).ToArray());

        var numericTotals = trend.Totals.Select(value => (double)value).ToArray();
        SummaryTrendYAxes = CreateYAxis(
            AccountingApp.Core.Services.AssetTrendChartAxisHelper.CalculateSummaryStep(numericTotals),
            AccountingApp.Core.Services.AssetTrendChartAxisHelper.FormatSummaryValue);
        DetailTrendYAxes = CreateYAxis(
            AccountingApp.Core.Services.AssetTrendChartAxisHelper.CalculateDetailStep(numericTotals),
            AccountingApp.Core.Services.AssetTrendChartAxisHelper.FormatDetailValue);
    }

    private void ApplyLatestTotalSummary(IReadOnlyList<AssetSnapshot> snapshots, IReadOnlyList<decimal> totals)
    {
        if (snapshots.Count == 0 || totals.Count == 0)
        {
            LatestTotalCaptionText = string.Empty;
            LatestTotalAmountText = string.Empty;
            return;
        }

        var latestSnapshot = snapshots
            .OrderBy(snapshot => snapshot.Date)
            .Last();
        var latestTotal = totals[^1];
        LatestTotalCaptionText = string.Format(
            _localizationService.GetString("AssetTrendLatestTotalCaptionFormat"),
            latestSnapshot.Date);
        LatestTotalAmountText = latestTotal.ToString("#,0.##");
    }

    private static string[] BuildCondensedDateLabels(string[] labels)
    {
        if (labels.Length <= 6)
        {
            return labels.Select(ShortenDateLabel).ToArray();
        }

        var step = labels.Length switch
        {
            <= 12 => 2,
            <= 24 => 3,
            _ => 5
        };

        return labels
            .Select((label, index) => index == labels.Length - 1 || index % step == 0
                ? ShortenDateLabel(label)
                : string.Empty)
            .ToArray();
    }

    private static string ShortenDateLabel(string label)
    {
        return DateTime.TryParse(label, out var date)
            ? date.ToString("MM/dd")
            : label;
    }

    private ISeries[] BuildTrendSeries(AccountingApp.Core.Services.AssetTrendSeriesResult trend) =>
    [
        BuildStackedColumnSeries("Stock", trend.StockValues, "#2563EB"),
        BuildStackedColumnSeries("Cash", trend.CashValues, "#16A34A"),
        BuildStackedColumnSeries("FirstTrade", trend.FirstTradeValues, "#EA580C"),
        BuildStackedColumnSeries(_localizationService.GetString("AssetTrendPropertySeriesName"), trend.PropertyValues, "#7C3AED"),
        new LineSeries<double>
        {
            Name = _localizationService.GetString("AssetTrendTotalSeriesName"),
            Values = trend.Totals.Select(value => (double)value).ToArray(),
            Fill = null,
            GeometrySize = 8,
            LineSmoothness = 0,
            Stroke = new SolidColorPaint(SKColors.Black) { StrokeThickness = 3 },
            GeometryFill = new SolidColorPaint(SKColors.Black),
            GeometryStroke = new SolidColorPaint(SKColors.Black) { StrokeThickness = 3 }
        }
    ];

    private static Axis[] CreateXAxis(string[] labels) =>
    [
        new Axis
        {
            Labels = labels,
            TextSize = 12
        }
    ];

    private static Axis[] CreateYAxis(double minStep, Func<double, string> labeler) =>
    [
        new Axis
        {
            MinLimit = 0,
            MinStep = minStep,
            ForceStepToMin = true,
            TextSize = 12,
            Labeler = labeler,
            SeparatorsPaint = new SolidColorPaint(SKColor.Parse("#E5E7EB")) { StrokeThickness = 1 }
        }
    ];

    private static StackedColumnSeries<double> BuildStackedColumnSeries(string name, decimal[] values, string color) =>
        new()
        {
            Name = name,
            Values = values.Select(value => (double)value).ToArray(),
            Fill = new SolidColorPaint(SKColor.Parse(color)),
            Stroke = null
        };
}
