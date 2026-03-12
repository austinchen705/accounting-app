namespace AccountingApp.Services;

public class DataRefreshService
{
    public event Action? DataChanged;

    public void NotifyChanged() => DataChanged?.Invoke();
}
