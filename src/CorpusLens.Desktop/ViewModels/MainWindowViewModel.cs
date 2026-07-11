using System.Collections.ObjectModel;
using CorpusLens.Analysis.Language;
using CorpusLens.Application.Queries;
using CorpusLens.Domain.Storage;

namespace CorpusLens.Desktop.ViewModels;

public sealed class MainWindowViewModel : ViewModelBase
{
    private const int DashboardWordLimit = 10;
    private const int DashboardPhraseLimit = 10;
    private const int DashboardMinPhraseCount = 3;
    private const int DashboardMinPhraseChapters = 2;
    private const int WordExplorerRelatedWordLimit = 10;
    private const int WordExplorerContextLimit = 10;
    private const int WordExplorerContextWords = 8;
    private const int WordExplorerBookLimit = 10;
    private const int CollocationExplorerDefaultLimit = 30;
    private const int PhraseExplorerDefaultLimit = 30;
    private const int CompareExplorerDefaultLimit = 30;

    private string _databasePath = "No database selected";
    private string _statusMessage = "Ready";
    private bool _isBusy;
    private RunListItemViewModel? _selectedRun;
    private string _runTitle = "Open a CorpusLens database";
    private string _runSubtitle = "Use Open database to load an existing corpuslens.db file.";
    private string _coreMetrics = "No run selected.";
    private string _profileSummary = "Corpus profile will appear here.";
    private string _difficultySummary = "Difficulty profile will appear here.";
    private string _topContentWords = "Top content words will appear here.";
    private string _topFunctionWords = "Top function words will appear here.";
    private string _recurringPhrases = "Recurring phrases will appear here.";
    private string _tokenIndexSummary = "Token index status will appear here.";
    private string _queryPathSummary = "Query path will appear here.";
    private string _reportPath = "Report path will appear here.";
    private string _wordExplorerTitle = "Word explorer";
    private string _wordExplorerSummary = "Select a run and search a word.";
    private string _wordNextWords = "Next words will appear here.";
    private string _wordPreviousWords = "Previous words will appear here.";
    private string _wordKwic = "KWIC contexts will appear here.";
    private string _wordBookDistribution = "Book distribution will appear here.";
    private string _collocationExplorerTitle = "Collocations explorer";
    private string _collocationExplorerSummary = "Select a run and search collocations.";
    private string _collocationResults = "Collocations will appear here.";
    private string _collocationFilterLabel = "Filter: all words";
    private string _phraseExplorerTitle = "Phrase explorer";
    private string _phraseExplorerSummary = "Select a run and search phrases.";
    private string _phraseResults = "Phrases will appear here.";
    private string _phraseBoundaryLabel = "Filter: all phrases";
    private string _comparisonExplorerTitle = "Compare runs";
    private string _comparisonExplorerSummary = "Open a database with at least two runs to compare corpora.";
    private string _comparisonWordSummary = "Word comparison will appear here.";
    private string _comparisonWords = "Word differences will appear here.";
    private string _comparisonDifficulty = "Difficulty comparison will appear here.";
    private string _comparisonWordFilterLabel = "Word filter: content words only";
    private string _comparisonPresenceLabel = "Presence: all words";
    private bool _phraseContentBoundaryOnly;
    private bool _phraseLongestOnly;
    private CollocationExplorerFilter _collocationFilter = CollocationExplorerFilter.All;
    private ComparisonWordFilter _comparisonWordFilter = ComparisonWordFilter.ContentOnly;
    private ComparisonPresenceFilter _comparisonPresenceFilter = ComparisonPresenceFilter.All;
    private RunListItemViewModel? _comparisonRightRun;

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

    public string ProfileSummary
    {
        get => _profileSummary;
        private set => SetProperty(ref _profileSummary, value);
    }

    public string DifficultySummary
    {
        get => _difficultySummary;
        private set => SetProperty(ref _difficultySummary, value);
    }

    public string TopContentWords
    {
        get => _topContentWords;
        private set => SetProperty(ref _topContentWords, value);
    }

    public string TopFunctionWords
    {
        get => _topFunctionWords;
        private set => SetProperty(ref _topFunctionWords, value);
    }

