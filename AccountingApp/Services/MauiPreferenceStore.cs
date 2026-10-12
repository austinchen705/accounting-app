using AccountingApp.Core.Abstractions;

namespace AccountingApp.Services;

public class MauiPreferenceStore : IPreferenceStore
{
    public string Get(string key, string defaultValue) => Preferences.Get(key, defaultValue);

    public void Set(string key, string value) => Preferences.Set(key, value);

    public bool Get(string key, bool defaultValue) => Preferences.Get(key, defaultValue);

    public void Set(string key, bool value) => Preferences.Set(key, value);
}
