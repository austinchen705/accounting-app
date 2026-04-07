using AccountingApp.ViewModels;

namespace AccountingApp.Views;

public partial class TransactionFormPage : ContentPage
{
    private readonly TransactionFormViewModel _vm;

    public TransactionFormPage(TransactionFormViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;
        FormCalendarDatePicker.CalendarOpened += OnCalendarOpened;
        FormCalendarDatePicker.CalendarCompleted += OnCalendarCompleted;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.InitializeAsync();
    }

    protected override void OnDisappearing()
    {
        FormCalendarDatePicker.CalendarOpened -= OnCalendarOpened;
        FormCalendarDatePicker.CalendarCompleted -= OnCalendarCompleted;
        base.OnDisappearing();
    }

    private void OnAmountEntryCompleted(object? sender, EventArgs e)
    {
        CategoryPicker.Focus();
    }

    private async void OnCalendarOpened(object? sender, EventArgs e)
    {
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            await Task.Yield();
            var targetY = Math.Max(0, FormCalendarDatePicker.Y - 24);
            await FormScrollView.ScrollToAsync(0, targetY, true);
        });
    }

    private void OnCalendarCompleted(object? sender, EventArgs e)
    {
        NoteEntry.Focus();
    }
}
