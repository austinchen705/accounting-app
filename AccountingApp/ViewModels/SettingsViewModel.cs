using System.Collections.ObjectModel;
using System.Windows.Input;
using AccountingApp.Services;

namespace AccountingApp.ViewModels;

public class SettingsViewModel : BindableObject
{
    private readonly ExportService _exportService;
    private readonly GoogleDriveService _googleDriveService;
    private readonly JsonImportService _jsonImportService;
    private readonly DataRefreshService _refreshService;
    private string _selectedCurrency;
    private string _selectedGoogleDriveFolder;
    private bool _isGoogleDriveBusy;

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

    public string SelectedGoogleDriveFolder
    {
        get => _selectedGoogleDriveFolder;
        set
        {
            _selectedGoogleDriveFolder = value;
            OnPropertyChanged();
        }
    }

    public bool IsGoogleDriveBusy
    {
        get => _isGoogleDriveBusy;
        set
        {
            _isGoogleDriveBusy = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsGoogleDriveIdle));
        }
    }

    public bool IsGoogleDriveIdle => !IsGoogleDriveBusy;

    public ICommand ExportCsvCommand { get; }
    public ICommand ExportExcelCommand { get; }
    public ICommand BackupNowCommand { get; }
    public ICommand RestoreFromGoogleDriveCommand { get; }
    public ICommand SelectGoogleDriveFolderCommand { get; }
    public ICommand ManageCategoriesCommand { get; }
    public ICommand ImportJsonCommand { get; }
    public ICommand ImportJsonFromUrlCommand { get; }

    public SettingsViewModel(
        ExportService exportService,
        GoogleDriveService googleDriveService,
        JsonImportService jsonImportService,
        DataRefreshService refreshService)
    {
        _exportService = exportService;
        _googleDriveService = googleDriveService;
        _jsonImportService = jsonImportService;
        _refreshService = refreshService;
        _selectedCurrency = Preferences.Get("base_currency", "TWD");
        _selectedGoogleDriveFolder = Preferences.Get("google_drive_folder_name", "尚未設定");

        ExportCsvCommand = new Command(async () => await ExportAsync(false));
        ExportExcelCommand = new Command(async () => await ExportAsync(true));
        BackupNowCommand = new Command(async () => await BackupAsync());
        RestoreFromGoogleDriveCommand = new Command(async () => await RestoreAsync());
        SelectGoogleDriveFolderCommand = new Command(async () => await SelectGoogleDriveFolderAsync());
        ManageCategoriesCommand = new Command(async () =>
            await Shell.Current.GoToAsync("CategoryListPage"));
        ImportJsonCommand = new Command(async () => await ImportJsonAsync());
        ImportJsonFromUrlCommand = new Command(async () => await ImportJsonFromUrlAsync());
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
        if (IsGoogleDriveBusy) return;
        IsGoogleDriveBusy = true;
        try
        {
            await _googleDriveService.UploadAsync();
            await Application.Current!.Windows[0].Page!
                .DisplayAlert("成功", "已備份至 Google Drive", "確定");
        }
        catch (Exception ex)
        {
            await Application.Current!.Windows[0].Page!
                .DisplayAlert("錯誤", $"備份失敗：{ex.Message}", "確定");
        }
        finally
        {
            IsGoogleDriveBusy = false;
        }
    }

    private async Task RestoreAsync()
    {
        if (IsGoogleDriveBusy) return;
        bool confirm = await Application.Current!.Windows[0].Page!
            .DisplayAlert("確認還原", "從 Google Drive 還原將覆蓋目前所有資料，確定繼續？", "還原", "取消");
        if (!confirm) return;

        IsGoogleDriveBusy = true;
        try
        {
            await _googleDriveService.RestoreAsync();
            _refreshService.NotifyChanged();
            await Application.Current.Windows[0].Page!
                .DisplayAlert("成功", "已從 Google Drive 還原資料", "確定");
        }
        catch (Exception ex)
        {
            await Application.Current.Windows[0].Page!
                .DisplayAlert("錯誤", ex.Message, "確定");
        }
        finally
        {
            IsGoogleDriveBusy = false;
        }
    }

    private async Task SelectGoogleDriveFolderAsync()
    {
        if (IsGoogleDriveBusy) return;
        IsGoogleDriveBusy = true;
        try
        {
            await _googleDriveService.EnsureDefaultBackupFolderSelectedAsync();
            SelectedGoogleDriveFolder = _googleDriveService.FolderName ?? "personaccount_backup";
            await Application.Current!.Windows[0].Page!
                .DisplayAlert("成功", $"已設定備份資料夾：{SelectedGoogleDriveFolder}", "確定");
        }
        catch (Exception ex)
        {
            await Application.Current!.Windows[0].Page!
                .DisplayAlert("錯誤", ex.Message, "確定");
        }
        finally
        {
            IsGoogleDriveBusy = false;
        }
    }

    private async Task ImportJsonAsync()
    {
        try
        {
            var replaceMode = await AskImportModeAsync();
            if (replaceMode is null) return;

            var result = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "選擇 JSON 種子檔",
                FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.iOS, ["public.json"] },
                    { DevicePlatform.Android, ["application/json"] },
                    { DevicePlatform.WinUI, [".json"] },
                    { DevicePlatform.macOS, ["json"] }
                })
            });

            if (result is null) return;

            var imported = await _jsonImportService.ImportTransactionsAsync(result.FullPath, replaceMode.Value);
            _refreshService.NotifyChanged();
            var modeText = replaceMode.Value ? "（已覆蓋舊交易資料）" : "（已附加到現有資料）";
            await Application.Current!.Windows[0].Page!
                .DisplayAlert("匯入完成", $"成功匯入 {imported} 筆交易{modeText}", "確定");
        }
        catch (Exception ex)
        {
            await Application.Current!.Windows[0].Page!
                .DisplayAlert("匯入失敗", ex.Message, "確定");
        }
    }

    private async Task ImportJsonFromUrlAsync()
    {
        try
        {
            var replaceMode = await AskImportModeAsync();
            if (replaceMode is null) return;

            var defaultUrl = "http://127.0.0.1:8000/transactions.json";
            var url = await Application.Current!.Windows[0].Page!
                .DisplayPromptAsync("從 URL 匯入", "請輸入 JSON URL", initialValue: defaultUrl, keyboard: Keyboard.Url);

            if (string.IsNullOrWhiteSpace(url)) return;

            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(20) };
            var json = await http.GetStringAsync(url.Trim());
            var imported = await _jsonImportService.ImportTransactionsFromJsonAsync(json, replaceMode.Value);
            _refreshService.NotifyChanged();
            var modeText = replaceMode.Value ? "（已覆蓋舊交易資料）" : "（已附加到現有資料）";

            await Application.Current.Windows[0].Page!
                .DisplayAlert("匯入完成", $"成功匯入 {imported} 筆交易{modeText}", "確定");
        }
        catch (Exception ex)
        {
            await Application.Current!.Windows[0].Page!
                .DisplayAlert("匯入失敗", ex.Message, "確定");
        }
    }

    private async Task<bool?> AskImportModeAsync()
    {
        var page = Application.Current!.Windows[0].Page!;
        var action = await page.DisplayActionSheet("選擇匯入模式", "取消", null, "附加匯入", "覆蓋舊交易資料");
        return action switch
        {
            "附加匯入" => false,
            "覆蓋舊交易資料" => true,
            _ => null
        };
    }
}
