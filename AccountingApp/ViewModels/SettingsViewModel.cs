using System.Collections.ObjectModel;
using System.Windows.Input;
using AccountingApp.Services;

namespace AccountingApp.ViewModels;

public class SettingsViewModel : BindableObject
{
    private readonly ExportService _exportService;
    private readonly ICloudService _icloudService;
    private string _selectedCurrency;

    public ObservableCollection<string> Currencies { get; } = new()
        { "TWD", "USD", "JPY", "EUR", "GBP", "CNY", "HKD", "AUD", "CAD", "SGD" };

    public string SelectedCurrency
    {
        get => _selectedCurrency;
        set
        {
            _selectedCurrency = value;
            Preferences.Set("base_currency", value);
            OnPropertyChanged();
        }
    }

    public ICommand ExportCsvCommand { get; }
    public ICommand ExportExcelCommand { get; }
    public ICommand BackupNowCommand { get; }
    public ICommand RestoreFromICloudCommand { get; }
    public ICommand ManageCategoriesCommand { get; }

    public SettingsViewModel(ExportService exportService, ICloudService icloudService)
    {
        _exportService = exportService;
        _icloudService = icloudService;
        _selectedCurrency = Preferences.Get("base_currency", "TWD");

        ExportCsvCommand = new Command(async () => await ExportAsync(false));
        ExportExcelCommand = new Command(async () => await ExportAsync(true));
        BackupNowCommand = new Command(async () => await BackupAsync());
        RestoreFromICloudCommand = new Command(async () => await RestoreAsync());
        ManageCategoriesCommand = new Command(async () =>
            await Shell.Current.GoToAsync("CategoryListPage"));
    }

    private async Task ExportAsync(bool excel)
    {
        string? path = excel
            ? await _exportService.ExportExcelAsync()
            : await _exportService.ExportCsvAsync();

        if (path is null)
        {
            await Application.Current!.Windows[0].Page!
                .DisplayAlert("提示", "尚無資料可匯出", "確定");
            return;
        }

        await ExportService.ShareFileAsync(path);
    }

    private async Task BackupAsync()
    {
        try
        {
            await _icloudService.UploadAsync();
            await Application.Current!.Windows[0].Page!
                .DisplayAlert("成功", "已備份至 iCloud", "確定");
        }
        catch (Exception ex)
        {
            await Application.Current!.Windows[0].Page!
                .DisplayAlert("錯誤", $"備份失敗：{ex.Message}", "確定");
        }
    }

    private async Task RestoreAsync()
    {
        bool confirm = await Application.Current!.Windows[0].Page!
            .DisplayAlert("確認還原", "從 iCloud 還原將覆蓋目前所有資料，確定繼續？", "還原", "取消");
        if (!confirm) return;

        try
        {
            await _icloudService.RestoreAsync();
            await Application.Current.Windows[0].Page!
                .DisplayAlert("成功", "已從 iCloud 還原資料", "確定");
        }
        catch (Exception ex)
        {
            await Application.Current.Windows[0].Page!
                .DisplayAlert("錯誤", ex.Message, "確定");
        }
    }
}
