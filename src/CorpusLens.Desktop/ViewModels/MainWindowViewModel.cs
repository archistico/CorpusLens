using System.Collections.ObjectModel;
using CorpusLens.Application.Queries;
using CorpusLens.Domain.Storage;

namespace CorpusLens.Desktop.ViewModels;

public sealed class MainWindowViewModel : ViewModelBase
{
    private string _databasePath = "No database selected";
    private string _statusMessage = "Ready";
    private bool _isBusy;
    private RunListItemViewModel? _selectedRun;
    private string _runTitle = "Open a CorpusLens database";
    private string _runSubtitle = "Use Open database to load an existing corpuslens.db file.";
    private string _coreMetrics = "No run selected.";
    private string _tokenIndexSummary = "Token index status will appear here.";
    private string _queryPathSummary = "Query path will appear here.";
    private string _reportPath = "Report path will appear here.";

    public string DatabasePath
    {
        get => _databasePath;
        private set => SetProperty(ref _databasePath, value);
    }

    public ObservableCollection<RunListItemViewModel> Runs { get; } = new();

    public RunListItemViewModel? SelectedRun
    {
        get => _selectedRun;
        private set => SetProperty(ref _selectedRun, value);
    }

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

    public string RunTitle
    {
        get => _runTitle;
        private set => SetProperty(ref _runTitle, value);
    }

    public string RunSubtitle
    {
        get => _runSubtitle;
        private set => SetProperty(ref _runSubtitle, value);
    }

    public string CoreMetrics
    {
        get => _coreMetrics;
        private set => SetProperty(ref _coreMetrics, value);
    }

    public string TokenIndexSummary
    {
        get => _tokenIndexSummary;
        private set => SetProperty(ref _tokenIndexSummary, value);
    }

    public string QueryPathSummary
    {
        get => _queryPathSummary;
        private set => SetProperty(ref _queryPathSummary, value);
    }

    public string ReportPath
    {
        get => _reportPath;
        private set => SetProperty(ref _reportPath, value);
    }

    public async Task OpenDatabaseAsync(string databasePath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(databasePath))
        {
            return;
        }

        if (!File.Exists(databasePath))
        {
            StatusMessage = $"Database not found: {databasePath}";
            return;
        }

        DatabasePath = databasePath;
        await RefreshRunsAsync(cancellationToken).ConfigureAwait(true);
    }

    public async Task RefreshRunsAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(DatabasePath))
        {
            Runs.Clear();
            ClearSelectedRun("No valid database selected.");
            StatusMessage = "Open a CorpusLens SQLite database first.";
            return;
        }

        try
        {
            IsBusy = true;
            StatusMessage = "Loading runs...";

            AnalysisRunQueryService service = new(DatabasePath);
            IReadOnlyList<StoredAnalysisRunSummary> runs = await service
                .ListRunsAsync(limit: 100, cancellationToken: cancellationToken)
                .ConfigureAwait(true);

            Runs.Clear();
            foreach (StoredAnalysisRunSummary run in runs)
            {
                Runs.Add(new RunListItemViewModel(run));
            }

            if (Runs.Count == 0)
            {
                ClearSelectedRun("The database was opened, but it does not contain analysis runs.");
                StatusMessage = "Database loaded. No runs found.";
                return;
            }

            await SelectRunAsync(Runs[0], cancellationToken).ConfigureAwait(true);
            StatusMessage = $"Loaded {Runs.Count} run(s).";
        }
        catch (Exception ex)
        {
            Runs.Clear();
            ClearSelectedRun("Could not load runs from this database.");
            StatusMessage = $"Error loading database: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task SelectRunAsync(RunListItemViewModel? runItem, CancellationToken cancellationToken = default)
    {
        SelectedRun = runItem;
        if (runItem is null)
        {
            ClearSelectedRun("No run selected.");
            return;
        }

        StoredAnalysisRunSummary run = runItem.Summary;
        RunTitle = $"Run {run.Id} — {run.CorpusName}";
        RunSubtitle = $"{run.BookTitle} · {run.Status}";
        CoreMetrics = FormatCoreMetrics(run);
        ReportPath = string.IsNullOrWhiteSpace(run.ReportPath)
            ? "Report: not available"
            : $"Report: {run.ReportPath}";

        try
        {
            TokenIndexHealthService healthService = new();
            TokenIndexHealthResult? health = await healthService
                .GetHealthAsync(DatabasePath, run.Id, cancellationToken)
                .ConfigureAwait(true);

            if (health is null)
            {
                TokenIndexSummary = "Token index: health unavailable";
                QueryPathSummary = "Query path: unavailable";
                return;
            }

            TokenIndexSummary = FormatTokenIndexSummary(health);
            QueryPathSummary = FormatQueryPathSummary(health.Diagnostics);
        }
        catch (Exception ex)
        {
            TokenIndexSummary = $"Token index: error reading health ({ex.Message})";
            QueryPathSummary = "Query path: unavailable";
        }
    }

    private void ClearSelectedRun(string message)
    {
        SelectedRun = null;
        RunTitle = "No run selected";
        RunSubtitle = message;
        CoreMetrics = "No metrics available.";
        TokenIndexSummary = "Token index status will appear here.";
        QueryPathSummary = "Query path will appear here.";
        ReportPath = "Report path will appear here.";
    }

    private static string FormatCoreMetrics(StoredAnalysisRunSummary run)
    {
        return string.Join(Environment.NewLine,
            $"Sentences: {run.SentenceCount:n0}",
            $"Tokens: {run.TokenCount:n0}",
            $"Word tokens: {run.WordTokenCount:n0}",
            $"Distinct words: {run.DistinctWordCount:n0}",
            $"Avg words/sentence: {run.AverageWordsPerSentence:n2}",
            $"Avg chars/word: {run.AverageCharactersPerWord:n2}");
    }

    private static string FormatTokenIndexSummary(TokenIndexHealthResult health)
    {
        StoredTokenIndexDiagnostics? diagnostics = health.Diagnostics;
        if (diagnostics is null || !diagnostics.IsIndexed)
        {
            return "Token index: missing; legacy fallback will be used.";
        }

        return string.Join(Environment.NewLine,
            $"Token index: indexed",
            $"Coverage: {diagnostics.WordTokenCoveragePercentage:n2}% ({diagnostics.IndexedWordTokenCount:n0} of {diagnostics.ExpectedWordTokenCount:n0})",
            $"Chapters: {diagnostics.IndexedChapterCount:n0} of {diagnostics.ExpectedChapterCount:n0}",
            $"Source books: {diagnostics.IndexedSourceBookCount:n0} of {diagnostics.SourceBookCount:n0}",
            $"Run position gaps: {diagnostics.RunPositionGapCount:n0}",
            health.Warnings.Count == 0 ? "Health: OK" : $"Warnings: {string.Join("; ", health.Warnings)}");
    }

    private static string FormatQueryPathSummary(StoredTokenIndexDiagnostics? diagnostics)
    {
        bool contextIndex = diagnostics?.CanUseTokenIndexForContextQueries == true;
        bool wordBooksIndex = diagnostics?.CanUseTokenIndexForWordBookDistribution == true;
        return string.Join(Environment.NewLine,
            $"KWIC: {(contextIndex ? "token index" : "legacy fallback")}",
            $"Collocations: {(contextIndex ? "token index" : "legacy fallback")}",
            $"Phrases: {(contextIndex ? "token index" : "legacy fallback")}",
            $"Word-books: {(wordBooksIndex ? "token index" : "legacy fallback")}");
    }
}
