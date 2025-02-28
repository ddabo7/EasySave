using System;
using System.Windows.Input;

namespace EasySave.MVVM.Commands;

public class RelayCommand : ICommand
{
    private readonly Action<object?> _execute;
    private readonly Func<object?, bool>? _canExecute;
    private bool _isEnabled;

    public RelayCommand(Action execute, Func<bool>? canExecute = null)
    {
        _execute = _ => execute();
        _canExecute = canExecute == null ? null : _ => canExecute();
        _isEnabled = true;
    }

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter)
    {
        var result = _isEnabled && (_canExecute?.Invoke(parameter) ?? true);
        return result;
    }

    public void Execute(object? parameter) => _execute(parameter);

    public void RaiseCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    public void SetEnabled(bool enabled)
    {
        if (_isEnabled != enabled)
        {
            _isEnabled = enabled;
            RaiseCanExecuteChanged();
        }
    }
}

public class RelayCommand<T> : ICommand
{
    private readonly Action<T?> _execute;
    private readonly Func<T?, bool>? _canExecute;
    private bool _isEnabled;

    public RelayCommand(Action<T?> execute, Func<T?, bool>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
        _isEnabled = true;
    }

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter)
    {
        var result = _isEnabled && (_canExecute?.Invoke(parameter is T ? (T)parameter : default) ?? true);
        return result;
    }

    public void Execute(object? parameter)
    {
        if (parameter is T typedParameter)
        {
            _execute(typedParameter);
        }
        else
        {
            _execute(default);
        }
    }

    public void RaiseCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    public void SetEnabled(bool enabled)
    {
        if (_isEnabled != enabled)
        {
            _isEnabled = enabled;
            RaiseCanExecuteChanged();
        }
    }
}
