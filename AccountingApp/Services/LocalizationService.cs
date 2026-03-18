using System.Globalization;
using System.Reflection;
using System.Resources;

namespace AccountingApp.Services;

public interface ILocalizationService
{
    string CurrentLanguage { get; }
    void Initialize();
    Task SetLanguageAsync(string languageCode);
    string GetString(string key);
}

public sealed class LocalizationService : ILocalizationService
{
    public const string DefaultLanguage = "zh-Hant";
    private const string LanguagePreferenceKey = "app_language";
    private static readonly ResourceManager ResourceManager = new(
        "AccountingApp.Resources.Strings.AppResources",
        typeof(LocalizationService).GetTypeInfo().Assembly);

    public string CurrentLanguage { get; private set; } = DefaultLanguage;

    public LocalizationService()
    {
        Initialize();
    }

    public void Initialize()
    {
        CurrentLanguage = NormalizeLanguage(Preferences.Get(LanguagePreferenceKey, DefaultLanguage));
        ApplyCulture(CurrentLanguage);
    }

    public Task SetLanguageAsync(string languageCode)
    {
        CurrentLanguage = NormalizeLanguage(languageCode);
        Preferences.Set(LanguagePreferenceKey, CurrentLanguage);
        ApplyCulture(CurrentLanguage);
        return Task.CompletedTask;
    }

    public string GetString(string key)
    {
        return ResourceManager.GetString(key, CultureInfo.CurrentUICulture) ?? key;
    }

    private static string NormalizeLanguage(string? languageCode)
    {
        return string.Equals(languageCode, "en", StringComparison.OrdinalIgnoreCase)
            ? "en"
            : DefaultLanguage;
    }

    private static void ApplyCulture(string languageCode)
    {
        var culture = CultureInfo.GetCultureInfo(languageCode);
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
    }
}
