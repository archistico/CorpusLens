using CorpusLens.Analysis.Language;
using CorpusLens.Application.Queries;
using CorpusLens.Domain.Storage;

namespace CorpusLens.Desktop.ViewModels;

public sealed class DashboardViewModel : ViewModelBase
{
    private const int WordLimit = 10;
    private const int PhraseLimit = 10;
    private const int MinPhraseCount = 3;
    private const int MinPhraseChapters = 2;

    private readonly Func<CorpusProfileRequest, CancellationToken, Task<CorpusProfileResult?>> _profileLoader;
    private readonly Func<string, long, CancellationToken, Task<TokenIndexHealthResult?>> _healthLoader;

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

    public DashboardViewModel(
        Func<CorpusProfileRequest, CancellationToken, Task<CorpusProfileResult?>>? profileLoader = null,
        Func<string, long, CancellationToken, Task<TokenIndexHealthResult?>>? healthLoader = null)
    {
        _profileLoader = profileLoader ?? LoadProfileFromApplicationAsync;
        _healthLoader = healthLoader ?? LoadHealthFromApplicationAsync;
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

    public void Prepare(StoredAnalysisRunSummary run)
    {
        ArgumentNullException.ThrowIfNull(run);

        RunTitle = $"Run {run.Id} — {run.CorpusName}";
        RunSubtitle = $"{run.BookTitle} · {run.Status}";
        CoreMetrics = FormatCoreMetrics(run);
        ProfileSummary = "Loading corpus profile...";
        DifficultySummary = "Loading difficulty profile...";
        TopContentWords = "Loading top content words...";
        TopFunctionWords = "Loading top function words...";
        RecurringPhrases = "Loading recurring phrases...";
        TokenIndexSummary = "Loading token-index health...";
        QueryPathSummary = "Loading query paths...";
        ReportPath = string.IsNullOrWhiteSpace(run.ReportPath)
            ? "Report: not available"
            : $"Report: {run.ReportPath}";
    }

    public async Task LoadAsync(string databasePath, long runId, CancellationToken cancellationToken = default)
    {
        Task profileTask = LoadProfileAsync(databasePath, runId, cancellationToken);
        Task healthTask = LoadHealthAsync(databasePath, runId, cancellationToken);
        await Task.WhenAll(profileTask, healthTask).ConfigureAwait(true);
    }

    public void Clear(string message)
    {
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

    private async Task LoadHealthAsync(string databasePath, long runId, CancellationToken cancellationToken)
    {
        try
        {
            TokenIndexHealthResult? health = await _healthLoader(databasePath, runId, cancellationToken)
                .ConfigureAwait(true);
            cancellationToken.ThrowIfCancellationRequested();

            if (health is null)
            {
                TokenIndexSummary = "Token index: health unavailable";
                QueryPathSummary = "Query path: unavailable";
                return;
            }

            TokenIndexSummary = FormatTokenIndexSummary(health);
            QueryPathSummary = FormatQueryPathSummary(health.Diagnostics);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            TokenIndexSummary = $"Token index: error reading health ({ex.Message})";
            QueryPathSummary = "Query path: unavailable";
        }
    }

    private async Task LoadProfileAsync(string databasePath, long runId, CancellationToken cancellationToken)
    {
        try
        {
            CorpusProfileResult? profile = await _profileLoader(new CorpusProfileRequest(
                    databasePath,
                    runId,
                    WordLimit,
                    PhraseLimit,
                    MinPhraseCount,
                    MinPhraseChapters),
                cancellationToken).ConfigureAwait(true);
            cancellationToken.ThrowIfCancellationRequested();

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
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
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

    private static Task<CorpusProfileResult?> LoadProfileFromApplicationAsync(
        CorpusProfileRequest request,
        CancellationToken cancellationToken)
    {
        return Task.Run(async () =>
        {
            CorpusProfileQueryService service = new();
            return await service.GetProfileAsync(request, cancellationToken).ConfigureAwait(false);
        }, cancellationToken);
    }

    private static Task<TokenIndexHealthResult?> LoadHealthFromApplicationAsync(
        string databasePath,
        long runId,
        CancellationToken cancellationToken)
    {
        return Task.Run(async () =>
        {
            TokenIndexHealthService service = new();
            return await service.GetHealthAsync(databasePath, runId, cancellationToken).ConfigureAwait(false);
        }, cancellationToken);
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
            $"Language: {DesktopTextFormatter.FormatLanguageCodes(profile.LanguageCodes)}",
            $"Profile: {languageProfile.Name} ({languageProfile.Code})",
            $"Source books: {profile.SourceBookCount:n0}",
            $"Chapters: {profile.ChapterCount:n0}",
            $"Stopwords: {languageProfile.StopWordCount:n0}",
            $"Phrase filters: count >= {MinPhraseCount}; chapters >= {MinPhraseChapters}; longest only");
    }

    private static string FormatDifficultySummary(CorpusProfileResult profile)
    {
        StoredDifficultyProfile? difficulty = profile.DifficultyProfile;
        if (difficulty is null)
        {
            return "Difficulty profile unavailable.";
        }

        return string.Join(Environment.NewLine,
            $"Score: {DesktopTextFormatter.FormatDouble(difficulty.HeuristicScore)}",
            $"Long words: {DesktopTextFormatter.FormatPercent(difficulty.LongWordShare)} (>={profile.DifficultyThresholds.LongWordLength} chars)",
            $"Very long: {DesktopTextFormatter.FormatPercent(difficulty.VeryLongWordShare)} (>={profile.DifficultyThresholds.VeryLongWordLength} chars)",
            $"Content share: {DesktopTextFormatter.FormatPercent(difficulty.ContentWordShare)}",
            $"Function share: {DesktopTextFormatter.FormatPercent(difficulty.FunctionWordShare)}",
            $"Lexical diversity / 1k: {DesktopTextFormatter.FormatDouble(difficulty.LexicalDiversityPerThousand)}");
    }

    private static string FormatWords(IReadOnlyList<StoredWordStatistic> words)
    {
        if (words.Count == 0)
        {
            return "No words available.";
        }

        IEnumerable<string> lines = words.Select((word, index) =>
            $"{index + 1,2}. {DesktopTextFormatter.TrimForColumn(word.Word, 18),-18} {word.Count,7:n0}  {DesktopTextFormatter.FormatDouble(word.FrequencyPerMillion),8}/M");
        return string.Join(Environment.NewLine, lines);
    }

    private static string FormatPhrases(IReadOnlyList<StoredPhraseStatistic> phrases)
    {
        if (phrases.Count == 0)
        {
            return "No recurring phrases available.";
        }

        IEnumerable<string> lines = phrases.Select((phrase, index) =>
            $"{index + 1,2}. {DesktopTextFormatter.TrimForColumn(phrase.Phrase, 26),-26} {phrase.Count,5:n0}  ch {phrase.ChapterCount:n0}");
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
            "Token index: indexed",
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
