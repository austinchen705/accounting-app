using AccountingApp.Services;
using AutoBackupCoordinator = AccountingApp.Core.Services.AutoBackupCoordinator;

namespace AccountingApp;

public partial class App : Application
{
    private readonly DatabaseService _database;
    private readonly ILocalizationService _localizationService;
    private readonly AutoBackupCoordinator _autoBackup;

    public App(DatabaseService database, ILocalizationService localizationService, AutoBackupCoordinator autoBackup)
    {
        InitializeComponent();
        _database = database;
        _localizationService = localizationService;
        _autoBackup = autoBackup;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(CreateShell());
    }

    protected override async void OnStart()
    {
        base.OnStart();
        await _database.InitializeAsync();
        await NotificationService.RequestPermissionAsync();
        await _autoBackup.RunIfDueAsync();
    }

    protected override async void OnResume()
    {
        base.OnResume();
        await _autoBackup.RunIfDueAsync();
    }

    public async Task ResetShellForLanguageChangeAsync()
    {
        if (Windows.Count == 0)
        {
            return;
        }

        var window = Windows[0];
        var shell = CreateShell();
        window.Page = shell;
        await Task.Yield();
        await shell.GoToAsync("//HomePage");
    }

    private AppShell CreateShell()
    {
        var shell = new AppShell(_localizationService);
        shell.BackgroundColor = Color.FromArgb("#10B981");
        return shell;
    }
}