    public string RecurringPhrases
    {
        get => _recurringPhrases;
        private set => SetProperty(ref _recurringPhrases, value);
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


    public string WordExplorerTitle
    {
        get => _wordExplorerTitle;
        private set => SetProperty(ref _wordExplorerTitle, value);
    }

    public string WordExplorerSummary
    {
        get => _wordExplorerSummary;
        private set => SetProperty(ref _wordExplorerSummary, value);
    }

    public string WordNextWords
    {
        get => _wordNextWords;
        private set => SetProperty(ref _wordNextWords, value);
    }

    public string WordPreviousWords
    {
        get => _wordPreviousWords;
        private set => SetProperty(ref _wordPreviousWords, value);
    }

    public string WordKwic
    {
        get => _wordKwic;
        private set => SetProperty(ref _wordKwic, value);
    }

    public string WordBookDistribution
    {
        get => _wordBookDistribution;
        private set => SetProperty(ref _wordBookDistribution, value);
    }

    public string CollocationExplorerTitle
    {
        get => _collocationExplorerTitle;
        private set => SetProperty(ref _collocationExplorerTitle, value);
    }

    public string CollocationExplorerSummary
    {
        get => _collocationExplorerSummary;
        private set => SetProperty(ref _collocationExplorerSummary, value);
    }

    public string CollocationResults
    {
        get => _collocationResults;
        private set => SetProperty(ref _collocationResults, value);
    }

    public string CollocationFilterLabel
    {
        get => _collocationFilterLabel;
        private set => SetProperty(ref _collocationFilterLabel, value);
    }

    public CollocationExplorerFilter CollocationFilter
    {
        get => _collocationFilter;
        private set
        {
            if (SetProperty(ref _collocationFilter, value))
            {
                CollocationFilterLabel = $"Filter: {CollocationFilterText(value)}";
            }
        }
    }


    public string PhraseExplorerTitle
    {
        get => _phraseExplorerTitle;
        private set => SetProperty(ref _phraseExplorerTitle, value);
    }

    public string PhraseExplorerSummary
    {
        get => _phraseExplorerSummary;
        private set => SetProperty(ref _phraseExplorerSummary, value);
    }

    public string PhraseResults
    {
        get => _phraseResults;
        private set => SetProperty(ref _phraseResults, value);
    }

    public string PhraseBoundaryLabel
    {
        get => _phraseBoundaryLabel;
        private set => SetProperty(ref _phraseBoundaryLabel, value);
    }

    public bool PhraseContentBoundaryOnly
    {
        get => _phraseContentBoundaryOnly;
        private set
        {
            if (SetProperty(ref _phraseContentBoundaryOnly, value))
            {
                PhraseBoundaryLabel = value ? "Filter: content-word boundary" : "Filter: all phrases";
            }
        }
    }

    public bool PhraseLongestOnly
    {
        get => _phraseLongestOnly;
        private set => SetProperty(ref _phraseLongestOnly, value);
    }

    public RunListItemViewModel? ComparisonRightRun
    {
        get => _comparisonRightRun;
        private set => SetProperty(ref _comparisonRightRun, value);
    }

    public string ComparisonExplorerTitle
    {
        get => _comparisonExplorerTitle;
        private set => SetProperty(ref _comparisonExplorerTitle, value);
    }

    public string ComparisonExplorerSummary
    {
        get => _comparisonExplorerSummary;
        private set => SetProperty(ref _comparisonExplorerSummary, value);
    }

    public string ComparisonWordSummary
    {
        get => _comparisonWordSummary;
        private set => SetProperty(ref _comparisonWordSummary, value);
    }

    public string ComparisonWords
    {
        get => _comparisonWords;
        private set => SetProperty(ref _comparisonWords, value);
    }

    public string ComparisonDifficulty
    {
        get => _comparisonDifficulty;
        private set => SetProperty(ref _comparisonDifficulty, value);
    }

    public string ComparisonWordFilterLabel
    {
        get => _comparisonWordFilterLabel;
        private set => SetProperty(ref _comparisonWordFilterLabel, value);
    }

    public string ComparisonPresenceLabel
    {
        get => _comparisonPresenceLabel;
        private set => SetProperty(ref _comparisonPresenceLabel, value);
    }

    public ComparisonWordFilter ComparisonWordFilter
    {
        get => _comparisonWordFilter;
        private set
        {
            if (SetProperty(ref _comparisonWordFilter, value))
            {
                ComparisonWordFilterLabel = $"Word filter: {ComparisonWordFilterText(value)}";
            }
        }
    }

    public ComparisonPresenceFilter ComparisonPresenceFilter
    {
        get => _comparisonPresenceFilter;
        private set
        {
            if (SetProperty(ref _comparisonPresenceFilter, value))
            {
                ComparisonPresenceLabel = $"Presence: {ComparisonPresenceFilterText(value)}";
            }
        }
    }

    private void BeginBusy(string message)
    {
        IsBusy = true;
        StatusMessage = message;
    }

    private void EndBusy()
    {
        IsBusy = false;
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
            BeginBusy("Loading runs from database...");
            string databasePath = DatabasePath;

            IReadOnlyList<StoredAnalysisRunSummary> runs = await Task.Run(async () =>
            {
                AnalysisRunQueryService service = new(databasePath);
                return await service.ListRunsAsync(limit: 100, cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
            }, cancellationToken).ConfigureAwait(true);

            cancellationToken.ThrowIfCancellationRequested();

            Runs.Clear();
            foreach (StoredAnalysisRunSummary run in runs)
            {
                Runs.Add(new RunListItemViewModel(run));
            }
            EnsureDefaultComparisonRightRun();

            if (Runs.Count == 0)
            {
                ClearSelectedRun("The database was opened, but it does not contain analysis runs.");
                StatusMessage = "Database loaded. No runs found.";
                return;
            }

            StatusMessage = "Loading dashboard for first run...";
            await SelectRunCoreAsync(Runs[0], cancellationToken).ConfigureAwait(true);
            StatusMessage = $"Loaded {Runs.Count} run(s).";
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "Loading cancelled.";
        }
        catch (Exception ex)
        {
            Runs.Clear();
            ClearSelectedRun("Could not load runs from this database.");
            StatusMessage = $"Error loading database: {ex.Message}";
        }
        finally
        {
            EndBusy();
        }
    }

    public async Task SelectRunAsync(RunListItemViewModel? runItem, CancellationToken cancellationToken = default)
    {
        try
        {
            BeginBusy(runItem is null ? "Clearing selection..." : $"Loading run {runItem.Id}...");
            await SelectRunCoreAsync(runItem, cancellationToken).ConfigureAwait(true);
            StatusMessage = runItem is null ? "No run selected." : $"Run {runItem.Id} loaded.";
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "Run loading cancelled.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading run: {ex.Message}";
        }
        finally
        {
            EndBusy();
        }
    }

    private async Task SelectRunCoreAsync(RunListItemViewModel? runItem, CancellationToken cancellationToken)
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
        ProfileSummary = "Loading corpus profile...";
        DifficultySummary = "Loading difficulty profile...";
        TopContentWords = "Loading top content words...";
        TopFunctionWords = "Loading top function words...";
        RecurringPhrases = "Loading recurring phrases...";
        ReportPath = string.IsNullOrWhiteSpace(run.ReportPath)
            ? "Report: not available"
            : $"Report: {run.ReportPath}";
        ClearWordExplorer("Search a word in the selected run.");
        ClearCollocationExplorer("Search collocations in the selected run.");
        ClearPhraseExplorer("Search phrases in the selected run.");
        EnsureDefaultComparisonRightRun();
        ClearComparisonExplorer("Choose a right run and compare corpora.");

        StatusMessage = $"Loading health for run {run.Id}...";
        Task healthTask = LoadHealthAsync(run.Id, cancellationToken);

        StatusMessage = $"Loading profile for run {run.Id}...";
        Task profileTask = LoadProfileAsync(run.Id, cancellationToken);

        await Task.WhenAll(healthTask, profileTask).ConfigureAwait(true);
    }

