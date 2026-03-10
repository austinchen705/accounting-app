using AccountingApp.Services;

namespace AccountingApp;

public partial class App : Application
{
    private readonly DatabaseService _database;
    private readonly ICloudService _icloud;

    public App(DatabaseService database, ICloudService icloud)
    {
        InitializeComponent();
        _database = database;
        _icloud = icloud;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell());
    }

    protected override async void OnStart()
    {
        base.OnStart();
        await _database.InitializeAsync();
        await _icloud.SyncOnStartupAsync();
        await NotificationService.RequestPermissionAsync();
    }

    protected override async void OnResume()
    {
        base.OnResume();
        await _icloud.FlushPendingSyncAsync();
    }
}
