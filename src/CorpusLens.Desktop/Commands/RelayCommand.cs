using System;
using System.Windows.Input;

namespace CorpusLens.Desktop.Commands;

public sealed class RelayCommand : ICommand
{
    private readonly Action _execute;
    private readonly Func<bool>? _canExecute;
    private event EventHandler? CanExecuteChangedInternal;

    public RelayCommand(Action execute, Func<bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged
    {
        add => CanExecuteChangedInternal += value;
        remove => CanExecuteChangedInternal -= value;
    }

    public bool CanExecute(object? parameter)
    {
        return _canExecute?.Invoke() ?? true;
    }

    public void Execute(object? parameter)
    {
        _execute();
    }

    public void RaiseCanExecuteChanged()
    {
        CanExecuteChangedInternal?.Invoke(this, EventArgs.Empty);
    }
}
