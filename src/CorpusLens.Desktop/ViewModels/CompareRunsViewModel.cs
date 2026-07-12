using CorpusLens.Application.Queries;
using CorpusLens.Domain.Storage;

namespace CorpusLens.Desktop.ViewModels;

public sealed class CompareRunsViewModel : ViewModelBase
{
    private const int DefaultLimit = 30;

    private readonly Func<CompareWordRequest, CancellationToken, Task<CompareWordResult>> _wordComparer;
    private readonly Func<CompareWordsRequest, CancellationToken, Task<CompareWordsResult>> _wordsComparer;
    private readonly Func<CompareDifficultyRequest, CancellationToken, Task<CompareDifficultyResult>> _difficultyComparer;

    private string _comparisonExplorerTitle = "Compare runs";
    private string _comparisonExplorerSummary = "Open a database with at least two runs to compare corpora.";
    private string _comparisonWordSummary = "Word comparison will appear here.";
    private string _comparisonWords = "Word differences will appear here.";
    private string _comparisonDifficulty = "Difficulty comparison will appear here.";
    private string _comparisonWordFilterLabel = "Word filter: content words only";
    private string _comparisonPresenceLabel = "Presence: all words";
    private ComparisonWordFilter _comparisonWordFilter = ComparisonWordFilter.ContentOnly;
    private ComparisonPresenceFilter _comparisonPresenceFilter = ComparisonPresenceFilter.All;
    private RunListItemViewModel? _comparisonRightRun;

