using AccountingApp.Services;
using AccountingApp.Views;

namespace AccountingApp;

public partial class AppShell : Shell
{
    private readonly ILocalizationService _localizationService;

    public AppShell(ILocalizationService localizationService)
    {
        _localizationService = localizationService;
        InitializeComponent();
        ApplyLocalizedTitles();

        Routing.RegisterRoute("TransactionFormPage", typeof(TransactionFormPage));
        Routing.RegisterRoute("CategoryListPage", typeof(CategoryListPage));
        Routing.RegisterRoute("CategoryFormPage", typeof(CategoryFormPage));
        Routing.RegisterRoute("AssetTrendPage", typeof(AssetTrendPage));
        Routing.RegisterRoute("AssetTrendChartPage", typeof(AssetTrendChartPage));
        Routing.RegisterRoute(nameof(TransactionImageViewerPage), typeof(TransactionImageViewerPage));
        Routing.RegisterRoute(nameof(CategoryReportTransactionDetailPage), typeof(CategoryReportTransactionDetailPage));
    }

    private void ApplyLocalizedTitles()
    {
        HomeTab.Title = _localizationService.GetString("HomeTabTitle");
        TransactionListTab.Title = _localizationService.GetString("TransactionListTabTitle");
        StatisticsTab.Title = _localizationService.GetString("StatisticsTabTitle");
        CategoryReportTab.Title = _localizationService.GetString("CategoryReportTabTitle");
        AssetTrendTab.Title = _localizationService.GetString("AssetTrendTabTitle");
        CategoryManagementTab.Title = _localizationService.GetString("CategoryManagementTabTitle");
        BudgetTab.Title = _localizationService.GetString("BudgetTabTitle");
        SettingsTab.Title = _localizationService.GetString("SettingsTabTitle");
    }
}
