using Microsoft.Extensions.Logging;
using AccountingApp.Services;
using AccountingApp.Views;
using AccountingApp.ViewModels;
using LiveChartsCore.SkiaSharpView.Maui;
using SkiaSharp.Views.Maui.Controls.Hosting;

namespace AccountingApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseSkiaSharp()
            .UseLiveCharts()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Services
        builder.Services.AddSingleton<DatabaseService>();
        builder.Services.AddSingleton<CurrencyService>();
        builder.Services.AddSingleton<CategoryService>();
        builder.Services.AddSingleton<TransactionService>();
        builder.Services.AddSingleton<DataRefreshService>();
        builder.Services.AddSingleton<BudgetService>();
        builder.Services.AddSingleton<StatisticsService>();
        builder.Services.AddSingleton<AssetSnapshotService>();
        builder.Services.AddSingleton<ExportService>();
        builder.Services.AddSingleton<JsonImportService>();
        builder.Services.AddSingleton<GoogleDriveService>();

        // ViewModels
        builder.Services.AddTransient<HomeViewModel>();
        builder.Services.AddTransient<TransactionListViewModel>();
        builder.Services.AddTransient<TransactionFormViewModel>();
        builder.Services.AddTransient<StatisticsViewModel>();
        builder.Services.AddTransient<CategoryReportViewModel>();
        builder.Services.AddTransient<BudgetViewModel>();
        builder.Services.AddTransient<SettingsViewModel>();
        builder.Services.AddTransient<AssetTrendViewModel>();
        builder.Services.AddTransient<CategoryListViewModel>();
        builder.Services.AddTransient<CategoryFormViewModel>();

        // Tab pages
        builder.Services.AddTransient<HomePage>();
        builder.Services.AddTransient<TransactionListPage>();
        builder.Services.AddTransient<StatisticsPage>();
        builder.Services.AddTransient<CategoryReportPage>();
        builder.Services.AddTransient<AssetTrendPage>();
        builder.Services.AddTransient<BudgetPage>();
        builder.Services.AddTransient<SettingsPage>();

        // Navigation pages
        builder.Services.AddTransient<TransactionFormPage>();
        builder.Services.AddTransient<CategoryListPage>();
        builder.Services.AddTransient<CategoryFormPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
