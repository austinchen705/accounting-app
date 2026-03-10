using AccountingApp.Core.Abstractions;

namespace AccountingApp.Tests.Helpers;

public class FakePreferenceStore : IPreferenceStore
{
    private readonly Dictionary<string, string> _strings = new();
    private readonly Dictionary<string, bool> _bools = new();

    public string Get(string key, string defaultValue) =>
        _strings.TryGetValue(key, out var v) ? v : defaultValue;

    public void Set(string key, string value) => _strings[key] = value;

    public bool Get(string key, bool defaultValue) =>
        _bools.TryGetValue(key, out var v) ? v : defaultValue;

    public void Set(string key, bool value) => _bools[key] = value;
}
