using System.Collections.ObjectModel;
using AccountingApp.Core.Services;
using AccountingApp.Services;

namespace AccountingApp.ViewModels;

[QueryProperty(nameof(CategoryId), "categoryId")]
[QueryProperty(nameof(CategoryName), "categoryName")]
[QueryProperty(nameof(Range), "range")]
[QueryProperty(nameof(AnchorDate), "anchorDate")]
public class CategoryReportTransactionDetailViewModel : BindableObject
{
    public class TransactionItem
    {
        public string Note { get; init; } = string.Empty;
        public string AmountText { get; init; } = "0";
        public string CurrencyText { get; init; } = string.Empty;
    }

    public class TransactionDateGroup : ObservableCollection<TransactionItem>
    {
        public TransactionDateGroup(string dateLabel, IEnumerable<TransactionItem> items)
            : base(items)
        {
            DateLabel = dateLabel;
        }

        public string DateLabel { get; }
    }

    private readonly StatisticsService _statisticsService;
    private readonly ILocalizedFormattingService _localizedFormattingService;
    private readonly ILocalizationService _localizationService;
    private int _categoryId;
    private string _categoryName = string.Empty;
    private ExpenseCategoryReportRange _range = ExpenseCategoryReportRange.Month;
    private DateTime _anchorDate = DateTime.Today;
    private string _periodLabel = string.Empty;
    private string _totalAmountText = "0";
    private bool _hasTransactions;

    public CategoryReportTransactionDetailViewModel(
        StatisticsService statisticsService,
        ILocalizedFormattingService localizedFormattingService,
        ILocalizationService localizationService)
    {
        _statisticsService = statisticsService;
        _localizedFormattingService = localizedFormattingService;
        _localizationService = localizationService;
    }

    public ObservableCollection<TransactionDateGroup> TransactionGroups { get; } = new();

    public string CategoryId
    {
        get => _categoryId.ToString();
        set
        {
            if (int.TryParse(value, out var parsed))
            {
                _categoryId = parsed;
            }
        }
    }

    public string CategoryName
    {
        get => _categoryName;
        set
        {
            _categoryName = Uri.UnescapeDataString(value ?? string.Empty);
            OnPropertyChanged();
        }
    }

    public string Range
    {
        get => _range.ToString();
        set
        {
            if (Enum.TryParse<ExpenseCategoryReportRange>(value, true, out var parsed))
            {
                _range = parsed;
            }
        }
    }

    public string AnchorDate
    {
        get => _anchorDate.ToString("O");
        set
        {
            if (DateTime.TryParse(Uri.UnescapeDataString(value ?? string.Empty), out var parsed))
            {
                _anchorDate = parsed;
            }
        }
    }

    public string PeriodLabel
    {
        get => _periodLabel;
        set
        {
            _periodLabel = value;
            OnPropertyChanged();
        }
    }

    public string TotalAmountText
    {
        get => _totalAmountText;
        set
        {
            _totalAmountText = value;
            OnPropertyChanged();
        }
    }

    public bool HasTransactions
    {
        get => _hasTransactions;
        set
        {
            _hasTransactions = value;
            OnPropertyChanged();
        }
    }

    public async Task LoadAsync()
    {
        PeriodLabel = _localizedFormattingService.FormatCategoryReportPeriod(_range, _anchorDate);

        var details = await _statisticsService.GetExpenseCategoryTransactionsAsync(
            _categoryId,
            _categoryName,
            _range,
            _anchorDate);

        TransactionGroups.Clear();
        foreach (var group in ExpenseCategoryTransactionDetailReport.BuildGroups(details))
        {
            TransactionGroups.Add(new TransactionDateGroup(
                group.DateLabel,
                group.Items.Select(item => new TransactionItem
                {
                    Note = string.IsNullOrWhiteSpace(item.Note)
                        ? _localizationService.GetString("CategoryReportTransactionDetailNoNoteText")
                        : item.Note,
                    AmountText = item.Amount.ToString("N0"),
                    CurrencyText = item.Currency
                })));
        }

        HasTransactions = TransactionGroups.Count > 0;
        TotalAmountText = details.Sum(detail => detail.ConvertedAmount).ToString("N0");
    }
}
