using AccountingApp.Services;
using Microsoft.Maui.Handlers;
using UIKit;

namespace AccountingApp.Platforms.iOS;

public static class IosEntryAccessory
{
    private static bool _isConfigured;

    public static readonly BindableProperty NextProperty = BindableProperty.CreateAttached(
        "Next",
        typeof(bool),
        typeof(IosEntryAccessory),
        false);

    public static bool GetNext(BindableObject bindable)
    {
        return (bool)bindable.GetValue(NextProperty);
    }

    public static void SetNext(BindableObject bindable, bool value)
    {
        bindable.SetValue(NextProperty, value);
    }

    public static void Configure()
    {
        if (_isConfigured)
        {
            return;
        }

        _isConfigured = true;
        EntryHandler.Mapper.AppendToMapping("IosEntryAccessory", (handler, view) =>
        {
            if (view is not BindableObject bindable || !GetNext(bindable))
            {
                return;
            }

            handler.PlatformView.InputAccessoryView = CreateNextToolbar(handler);
        });
    }

    private static UIToolbar CreateNextToolbar(IEntryHandler handler)
    {
        var toolbar = new UIToolbar();
        toolbar.SizeToFit();

        var nextButton = new UIBarButtonItem(
            "Next",
            UIBarButtonItemStyle.Done,
            (_, _) =>
            {
                handler.PlatformView?.SendActionForControlEvents(UIControlEvent.EditingDidEndOnExit);
                handler.PlatformView?.ResignFirstResponder();
            });

        nextButton.Title = LocalizationResourceManager.Instance.GetString("TransactionFormInputNextButton");
        toolbar.Items =
        [
            new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
            nextButton
        ];

        return toolbar;
    }
}