    private async Task LoadHealthAsync(long runId, CancellationToken cancellationToken)
    {
        try
        {
            string databasePath = DatabasePath;
            TokenIndexHealthResult? health = await Task.Run(async () =>
            {
                TokenIndexHealthService healthService = new();
                return await healthService.GetHealthAsync(databasePath, runId, cancellationToken)
                    .ConfigureAwait(false);
            }, cancellationToken).ConfigureAwait(true);

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

    private async Task LoadProfileAsync(long runId, CancellationToken cancellationToken)
    {
        try
        {
            string databasePath = DatabasePath;
            CorpusProfileResult? profile = await Task.Run(async () =>
            {
                CorpusProfileQueryService profileService = new();
                return await profileService.GetProfileAsync(new CorpusProfileRequest(
                    databasePath,
                    runId,
                    DashboardWordLimit,
                    DashboardPhraseLimit,
                    DashboardMinPhraseCount,
                    DashboardMinPhraseChapters),
                    cancellationToken)
                    .ConfigureAwait(false);
            }, cancellationToken).ConfigureAwait(true);

            if (profile is null)
            {
                ProfileSummary = "Profile unavailable: run not found.";
                DifficultySummary = "Difficulty profile unavailable.";
                TopContentWords = "No content words available.";
                TopFunctionWords = "No function words available.";
                RecurringPhrases = "No recurring phrases available.";
                return;
            }

            ProfileSummary = FormatProfileSummary(profile);
            DifficultySummary = FormatDifficultySummary(profile);
            TopContentWords = FormatWords(profile.ContentWords);
            TopFunctionWords = FormatWords(profile.FunctionWords);
            RecurringPhrases = FormatPhrases(profile.Phrases);
        }
        catch (Exception ex)
        {
            ProfileSummary = $"Profile error: {ex.Message}";
            DifficultySummary = "Difficulty profile unavailable.";
            TopContentWords = "Top content words unavailable.";
            TopFunctionWords = "Top function words unavailable.";
            RecurringPhrases = "Recurring phrases unavailable.";
        }
    }


    public async Task SearchWordAsync(string? wordText, CancellationToken cancellationToken = default)
    {
        string normalizedInput = wordText?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(normalizedInput))
        {
            ClearWordExplorer("Enter a word to search in the selected run.");
            StatusMessage = "Enter a word to search.";
            return;
        }

        if (SelectedRun is null)
        {
            ClearWordExplorer("Select a run before searching a word.");
            StatusMessage = "Select a run before searching a word.";
            return;
        }

        try
        {
            BeginBusy($"Searching '{normalizedInput}' in run {SelectedRun.Id}...");
            string databasePath = DatabasePath;
            long runId = SelectedRun.Id;

            WordExplorerResult result = await Task.Run(async () =>
            {
                WordExplorerQueryService service = new();
                return await service.GetWordExplorerAsync(new WordExplorerRequest(
                        databasePath,
                        runId,
                        normalizedInput,
                        WordExplorerRelatedWordLimit,
                        WordExplorerContextLimit,
                        WordExplorerContextWords,
                        WordExplorerBookLimit),
                    cancellationToken)
                    .ConfigureAwait(false);
            }, cancellationToken).ConfigureAwait(true);

            cancellationToken.ThrowIfCancellationRequested();
            ApplyWordExplorerResult(normalizedInput, result);
            StatusMessage = result.Word is null
                ? $"Word '{normalizedInput}' was not found in run {runId}."
                : $"Word '{result.Word.Word}' loaded.";
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "Word search cancelled.";
        }
        catch (Exception ex)
        {
            ClearWordExplorer($"Word search error: {ex.Message}");
            StatusMessage = $"Error searching word: {ex.Message}";
        }
        finally
        {
            EndBusy();
        }
    }

    private void ApplyWordExplorerResult(string requestedWord, WordExplorerResult result)
    {
        if (result.Word is null)
        {
            WordExplorerTitle = $"Word explorer — {requestedWord}";
            WordExplorerSummary = "Word not found in the selected run.";
            WordNextWords = "No next words.";
            WordPreviousWords = "No previous words.";
            WordKwic = "No KWIC contexts.";
            WordBookDistribution = "No book distribution.";
            return;
        }

        StoredWordStatistic word = result.Word;
        WordExplorerTitle = $"Word explorer — {word.Word}";
        WordExplorerSummary = string.Join(Environment.NewLine,
            $"Type: {WordTypeLabel(word)}",
            $"Count: {word.Count:n0}",
            $"Documents: {word.DocumentCount:n0}",
            $"Per million: {FormatDouble(word.FrequencyPerMillion)}");
        WordNextWords = FormatNextWords(result.NextWords, useNextWord: true);
        WordPreviousWords = FormatNextWords(result.PreviousWords, useNextWord: false);
        WordKwic = FormatKwic(result.Contexts);
        WordBookDistribution = FormatBookDistribution(result.BookDistribution);
    }

    private void ClearWordExplorer(string message)
    {
        WordExplorerTitle = "Word explorer";
        WordExplorerSummary = message;
        WordNextWords = "Next words will appear here.";
        WordPreviousWords = "Previous words will appear here.";
        WordKwic = "KWIC contexts will appear here.";
        WordBookDistribution = "Book distribution will appear here.";
    }

    public void SetCollocationFilter(CollocationExplorerFilter filter)
    {
        CollocationFilter = filter;
    }

    public async Task SearchCollocationsAsync(
        string? wordText,
        string? windowText,
        string? minCountText,
        string? minDiceText,
        string? limitText,
        CancellationToken cancellationToken = default)
    {
        string normalizedInput = wordText?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(normalizedInput))
        {
            ClearCollocationExplorer("Enter a word to search collocations.");
            StatusMessage = "Enter a word to search collocations.";
            return;
        }

        if (SelectedRun is null)
        {
            ClearCollocationExplorer("Select a run before searching collocations.");
            StatusMessage = "Select a run before searching collocations.";
            return;
        }

        int window = ParseIntOrDefault(windowText, 4);
        int minCount = ParseIntOrDefault(minCountText, 1);
        int limit = ParseIntOrDefault(limitText, CollocationExplorerDefaultLimit);
        double minDice = ParseDoubleOrDefault(minDiceText, 0.0);

        try
        {
            BeginBusy($"Loading collocations for '{normalizedInput}'...");
            string databasePath = DatabasePath;
            long runId = SelectedRun.Id;
            CollocationExplorerFilter filter = CollocationFilter;

            CollocationExplorerResult result = await Task.Run(async () =>
            {
                CollocationExplorerQueryService service = new();
                return await service.GetCollocationsAsync(new CollocationExplorerRequest(
                        databasePath,
                        runId,
                        normalizedInput,
                        window,
                        limit,
                        minCount,
                        minDice,
                        filter),
                    cancellationToken)
                    .ConfigureAwait(false);
            }, cancellationToken).ConfigureAwait(true);

            cancellationToken.ThrowIfCancellationRequested();
            ApplyCollocationExplorerResult(result);
            StatusMessage = $"Loaded {result.Collocations.Count:n0} collocation(s) for '{result.Word}'.";
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "Collocation search cancelled.";
        }
        catch (Exception ex)
        {
            ClearCollocationExplorer($"Collocation search error: {ex.Message}");
            StatusMessage = $"Error searching collocations: {ex.Message}";
        }
        finally
        {
            EndBusy();
        }
    }

