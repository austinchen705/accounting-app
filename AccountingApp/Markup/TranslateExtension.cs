using AccountingApp.Services;
using Microsoft.Maui.Controls.Xaml;

namespace AccountingApp.Markup;

[ContentProperty(nameof(Key))]
public sealed class TranslateExtension : IMarkupExtension<BindingBase>
{
    public string Key { get; set; } = string.Empty;

    public BindingBase ProvideValue(IServiceProvider serviceProvider)
    {
        return new Binding($"[{Key}]", source: LocalizationResourceManager.Instance);
    }

    object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider)
    {
        return ProvideValue(serviceProvider);
    }
}
