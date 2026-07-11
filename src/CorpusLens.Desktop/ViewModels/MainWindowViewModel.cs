using System.Collections.ObjectModel;
using System.Windows.Input;
using CorpusLens.Desktop.Commands;

namespace CorpusLens.Desktop.ViewModels;

public sealed class MainWindowViewModel : ViewModelBase
{
    private string _databasePath = "No database selected";
    private string _selectedRun = "No run selected";
    private string _statusMessage = "Ready";

    public MainWindowViewModel()
    {
        OpenDatabaseCommand = new RelayCommand(() => SetPlaceholderStatus("Open database"));
        RefreshCommand = new RelayCommand(() => SetPlaceholderStatus("Refresh"));

        Runs.Add("Open a CorpusLens database to load runs");
    }

    public string DatabasePath
    {
        get => _databasePath;
        private set => SetProperty(ref _databasePath, value);
    }

    public ObservableCollection<string> Runs { get; } = new();

    public string SelectedRun
    {
        get => _selectedRun;
        set => SetProperty(ref _selectedRun, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public ICommand OpenDatabaseCommand { get; }

    public ICommand RefreshCommand { get; }

    public string EmptyStateTitle => "CorpusLens Desktop";

    public string EmptyStateMessage => "Milestone 18.1 adds the Avalonia shell only. Database loading and run navigation arrive in Milestone 18.2.";

    private void SetPlaceholderStatus(string action)
    {
        StatusMessage = $"{action} will be implemented in Milestone 18.2.";
    }
}