    private void ApplyCollocationExplorerResult(CollocationExplorerResult result)
    {
        CollocationExplorerTitle = $"Collocations — {result.Word}";
        CollocationExplorerSummary = string.Join(Environment.NewLine,
            $"Window: {result.Window} words per side",
            $"{CollocationFilterLabel}",
            $"Minimum count: {result.MinCount:n0}",
            $"Minimum Dice: {FormatDouble(result.MinDice)}",
            $"Matched collocates: {result.MatchedCount:n0}",
            $"Shown collocates: {result.Collocations.Count:n0} of {result.MatchedCount:n0}");
        CollocationResults = FormatCollocations(result.Collocations);
    }

    private void ClearCollocationExplorer(string message)
    {
        CollocationExplorerTitle = "Collocations explorer";
        CollocationExplorerSummary = message;
        CollocationResults = "Collocations will appear here.";
    }

    public void SetPhraseContentBoundary(bool enabled)
    {
        PhraseContentBoundaryOnly = enabled;
    }

    public void SetPhraseLongestOnly(bool enabled)
    {
        PhraseLongestOnly = enabled;
    }

    public async Task SearchPhrasesAsync(
        string? minNText,
        string? maxNText,
        string? minCountText,
        string? minChaptersText,
        string? limitText,
        CancellationToken cancellationToken = default)
    {
        if (SelectedRun is null)
        {
            ClearPhraseExplorer("Select a run before searching phrases.");
            StatusMessage = "Select a run before searching phrases.";
            return;
        }

        int minN = ParseIntOrDefault(minNText, 2);
        int maxN = ParseIntOrDefault(maxNText, 5);
        int minCount = ParseIntOrDefault(minCountText, 3);
        int minChapters = ParseIntOrDefault(minChaptersText, 2);
        int limit = ParseIntOrDefault(limitText, PhraseExplorerDefaultLimit);

        try
        {
            BeginBusy($"Loading phrases for run {SelectedRun.Id}...");
            string databasePath = DatabasePath;
            long runId = SelectedRun.Id;
            bool contentBoundaryOnly = PhraseContentBoundaryOnly;
            bool longestOnly = PhraseLongestOnly;

            PhraseExplorerResult result = await Task.Run(async () =>
            {
                PhraseExplorerQueryService service = new();
                return await service.GetPhrasesAsync(new PhraseExplorerRequest(
                        databasePath,
                        runId,
                        minN,
                        maxN,
                        minCount,
                        minChapters,
                        limit,
                        contentBoundaryOnly,
                        longestOnly),
                    cancellationToken)
                    .ConfigureAwait(false);
            }, cancellationToken).ConfigureAwait(true);

            cancellationToken.ThrowIfCancellationRequested();
            ApplyPhraseExplorerResult(result);
            StatusMessage = $"Loaded {result.Phrases.Count:n0} phrase(s).";
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "Phrase search cancelled.";
        }
        catch (Exception ex)
        {
            ClearPhraseExplorer($"Phrase search error: {ex.Message}");
            StatusMessage = $"Error searching phrases: {ex.Message}";
        }
        finally
        {
            EndBusy();
        }
    }

