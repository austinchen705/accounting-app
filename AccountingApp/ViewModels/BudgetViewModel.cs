using System.Collections.ObjectModel;
using System.Windows.Input;
using AccountingApp.Models;
using AccountingApp.Services;

namespace AccountingApp.ViewModels;

public class BudgetItemViewModel : BindableObject
{
    public string CategoryName { get; set; } = string.Empty;
    public decimal BudgetAmount { get; set; }
    public decimal SpentAmount { get; set; }
    public decimal Ratio { get; set; }
    public Color BarColor => Ratio > 1.0m ? Colors.Red : Ratio > 0.8m ? Colors.Orange : Colors.Green;
    public string RatioText => $"{SpentAmount:N0} / {BudgetAmount:N0} ({Ratio:P0})";
}

public class BudgetViewModel : BindableObject
{
    private readonly BudgetService _budgetService;
    private readonly CategoryService _categoryService;
    private string _currentMonth;
    private bool _hasBudgetItems;

    public ObservableCollection<BudgetItemViewModel> BudgetItems { get; } = new();
    public ICommand SetBudgetCommand { get; }
    public bool HasBudgetItems
    {
        get => _hasBudgetItems;
        set { _hasBudgetItems = value; OnPropertyChanged(); }
    }

    public BudgetViewModel(BudgetService budgetService, CategoryService categoryService)
    {
        _budgetService = budgetService;
        _categoryService = categoryService;
        _currentMonth = DateTime.Today.ToString("yyyy-MM");
        SetBudgetCommand = new Command<BudgetItemViewModel>(async item => await SetBudgetAsync(item));
    }

    public async Task LoadAsync()
    {
        var usage = await _budgetService.GetMonthUsageAsync(_currentMonth);
        var allCategories = await _categoryService.GetByTypeAsync("expense");

        BudgetItems.Clear();
        foreach (var cat in allCategories)
        {
            var u = usage.FirstOrDefault(x => x.CategoryId == cat.Id);
            BudgetItems.Add(new BudgetItemViewModel
            {
                CategoryName = cat.Name,
                BudgetAmount = u.BudgetAmount,
                SpentAmount = u.SpentAmount,
                Ratio = u.Ratio
            });
        }

        HasBudgetItems = BudgetItems.Count > 0;
    }

    private async Task SetBudgetAsync(BudgetItemViewModel item)
    {
        var result = await Application.Current!.Windows[0].Page!
            .DisplayPromptAsync("設定預算", $"請輸入「{item.CategoryName}」本月預算金額", keyboard: Keyboard.Numeric);
        if (result == null) return;
        if (!decimal.TryParse(result, out var amount) || amount <= 0)
        {
            await Application.Current.Windows[0].Page!.DisplayAlert("錯誤", "請輸入有效金額", "確定");
            return;
        }

        var cats = await _categoryService.GetByTypeAsync("expense");
        var cat = cats.FirstOrDefault(c => c.Name == item.CategoryName);
        if (cat == null) return;

        await _budgetService.SetBudgetAsync(cat.Id, amount, _currentMonth);
        await LoadAsync();
    }
}
