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
        var shell = new AppShell();
        shell.BackgroundColor = Color.FromArgb("#10B981");
        return new Window(shell);
    }

    protected override async void OnStart()
    {
        base.OnStart();
        await _database.InitializeAsync();
        await NotificationService.RequestPermissionAsync();
    }
}
