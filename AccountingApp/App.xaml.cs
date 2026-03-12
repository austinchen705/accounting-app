using AccountingApp.Services;

namespace AccountingApp;

public partial class App : Application
{
    private readonly DatabaseService _database;

    public App(DatabaseService database)
    {
        InitializeComponent();
        _database = database;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell());
    }

    protected override async void OnStart()
    {
        base.OnStart();
        await _database.InitializeAsync();
        await NotificationService.RequestPermissionAsync();
    }
}
