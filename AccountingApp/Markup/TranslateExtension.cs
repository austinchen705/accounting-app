using System.Globalization;
using System.Reflection;
using System.Resources;
using Microsoft.Maui.Controls.Xaml;

namespace AccountingApp.Markup;

[ContentProperty(nameof(Key))]
public sealed class TranslateExtension : IMarkupExtension<string>
{
    private static readonly ResourceManager ResourceManager = new(
        "AccountingApp.Resources.Strings.AppResources",
        typeof(TranslateExtension).GetTypeInfo().Assembly);

    public string Key { get; set; } = string.Empty;

    public string ProvideValue(IServiceProvider serviceProvider)
    {
        return ResourceManager.GetString(Key, CultureInfo.CurrentUICulture) ?? Key;
    }

    object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider)
    {
        return ProvideValue(serviceProvider);
    }
}