    private void ApplyPhraseExplorerResult(PhraseExplorerResult result)
    {
        PhraseExplorerTitle = "Phrase explorer";
        PhraseExplorerSummary = string.Join(Environment.NewLine,
            result.MinN == result.MaxN ? $"N: {result.MinN}" : $"N range: {result.MinN}-{result.MaxN}",
            $"Minimum count: {result.MinCount:n0}",
            $"Minimum chapters: {result.MinChapters:n0}",
            result.ContentBoundaryOnly ? "Filter: content-word boundary" : "Filter: all phrases",
            result.LongestOnly ? "Nested phrases: longest only" : "Nested phrases: shown",
            $"Fetched candidates: {result.FetchedCount:n0}",
            $"Matched phrases: {result.MatchedCount:n0}",
            $"Shown phrases: {result.Phrases.Count:n0} of {result.MatchedCount:n0}");
        PhraseResults = FormatPhraseExplorerItems(result.Phrases);
    }

    private void ClearPhraseExplorer(string message)
    {
        PhraseExplorerTitle = "Phrase explorer";
        PhraseExplorerSummary = message;
        PhraseResults = "Phrases will appear here.";
    }


    public void SetComparisonRightRun(RunListItemViewModel? run)
    {
        ComparisonRightRun = run;
    }

    public void SetComparisonWordFilter(ComparisonWordFilter filter)
    {
        ComparisonWordFilter = filter;
    }

    public void SetComparisonPresenceFilter(ComparisonPresenceFilter filter)
    {
        ComparisonPresenceFilter = filter;
    }

    public async Task CompareRunsAsync(
        string? wordText,
        string? minCountText,
        string? limitText,
        CancellationToken cancellationToken = default)
    {
        if (SelectedRun is null)
        {
            ClearComparisonExplorer("Select a left run before comparing corpora.");
            StatusMessage = "Select a left run before comparing corpora.";
            return;
        }

        if (ComparisonRightRun is null)
        {
            ClearComparisonExplorer("Choose a right run before comparing corpora.");
            StatusMessage = "Choose a right run before comparing corpora.";
            return;
        }

        if (SelectedRun.Id == ComparisonRightRun.Id)
        {
            ClearComparisonExplorer("Choose two different runs.");
            StatusMessage = "Choose two different runs to compare.";
            return;
        }

        string word = wordText?.Trim() ?? string.Empty;
        int minCount = ParseIntOrDefault(minCountText, 5);
        int limit = ParseIntOrDefault(limitText, CompareExplorerDefaultLimit);

        try
        {
            BeginBusy($"Comparing run {SelectedRun.Id} and run {ComparisonRightRun.Id}...");
            string databasePath = DatabasePath;
            long leftRunId = SelectedRun.Id;
            long rightRunId = ComparisonRightRun.Id;
            ComparisonWordFilter wordFilter = ComparisonWordFilter;
            ComparisonPresenceFilter presenceFilter = ComparisonPresenceFilter;

            RunComparisonQueryService service = new();
            Task<CompareWordResult?> wordTask = string.IsNullOrWhiteSpace(word)
                ? Task.FromResult<CompareWordResult?>(null)
                : Task.Run(async () => (CompareWordResult?)await service.CompareWordAsync(new CompareWordRequest(
                        databasePath,
                        leftRunId,
                        rightRunId,
                        word),
                    cancellationToken).ConfigureAwait(false), cancellationToken);

            Task<CompareWordsResult> wordsTask = Task.Run(async () => await service.CompareWordsAsync(new CompareWordsRequest(
                    databasePath,
                    leftRunId,
                    rightRunId,
                    limit,
                    minCount,
                    wordFilter,
                    presenceFilter),
                cancellationToken).ConfigureAwait(false), cancellationToken);

            Task<CompareDifficultyResult> difficultyTask = Task.Run(async () => await service.CompareDifficultyAsync(new CompareDifficultyRequest(
                    databasePath,
                    leftRunId,
                    rightRunId),
                cancellationToken).ConfigureAwait(false), cancellationToken);

            await Task.WhenAll(wordTask, wordsTask, difficultyTask).ConfigureAwait(true);
            cancellationToken.ThrowIfCancellationRequested();

            ApplyComparisonResults(
                wordTask.Result,
                wordsTask.Result,
                difficultyTask.Result);
            StatusMessage = $"Compared run {leftRunId} and run {rightRunId}.";
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "Run comparison cancelled.";
        }
        catch (Exception ex)
        {
            ClearComparisonExplorer($"Comparison error: {ex.Message}");
            StatusMessage = $"Error comparing runs: {ex.Message}";
        }
        finally
        {
            EndBusy();
        }
    }

