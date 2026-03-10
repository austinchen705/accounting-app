namespace AccountingApp.Core.Abstractions;

public interface IPreferenceStore
{
    string Get(string key, string defaultValue);
    void Set(string key, string value);
    bool Get(string key, bool defaultValue);
    void Set(string key, bool value);
}
