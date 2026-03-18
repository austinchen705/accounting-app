using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Resources;

namespace AccountingApp.Services;

public sealed class LocalizationResourceManager : INotifyPropertyChanged
{
    private static readonly ResourceManager ResourceManager = new(
        "AccountingApp.Resources.Strings.AppResources",
        typeof(LocalizationResourceManager).GetTypeInfo().Assembly);

    public static LocalizationResourceManager Instance { get; } = new();

    public event PropertyChangedEventHandler? PropertyChanged;

    public string this[string key] => ResourceManager.GetString(key, CultureInfo.CurrentUICulture) ?? key;

    public string GetString(string key)
    {
        return this[key];
    }

    public void Refresh()
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
    }
}