    private void ApplyComparisonResults(
        CompareWordResult? wordResult,
        CompareWordsResult wordsResult,
        CompareDifficultyResult difficultyResult)
    {
        RunComparisonContext context = wordsResult.Context;
        ComparisonExplorerTitle = "Compare runs";
        ComparisonExplorerSummary = string.Join(Environment.NewLine,
            $"Left: {RunLabel(context.LeftRun)}",
            $"Right: {RunLabel(context.RightRun)}",
            context.HasDifferentLanguages
                ? $"Note: lexical comparison, not translated ({context.LeftLanguageCode} vs {context.RightLanguageCode})."
                : "Languages: comparable lexical forms",
            $"Word filter: {ComparisonWordFilterText(wordsResult.WordFilter)}",
            $"Presence: {ComparisonPresenceFilterText(wordsResult.PresenceFilter)}",
            $"Minimum count: {wordsResult.MinCount:n0}",
            $"Matched words: {wordsResult.MatchedCount:n0}",
            $"Shown words: {wordsResult.Comparisons.Count:n0} of {wordsResult.MatchedCount:n0}");
        ComparisonWordSummary = wordResult is null
            ? "Enter a word to compare one form directly."
            : FormatCompareWord(wordResult);
        ComparisonWords = FormatCompareWords(wordsResult.Comparisons);
        ComparisonDifficulty = FormatCompareDifficulty(difficultyResult);
    }

    private void ClearComparisonExplorer(string message)
    {
        ComparisonExplorerTitle = "Compare runs";
        ComparisonExplorerSummary = message;
        ComparisonWordSummary = "Word comparison will appear here.";
        ComparisonWords = "Word differences will appear here.";
        ComparisonDifficulty = "Difficulty comparison will appear here.";
    }

    private void EnsureDefaultComparisonRightRun()
    {
        if (Runs.Count == 0)
        {
            ComparisonRightRun = null;
            return;
        }

        if (ComparisonRightRun is not null
            && Runs.Any(run => ReferenceEquals(run, ComparisonRightRun))
            && (SelectedRun is null || ComparisonRightRun.Id != SelectedRun.Id))
        {
            return;
        }

        ComparisonRightRun = Runs.FirstOrDefault(run => SelectedRun is null || run.Id != SelectedRun.Id)
            ?? Runs.FirstOrDefault();
    }

