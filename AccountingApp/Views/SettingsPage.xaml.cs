using AccountingApp.ViewModels;

namespace AccountingApp.Views;

public partial class SettingsPage : ContentPage
{
    public SettingsPage(SettingsViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        (BindingContext as SettingsViewModel)?.RefreshAutoBackupStatus();
    }
}
