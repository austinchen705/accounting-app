using AccountingApp.ViewModels;
using AccountingApp.Services;

namespace AccountingApp.Views;

public partial class TransactionFormPage : ContentPage
{
    private readonly TransactionFormViewModel _vm;
    private readonly TransactionImageService _transactionImageService;

    public TransactionFormPage(TransactionFormViewModel vm, TransactionImageService transactionImageService)
    {
        InitializeComponent();
        _vm = vm;
        _transactionImageService = transactionImageService;
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

    private async void OnCaptureAttachmentClicked(object? sender, EventArgs e)
    {
        if (!MediaPicker.Default.IsCaptureSupported)
        {
            return;
        }

        var photo = await MediaPicker.Default.CapturePhotoAsync();
        await StageImportedAttachmentAsync(photo);
    }

    private async void OnPickAttachmentFromLibraryClicked(object? sender, EventArgs e)
    {
        var photo = await MediaPicker.Default.PickPhotoAsync();
        await StageImportedAttachmentAsync(photo);
    }

    private async void OnViewAttachmentClicked(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_vm.AttachmentImageRelativePath))
        {
            return;
        }

        await Shell.Current.GoToAsync(nameof(TransactionImageViewerPage), new Dictionary<string, object> { ["path"] = _vm.AttachmentImageRelativePath });
    }

    private async void OnReplaceAttachmentClicked(object? sender, EventArgs e)
    {
        await OnPickAttachmentFromLibraryClicked(sender, e);
    }

    private void OnRemoveAttachmentClicked(object? sender, EventArgs e)
    {
        _vm.RemoveAttachmentImage();
    }

    private async Task StageImportedAttachmentAsync(FileResult? photo)
    {
        if (photo is null || string.IsNullOrWhiteSpace(photo.FullPath))
        {
            return;
        }

        var relativePath = await _transactionImageService.ImportAsync(photo.FullPath);
        _vm.StageAttachmentImage(relativePath);
    }
}
