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

    private static string FormatLanguageCodes(IReadOnlyList<string> languageCodes)
    {
        return languageCodes.Count == 0
            ? "unknown"
            : string.Join(", ", languageCodes);
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
