using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Microsoft.Maui.Controls;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class QueryPropertyAttribute : Attribute
{
    public QueryPropertyAttribute(string name, string queryId)
    {
        Name = name;
        QueryId = queryId;
    }

    public string Name { get; }
    public string QueryId { get; }
}

public abstract class BindableObject : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

public sealed class Command : ICommand
{
    private readonly Action? _execute;
    private readonly Func<Task>? _executeAsync;

    public Command(Action execute)
    {
        _execute = execute;
    }

    public Command(Func<Task> executeAsync)
    {
        _executeAsync = executeAsync;
    }

    public event EventHandler? CanExecuteChanged
    {
        add { }
        remove { }
    }

    public bool CanExecute(object? parameter) => true;

    public void Execute(object? parameter)
    {
        if (_executeAsync is not null)
        {
            _ = _executeAsync();
            return;
        }

        _execute?.Invoke();
    }
}

public sealed class Command<T> : ICommand
{
    private readonly Action<T?> _execute;

    public Command(Action<T?> execute)
    {
        _execute = execute;
    }

    public event EventHandler? CanExecuteChanged
    {
        add { }
        remove { }
    }

    public bool CanExecute(object? parameter) => true;

    public void Execute(object? parameter) => _execute(parameter is null ? default : (T?)parameter);
}

public sealed class Shell
{
    public static Shell Current { get; set; } = new();

    public Task GoToAsync(string route) => Task.CompletedTask;
}
