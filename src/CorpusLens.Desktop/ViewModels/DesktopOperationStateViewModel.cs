namespace CorpusLens.Desktop.ViewModels;

public sealed class DesktopOperationStateViewModel : ViewModelBase
{
    private string _statusMessage = "Ready";
    private bool _isBusy;

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set => SetProperty(ref _isBusy, value);
    }

    public void Begin(string message)
    {
        StatusMessage = message;
        IsBusy = true;
    }

    public void Complete(string message)
    {
        StatusMessage = message;
        IsBusy = false;
    }

    public void SetStatus(string message)
    {
        StatusMessage = message;
    }

    public void End()
    {
        IsBusy = false;
    }
}
