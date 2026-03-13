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
    private DateTime _snapshotDate = DateTime.Today;
    private decimal _stock;
    private decimal _cash;
    private decimal _firstTrade;
    private decimal _property;
    private ISeries[] _trendSeries = Array.Empty<ISeries>();
    private Axis[] _trendXAxes = Array.Empty<Axis>();
    private Axis[] _trendYAxes = Array.Empty<Axis>();
    private string _errorMessage = string.Empty;
    private bool _hasError;
    private bool _hasSnapshots;
    private int _importedCount;
    private int _skippedCount;
    private int? _editingSnapshotId;

    public event EventHandler? EditRequested;

    public AssetTrendViewModel(AssetSnapshotService assetSnapshotService)
    {
        _assetSnapshotService = assetSnapshotService;
        AddSnapshotCommand = new Command(async () => await AddSnapshotAsync());
        EditSnapshotCommand = new Command<AssetSnapshot>(BeginEditSnapshot);
        CancelEditCommand = new Command(CancelEdit);
        UpdateSnapshotCommand = new Command<AssetSnapshot>(async snapshot => await UpdateSnapshotAsync(snapshot));
        DeleteSnapshotCommand = new Command<AssetSnapshot>(async snapshot => await DeleteSnapshotAsync(snapshot));
        ImportCsvCommand = new Command(async () => await ImportCsvAsync());
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

    public string ImportSummaryText => $"Imported {ImportedCount}, Skipped {SkippedCount}";

    public bool HasImportSummary => ImportedCount > 0 || SkippedCount > 0;

    public string ImportErrorDetailsText => string.Join(Environment.NewLine, ImportErrorDetails);

    public bool HasImportErrorDetails => ImportErrorDetails.Count > 0;

    public bool IsEditing
    {
        get => _editingSnapshotId.HasValue;
    }

    public string FormTitleText => IsEditing ? "編輯資產快照" : "新增資產快照";

    public string EditingSnapshotDisplayText => IsEditing ? $"正在編輯 {SnapshotDate:yyyy/MM/dd} 的資產快照" : string.Empty;

    public string SubmitButtonText => IsEditing ? "更新快照" : "新增快照";

    public ObservableCollection<AssetSnapshot> Snapshots { get; } = new();
    public ObservableCollection<string> ImportErrorDetails { get; } = new();

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

    public ICommand AddSnapshotCommand { get; }
    public ICommand EditSnapshotCommand { get; }
    public ICommand CancelEditCommand { get; }
    public ICommand UpdateSnapshotCommand { get; }
    public ICommand DeleteSnapshotCommand { get; }
    public ICommand ImportCsvCommand { get; }

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
                PickerTitle = "選擇資產快照 CSV",
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
                .DisplayAlert("確認匯入", "這會清空目前所有資產快照並改用所選 CSV 重新建立。", "匯入", "取消");
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
        var condensedLabels = BuildCondensedDateLabels(trend.Labels);

        TrendSeries =
        [
            BuildStackedColumnSeries("Stock", trend.StockValues, "#2563EB"),
            BuildStackedColumnSeries("Cash", trend.CashValues, "#16A34A"),
            BuildStackedColumnSeries("FirstTrade", trend.FirstTradeValues, "#EA580C"),
            BuildStackedColumnSeries("Property(房產)", trend.PropertyValues, "#7C3AED"),
            new LineSeries<double>
            {
                Name = "Total",
                Values = trend.Totals.Select(value => (double)value).ToArray(),
                Fill = null,
                GeometrySize = 8,
                LineSmoothness = 0,
                Stroke = new SolidColorPaint(SKColors.Black) { StrokeThickness = 3 },
                GeometryFill = new SolidColorPaint(SKColors.Black),
                GeometryStroke = new SolidColorPaint(SKColors.Black) { StrokeThickness = 3 }
            }
        ];

        TrendXAxes =
        [
            new Axis
            {
                Labels = condensedLabels,
                TextSize = 12
            }
        ];

        TrendYAxes =
        [
            new Axis
            {
                MinLimit = 0,
                MinStep = 2_500_000,
                ForceStepToMin = true,
                TextSize = 12,
                Labeler = FormatYAxisValue,
                SeparatorsPaint = new SolidColorPaint(SKColor.Parse("#E5E7EB")) { StrokeThickness = 1 }
            }
        ];
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

    private static string FormatYAxisValue(double value)
    {
        if (value >= 1_000_000d)
        {
            return $"{value / 1_000_000d:0.#}M";
        }

        if (value >= 1_000d)
        {
            return $"{value / 1_000d:0.#}K";
        }

        return value.ToString("0");
    }

    private static StackedColumnSeries<double> BuildStackedColumnSeries(string name, decimal[] values, string color) =>
        new()
        {
            Name = name,
            Values = values.Select(value => (double)value).ToArray(),
            Fill = new SolidColorPaint(SKColor.Parse(color)),
            Stroke = null
        };
}
