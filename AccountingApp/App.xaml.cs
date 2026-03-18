using AccountingApp.Services;

namespace AccountingApp;

public partial class App : Application
{
    private readonly DatabaseService _database;
    private readonly ILocalizationService _localizationService;

    public App(DatabaseService database, ILocalizationService localizationService)
    {
        InitializeComponent();
        _database = database;
        _localizationService = localizationService;
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