    private void ClearSelectedRun(string message)
    {
        SelectedRun = null;
        RunTitle = "No run selected";
        RunSubtitle = message;
        CoreMetrics = "No metrics available.";
        ProfileSummary = "Corpus profile will appear here.";
        DifficultySummary = "Difficulty profile will appear here.";
        TopContentWords = "Top content words will appear here.";
        TopFunctionWords = "Top function words will appear here.";
        RecurringPhrases = "Recurring phrases will appear here.";
        TokenIndexSummary = "Token index status will appear here.";
        QueryPathSummary = "Query path will appear here.";
        ReportPath = "Report path will appear here.";
        ClearWordExplorer("Select a run and search a word.");
        ClearCollocationExplorer("Select a run and search collocations.");
        ClearPhraseExplorer("Select a run and search phrases.");
        ClearComparisonExplorer("Select a left run before comparing corpora.");
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

    private static string FormatProfileSummary(CorpusProfileResult profile)
    {
        LanguageProfile languageProfile = profile.LanguageProfile;
        return string.Join(Environment.NewLine,
            $"Language: {FormatLanguageCodes(profile.LanguageCodes)}",
            $"Profile: {languageProfile.Name} ({languageProfile.Code})",
            $"Source books: {profile.SourceBookCount:n0}",
            $"Chapters: {profile.ChapterCount:n0}",
            $"Stopwords: {languageProfile.StopWordCount:n0}",
            $"Phrase filters: count >= {DashboardMinPhraseCount}; chapters >= {DashboardMinPhraseChapters}; longest only");
    }

    private static string FormatDifficultySummary(CorpusProfileResult profile)
    {
        StoredDifficultyProfile? difficulty = profile.DifficultyProfile;
        if (difficulty is null)
        {
            return "Difficulty profile unavailable.";
        }

        return string.Join(Environment.NewLine,
            $"Score: {FormatDouble(difficulty.HeuristicScore)}",
            $"Long words: {FormatPercent(difficulty.LongWordShare)} (>={profile.DifficultyThresholds.LongWordLength} chars)",
            $"Very long: {FormatPercent(difficulty.VeryLongWordShare)} (>={profile.DifficultyThresholds.VeryLongWordLength} chars)",
            $"Content share: {FormatPercent(difficulty.ContentWordShare)}",
            $"Function share: {FormatPercent(difficulty.FunctionWordShare)}",
            $"Lexical diversity / 1k: {FormatDouble(difficulty.LexicalDiversityPerThousand)}");
    }

    private static string FormatWords(IReadOnlyList<StoredWordStatistic> words)
    {
        if (words.Count == 0)
        {
            return "No words available.";
        }

        IEnumerable<string> lines = words.Select((word, index) =>
            $"{index + 1,2}. {TrimForColumn(word.Word, 18),-18} {word.Count,7:n0}  {FormatDouble(word.FrequencyPerMillion),8}/M");
        return string.Join(Environment.NewLine, lines);
    }

    private static string FormatPhrases(IReadOnlyList<StoredPhraseStatistic> phrases)
    {
        if (phrases.Count == 0)
        {
            return "No recurring phrases available.";
        }

        IEnumerable<string> lines = phrases.Select((phrase, index) =>
            $"{index + 1,2}. {TrimForColumn(phrase.Phrase, 26),-26} {phrase.Count,5:n0}  ch {phrase.ChapterCount:n0}");
        return string.Join(Environment.NewLine, lines);
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


    private static string FormatNextWords(IReadOnlyList<StoredNextWordStatistic> words, bool useNextWord)
    {
        if (words.Count == 0)
        {
            return useNextWord ? "No next-word statistics found." : "No previous-word statistics found.";
        }

        IEnumerable<string> lines = words.Select((item, index) =>
        {
            string value = useNextWord ? item.NextWord : item.Word;
            return $"{index + 1,2}. {TrimForColumn(value, 18),-18} {item.Count,6:n0}  {FormatProbability(item.Probability),7}";
        });
        return string.Join(Environment.NewLine, lines);
    }

    private static string FormatKwic(IReadOnlyList<StoredWordContext> contexts)
    {
        if (contexts.Count == 0)
        {
            return "No contexts found.";
        }

        IEnumerable<string> lines = contexts.Select((context, index) =>
        {
            string chapter = string.IsNullOrWhiteSpace(context.ChapterTitle)
                ? $"Chapter {context.ChapterOrderIndex}"
                : context.ChapterTitle;
            return $"{index + 1,2}. {TrimForColumn(chapter, 24),-24}  {TrimForColumn(context.LeftContext, 28),-28}  [{context.MatchText}]  {TrimForColumn(context.RightContext, 28)}";
        });
        return string.Join(Environment.NewLine, lines);
    }

    private static string FormatBookDistribution(IReadOnlyList<StoredWordBookStatistic> books)
    {
        if (books.Count == 0)
        {
            return "No matching source books found.";
        }

        IEnumerable<string> lines = books.Select((book, index) =>
            $"{index + 1,2}. {TrimForColumn(book.Title, 24),-24} {book.Count,6:n0}  {FormatDouble(book.FrequencyPerMillion),8}/M");
        return string.Join(Environment.NewLine, lines);
    }

    private static string FormatCollocations(IReadOnlyList<CollocationExplorerItem> collocations)
    {
        if (collocations.Count == 0)
        {
            return "No collocations found for the selected filter and thresholds.";
        }

        IEnumerable<string> lines = collocations.Select((item, index) =>
            $"{index + 1,2}. {TrimForColumn(item.Collocate, 18),-18} {item.WordType,-8} {item.Count,5:n0}  L {item.LeftCount,4:n0}  R {item.RightCount,4:n0}  {FormatDouble(item.RatePerTarget),6}/t  d {FormatDouble(item.AverageDistance),5}  dice {FormatDouble(item.DiceCoefficient),5}");
        return string.Join(Environment.NewLine, lines);
    }

    private static string FormatPhraseExplorerItems(IReadOnlyList<PhraseExplorerItem> phrases)
    {
        if (phrases.Count == 0)
        {
            return "No phrases found for the selected thresholds.";
        }

        IEnumerable<string> lines = phrases.Select((item, index) =>
            $"{index + 1,2}. {TrimForColumn(item.Phrase, 30),-30} n {item.N}  {item.Count,5:n0}  ch {item.ChapterCount,4:n0}  {FormatDouble(item.FrequencyPerMillion),8}/M  {item.Boundary}");
        return string.Join(Environment.NewLine, lines);
    }


    private static string FormatCompareWord(CompareWordResult result)
    {
        WordComparisonItem item = result.Comparison;
        return string.Join(Environment.NewLine,
            $"Word: {item.Word}",
            $"Type: {(item.IsFunctionWord ? "function" : "content")}",
            $"Left count: {item.LeftCount:n0} ({FormatDouble(item.LeftFrequencyPerMillion)}/M)",
            $"Right count: {item.RightCount:n0} ({FormatDouble(item.RightFrequencyPerMillion)}/M)",
            $"Difference/M: {FormatSignedDouble(item.DifferencePerMillion)}",
            $"Ratio left/right: {FormatRatio(item.Ratio)}",
            $"Favours: {item.Direction}");
    }

    private static string FormatCompareWords(IReadOnlyList<WordComparisonItem> comparisons)
    {
        if (comparisons.Count == 0)
        {
            return "No word differences matched the selected filters.";
        }

        IEnumerable<string> lines = comparisons.Select((item, index) =>
            $"{index + 1,2}. {TrimForColumn(item.Word, 16),-16} {(item.IsFunctionWord ? "function" : "content"),-8} L {item.LeftCount,6:n0} {FormatDouble(item.LeftFrequencyPerMillion),7}/M  R {item.RightCount,6:n0} {FormatDouble(item.RightFrequencyPerMillion),7}/M  Δ {FormatSignedDouble(item.DifferencePerMillion),8}  {item.Direction}");
        return string.Join(Environment.NewLine, lines);
    }

    private static string FormatCompareDifficulty(CompareDifficultyResult result)
    {
        return string.Join(Environment.NewLine,
            $"Language profile: {(result.LanguageProfile.IsKnown ? $"{result.LanguageProfile.Name} ({result.LanguageProfile.Code})" : $"generic defaults ({result.LanguageProfile.Code})")}",
            $"Thresholds: >= {result.Thresholds.LongWordLength} / >= {result.Thresholds.VeryLongWordLength} chars",
            $"Left score: {FormatDouble(result.LeftProfile.HeuristicScore)}",
            $"Right score: {FormatDouble(result.RightProfile.HeuristicScore)}",
            $"Score difference: {FormatSignedDouble(result.ScoreDifference)}",
            $"Relatively harder: {result.Direction}",
            $"Left avg sent/word: {FormatDouble(result.LeftProfile.AverageWordsPerSentence)} / {FormatDouble(result.LeftProfile.AverageCharactersPerWord)}",
            $"Right avg sent/word: {FormatDouble(result.RightProfile.AverageWordsPerSentence)} / {FormatDouble(result.RightProfile.AverageCharactersPerWord)}");
    }

    private static string RunLabel(StoredAnalysisRunSummary run)
    {
        return $"{run.CorpusName} / {run.BookTitle} ({run.Id})";
    }

    private static string WordTypeLabel(StoredWordStatistic word)
    {
        return word.IsStopWord ? "function" : "content";
    }

    private static string FormatProbability(double value)
    {
        return value.ToString("P2");
    }

    private static int ParseIntOrDefault(string? value, int fallback)
    {
        return int.TryParse(value, out int parsed) ? parsed : fallback;
    }

    private static double ParseDoubleOrDefault(string? value, double fallback)
    {
        return double.TryParse(value, out double parsed) ? parsed : fallback;
    }


    private static string ComparisonWordFilterText(ComparisonWordFilter filter)
    {
        return filter switch
        {
            ComparisonWordFilter.ContentOnly => "content words only",
            ComparisonWordFilter.FunctionOnly => "function words only",
            _ => "all words"
        };
    }

    private static string ComparisonPresenceFilterText(ComparisonPresenceFilter filter)
    {
        return filter switch
        {
            ComparisonPresenceFilter.SharedOnly => "shared words only",
            ComparisonPresenceFilter.ExclusiveOnly => "exclusive words only",
            _ => "all words"
        };
    }

    private static string CollocationFilterText(CollocationExplorerFilter filter)
    {
        return filter switch
        {
            CollocationExplorerFilter.ContentOnly => "content words only",
            CollocationExplorerFilter.FunctionOnly => "function words only",
            _ => "all words"
        };
    }

    private static string FormatLanguageCodes(IReadOnlyList<string> languageCodes)
    {
        return languageCodes.Count == 0
            ? "unknown"
            : string.Join(", ", languageCodes);
    }


    private static string FormatSignedDouble(double value)
    {
        if (Math.Abs(value) < 0.005)
        {
            return "0";
        }

        return value > 0 ? $"+{FormatDouble(value)}" : FormatDouble(value);
    }

    private static string FormatRatio(double value)
    {
        if (double.IsPositiveInfinity(value))
        {
            return "inf";
        }

        if (double.IsNaN(value))
        {
            return "n/a";
        }

        if (value > 0 && value < 0.01)
        {
            return "<0.01";
        }

        return FormatDouble(value);
    }

    private static string FormatDouble(double value)
    {
        return value.ToString("0.##");
    }

    private static string FormatPercent(double value)
    {
        return value.ToString("P2");
    }

    private static string TrimForColumn(string value, int maxLength)
    {
        if (value.Length <= maxLength)
        {
            return value;
        }

        return maxLength <= 1
            ? value[..maxLength]
            : value[..(maxLength - 1)] + "…";
    }
}