    public CompareRunsViewModel(
        Func<CompareWordRequest, CancellationToken, Task<CompareWordResult>>? wordComparer = null,
        Func<CompareWordsRequest, CancellationToken, Task<CompareWordsResult>>? wordsComparer = null,
        Func<CompareDifficultyRequest, CancellationToken, Task<CompareDifficultyResult>>? difficultyComparer = null)
    {
        _wordComparer = wordComparer ?? CompareWordFromApplicationAsync;
        _wordsComparer = wordsComparer ?? CompareWordsFromApplicationAsync;
        _difficultyComparer = difficultyComparer ?? CompareDifficultyFromApplicationAsync;
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
                ComparisonWordFilterLabel = $"Word filter: {WordFilterText(value)}";
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
                ComparisonPresenceLabel = $"Presence: {PresenceFilterText(value)}";
            }
        }
    }

    public void SetRightRun(RunListItemViewModel? run)
    {
        ComparisonRightRun = run;
    }

    public void SetWordFilter(ComparisonWordFilter filter)
    {
        ComparisonWordFilter = filter;
    }

    public void SetPresenceFilter(ComparisonPresenceFilter filter)
    {
        ComparisonPresenceFilter = filter;
    }

    public void EnsureDefaultRightRun(
        IReadOnlyList<RunListItemViewModel> runs,
        RunListItemViewModel? selectedRun)
    {
        if (runs.Count == 0)
        {
            ComparisonRightRun = null;
            return;
        }

        if (ComparisonRightRun is not null
            && runs.Any(run => ReferenceEquals(run, ComparisonRightRun))
            && (selectedRun is null || ComparisonRightRun.Id != selectedRun.Id))
        {
            return;
        }

        ComparisonRightRun = runs.FirstOrDefault(run => selectedRun is null || run.Id != selectedRun.Id)
            ?? runs.FirstOrDefault();
    }

    public async Task<string> CompareAsync(
        string databasePath,
        RunListItemViewModel leftRun,
        string? wordText,
        string? minCountText,
        string? limitText,
        CancellationToken cancellationToken = default)
    {
        if (ComparisonRightRun is null)
        {
            throw new InvalidOperationException("Choose a right run before comparing corpora.");
        }

        string word = wordText?.Trim() ?? string.Empty;
        int minCount = DesktopTextFormatter.ParseIntOrDefault(minCountText, 5);
        int limit = DesktopTextFormatter.ParseIntOrDefault(limitText, DefaultLimit);
        long leftRunId = leftRun.Id;
        long rightRunId = ComparisonRightRun.Id;
        ComparisonWordFilter wordFilter = ComparisonWordFilter;
        ComparisonPresenceFilter presenceFilter = ComparisonPresenceFilter;

        Task<CompareWordResult?> wordTask = string.IsNullOrWhiteSpace(word)
            ? Task.FromResult<CompareWordResult?>(null)
            : CompareOptionalWordAsync(new CompareWordRequest(databasePath, leftRunId, rightRunId, word), cancellationToken);

        Task<CompareWordsResult> wordsTask = _wordsComparer(new CompareWordsRequest(
            databasePath,
            leftRunId,
            rightRunId,
            limit,
            minCount,
            wordFilter,
            presenceFilter), cancellationToken);

        Task<CompareDifficultyResult> difficultyTask = _difficultyComparer(new CompareDifficultyRequest(
            databasePath,
            leftRunId,
            rightRunId), cancellationToken);

        await Task.WhenAll(wordTask, wordsTask, difficultyTask).ConfigureAwait(true);
        cancellationToken.ThrowIfCancellationRequested();

        ApplyResults(
            await wordTask.ConfigureAwait(true),
            await wordsTask.ConfigureAwait(true),
            await difficultyTask.ConfigureAwait(true));
        return $"Compared run {leftRunId} and run {rightRunId}.";
    }

    public void Clear(string message)
    {
        ComparisonExplorerTitle = "Compare runs";
        ComparisonExplorerSummary = message;
        ComparisonWordSummary = "Word comparison will appear here.";
        ComparisonWords = "Word differences will appear here.";
        ComparisonDifficulty = "Difficulty comparison will appear here.";
    }

    private async Task<CompareWordResult?> CompareOptionalWordAsync(
        CompareWordRequest request,
        CancellationToken cancellationToken)
    {
        return await _wordComparer(request, cancellationToken).ConfigureAwait(false);
    }

    private void ApplyResults(
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
            $"Word filter: {WordFilterText(wordsResult.WordFilter)}",
            $"Presence: {PresenceFilterText(wordsResult.PresenceFilter)}",
            $"Minimum count: {wordsResult.MinCount:n0}",
            $"Matched words: {wordsResult.MatchedCount:n0}",
            $"Shown words: {wordsResult.Comparisons.Count:n0} of {wordsResult.MatchedCount:n0}");
        ComparisonWordSummary = wordResult is null
            ? "Enter a word to compare one form directly."
            : FormatCompareWord(wordResult);
        ComparisonWords = FormatCompareWords(wordsResult.Comparisons);
        ComparisonDifficulty = FormatCompareDifficulty(difficultyResult);
    }

    private static Task<CompareWordResult> CompareWordFromApplicationAsync(
        CompareWordRequest request,
        CancellationToken cancellationToken)
    {
        return Task.Run(async () =>
        {
            RunComparisonQueryService service = new();
            return await service.CompareWordAsync(request, cancellationToken).ConfigureAwait(false);
        }, cancellationToken);
    }

    private static Task<CompareWordsResult> CompareWordsFromApplicationAsync(
        CompareWordsRequest request,
        CancellationToken cancellationToken)
    {
        return Task.Run(async () =>
        {
            RunComparisonQueryService service = new();
            return await service.CompareWordsAsync(request, cancellationToken).ConfigureAwait(false);
        }, cancellationToken);
    }

    private static Task<CompareDifficultyResult> CompareDifficultyFromApplicationAsync(
        CompareDifficultyRequest request,
        CancellationToken cancellationToken)
    {
        return Task.Run(async () =>
        {
            RunComparisonQueryService service = new();
            return await service.CompareDifficultyAsync(request, cancellationToken).ConfigureAwait(false);
        }, cancellationToken);
    }

    private static string FormatCompareWord(CompareWordResult result)
    {
        WordComparisonItem item = result.Comparison;
        return string.Join(Environment.NewLine,
            $"Word: {item.Word}",
            $"Type: {(item.IsFunctionWord ? "function" : "content")}",
            $"Left count: {item.LeftCount:n0} ({DesktopTextFormatter.FormatDouble(item.LeftFrequencyPerMillion)}/M)",
            $"Right count: {item.RightCount:n0} ({DesktopTextFormatter.FormatDouble(item.RightFrequencyPerMillion)}/M)",
            $"Difference/M: {DesktopTextFormatter.FormatSignedDouble(item.DifferencePerMillion)}",
            $"Ratio left/right: {DesktopTextFormatter.FormatRatio(item.Ratio)}",
            $"Favours: {item.Direction}");
    }

    private static string FormatCompareWords(IReadOnlyList<WordComparisonItem> comparisons)
    {
        if (comparisons.Count == 0)
        {
            return "No word differences matched the selected filters.";
        }

        IEnumerable<string> lines = comparisons.Select((item, index) =>
            $"{index + 1,2}. {DesktopTextFormatter.TrimForColumn(item.Word, 16),-16} {(item.IsFunctionWord ? "function" : "content"),-8} L {item.LeftCount,6:n0} {DesktopTextFormatter.FormatDouble(item.LeftFrequencyPerMillion),7}/M  R {item.RightCount,6:n0} {DesktopTextFormatter.FormatDouble(item.RightFrequencyPerMillion),7}/M  Δ {DesktopTextFormatter.FormatSignedDouble(item.DifferencePerMillion),8}  {item.Direction}");
        return string.Join(Environment.NewLine, lines);
    }

    private static string FormatCompareDifficulty(CompareDifficultyResult result)
    {
        return string.Join(Environment.NewLine,
            $"Language profile: {(result.LanguageProfile.IsKnown ? $"{result.LanguageProfile.Name} ({result.LanguageProfile.Code})" : $"generic defaults ({result.LanguageProfile.Code})")}",
            $"Thresholds: >= {result.Thresholds.LongWordLength} / >= {result.Thresholds.VeryLongWordLength} chars",
            $"Left score: {DesktopTextFormatter.FormatDouble(result.LeftProfile.HeuristicScore)}",
            $"Right score: {DesktopTextFormatter.FormatDouble(result.RightProfile.HeuristicScore)}",
            $"Score difference: {DesktopTextFormatter.FormatSignedDouble(result.ScoreDifference)}",
            $"Relatively harder: {result.Direction}",
            $"Left avg sent/word: {DesktopTextFormatter.FormatDouble(result.LeftProfile.AverageWordsPerSentence)} / {DesktopTextFormatter.FormatDouble(result.LeftProfile.AverageCharactersPerWord)}",
            $"Right avg sent/word: {DesktopTextFormatter.FormatDouble(result.RightProfile.AverageWordsPerSentence)} / {DesktopTextFormatter.FormatDouble(result.RightProfile.AverageCharactersPerWord)}");
    }

    private static string RunLabel(StoredAnalysisRunSummary run)
    {
        return $"{run.CorpusName} / {run.BookTitle} ({run.Id})";
    }

    private static string WordFilterText(ComparisonWordFilter filter)
    {
        return filter switch
        {
            ComparisonWordFilter.ContentOnly => "content words only",
            ComparisonWordFilter.FunctionOnly => "function words only",
            _ => "all words"
        };
    }

    private static string PresenceFilterText(ComparisonPresenceFilter filter)
    {
        return filter switch
        {
            ComparisonPresenceFilter.SharedOnly => "shared words only",
            ComparisonPresenceFilter.ExclusiveOnly => "exclusive words only",
            _ => "all words"
        };
    }
}
