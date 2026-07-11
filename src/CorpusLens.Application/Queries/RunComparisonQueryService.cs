using CorpusLens.Analysis.Language;
using CorpusLens.Domain.Storage;
using CorpusLens.Infrastructure.Storage;

namespace CorpusLens.Application.Queries;

public sealed class RunComparisonQueryService
{
    public async Task<CompareWordResult> CompareWordAsync(
        CompareWordRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.DatabasePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Word);

        SqliteCorpusStore store = new(request.DatabasePath);
        RunComparisonContext context = await ReadContextAsync(
            store,
            request.LeftRunId,
            request.RightRunId,
            cancellationToken).ConfigureAwait(false);

        StoredWordStatistic? leftWord = await store
            .GetWordStatisticAsync(request.LeftRunId, request.Word, cancellationToken)
            .ConfigureAwait(false);
        StoredWordStatistic? rightWord = await store
            .GetWordStatisticAsync(request.RightRunId, request.Word, cancellationToken)
            .ConfigureAwait(false);

        return new CompareWordResult(
            context,
            CreateWordComparison(request.Word.Trim(), leftWord, rightWord));
    }

    public async Task<CompareWordsResult> CompareWordsAsync(
        CompareWordsRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.DatabasePath);

        int requestedLimit = Math.Clamp(request.Limit, 1, 1_000);
        int minCount = Math.Max(1, request.MinCount);
        int fetchLimit = Math.Min(Math.Max(requestedLimit * 100, 2_000), 20_000);
        StoredWordFilter storedFilter = ToStoredWordFilter(request.WordFilter);

        SqliteCorpusStore store = new(request.DatabasePath);
        RunComparisonContext context = await ReadContextAsync(
            store,
            request.LeftRunId,
            request.RightRunId,
            cancellationToken).ConfigureAwait(false);

        IReadOnlyList<StoredWordStatistic> leftWords = await store
            .ListTopWordsAsync(request.LeftRunId, fetchLimit, storedFilter, cancellationToken)
            .ConfigureAwait(false);
        IReadOnlyList<StoredWordStatistic> rightWords = await store
            .ListTopWordsAsync(request.RightRunId, fetchLimit, storedFilter, cancellationToken)
            .ConfigureAwait(false);

        IReadOnlyList<WordComparisonItem> matchedComparisons = BuildWordComparisons(leftWords, rightWords)
            .Where(item => item.LeftCount >= minCount || item.RightCount >= minCount)
            .Where(item => MatchesPresenceFilter(item, request.PresenceFilter))
            .OrderByDescending(item => item.AbsoluteDifferencePerMillion)
            .ThenByDescending(item => item.TotalCount)
            .ThenBy(item => item.Word, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        IReadOnlyList<WordComparisonItem> comparisons = matchedComparisons
            .Take(requestedLimit)
            .ToArray();

        return new CompareWordsResult(
            context,
            request.WordFilter,
            request.PresenceFilter,
            minCount,
            fetchLimit,
            matchedComparisons.Count,
            comparisons);
    }

    public async Task<CompareDifficultyResult> CompareDifficultyAsync(
        CompareDifficultyRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.DatabasePath);

        SqliteCorpusStore store = new(request.DatabasePath);
        RunComparisonContext context = await ReadContextAsync(
            store,
            request.LeftRunId,
            request.RightRunId,
            cancellationToken).ConfigureAwait(false);

        LanguageProfile languageProfile = SelectComparisonDifficultyProfile(
            context.LeftLanguageCode,
            context.RightLanguageCode);
        DifficultyThresholds thresholds = ReadDifficultyThresholds(request, languageProfile);

        StoredDifficultyProfile? leftProfile = await store
            .GetDifficultyProfileAsync(request.LeftRunId, thresholds.LongWordLength, thresholds.VeryLongWordLength, cancellationToken)
            .ConfigureAwait(false);
        StoredDifficultyProfile? rightProfile = await store
            .GetDifficultyProfileAsync(request.RightRunId, thresholds.LongWordLength, thresholds.VeryLongWordLength, cancellationToken)
            .ConfigureAwait(false);

        if (leftProfile is null || rightProfile is null)
        {
            throw new InvalidOperationException("Unable to build one or both difficulty profiles.");
        }

        return new CompareDifficultyResult(
            context,
            languageProfile,
            thresholds,
            leftProfile,
            rightProfile);
    }

    private static async Task<RunComparisonContext> ReadContextAsync(
        SqliteCorpusStore store,
        long leftRunId,
        long rightRunId,
        CancellationToken cancellationToken)
    {
        StoredAnalysisRunSummary? leftRun = await store
            .GetAnalysisRunSummaryAsync(leftRunId, cancellationToken)
            .ConfigureAwait(false);
        StoredAnalysisRunSummary? rightRun = await store
            .GetAnalysisRunSummaryAsync(rightRunId, cancellationToken)
            .ConfigureAwait(false);

        if (leftRun is null)
        {
            throw new InvalidOperationException($"Analysis run {leftRunId} was not found.");
        }

        if (rightRun is null)
        {
            throw new InvalidOperationException($"Analysis run {rightRunId} was not found.");
        }

        string? leftLanguageCode = await ReadRunLanguageCodeAsync(store, leftRunId, cancellationToken)
            .ConfigureAwait(false);
        string? rightLanguageCode = await ReadRunLanguageCodeAsync(store, rightRunId, cancellationToken)
            .ConfigureAwait(false);

        return new RunComparisonContext(leftRun, rightRun, leftLanguageCode, rightLanguageCode);
    }

    private static async Task<string?> ReadRunLanguageCodeAsync(
        SqliteCorpusStore store,
        long analysisRunId,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<StoredAnalysisRunBook> sourceBooks = await store
            .ListAnalysisRunBooksAsync(analysisRunId, cancellationToken)
            .ConfigureAwait(false);

        string[] languageCodes = sourceBooks
            .Select(book => book.LanguageCode)
            .Where(code => !string.IsNullOrWhiteSpace(code))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return languageCodes.Length == 1 ? languageCodes[0] : null;
    }

    private static IReadOnlyList<WordComparisonItem> BuildWordComparisons(
        IReadOnlyList<StoredWordStatistic> leftWords,
        IReadOnlyList<StoredWordStatistic> rightWords)
    {
        Dictionary<string, StoredWordStatistic> leftByWord = leftWords.ToDictionary(
            word => word.Word,
            StringComparer.OrdinalIgnoreCase);
        Dictionary<string, StoredWordStatistic> rightByWord = rightWords.ToDictionary(
            word => word.Word,
            StringComparer.OrdinalIgnoreCase);

        string[] words = leftByWord.Keys
            .Concat(rightByWord.Keys)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return words
            .Select(word => CreateWordComparison(
                word,
                leftByWord.TryGetValue(word, out StoredWordStatistic? leftWord) ? leftWord : null,
                rightByWord.TryGetValue(word, out StoredWordStatistic? rightWord) ? rightWord : null))
            .ToArray();
    }

    private static WordComparisonItem CreateWordComparison(
        string requestedWord,
        StoredWordStatistic? leftWord,
        StoredWordStatistic? rightWord)
    {
        string word = leftWord?.Word ?? rightWord?.Word ?? requestedWord;
        return new WordComparisonItem(
            word,
            leftWord?.Count ?? 0,
            leftWord?.DocumentCount ?? 0,
            leftWord?.FrequencyPerMillion ?? 0,
            rightWord?.Count ?? 0,
            rightWord?.DocumentCount ?? 0,
            rightWord?.FrequencyPerMillion ?? 0,
            (leftWord?.IsStopWord ?? false) || (rightWord?.IsStopWord ?? false));
    }

    private static bool MatchesPresenceFilter(WordComparisonItem comparison, ComparisonPresenceFilter filter)
    {
        return filter switch
        {
            ComparisonPresenceFilter.SharedOnly => comparison.LeftCount > 0 && comparison.RightCount > 0,
            ComparisonPresenceFilter.ExclusiveOnly => comparison.LeftCount == 0 || comparison.RightCount == 0,
            _ => true
        };
    }

    private static StoredWordFilter ToStoredWordFilter(ComparisonWordFilter filter)
    {
        return filter switch
        {
            ComparisonWordFilter.ContentOnly => StoredWordFilter.ContentOnly,
            ComparisonWordFilter.FunctionOnly => StoredWordFilter.FunctionOnly,
            _ => StoredWordFilter.All
        };
    }

    private static LanguageProfile SelectComparisonDifficultyProfile(
        string? leftLanguageCode,
        string? rightLanguageCode)
    {
        if (!string.IsNullOrWhiteSpace(leftLanguageCode)
            && string.Equals(leftLanguageCode, rightLanguageCode, StringComparison.OrdinalIgnoreCase))
        {
            return LanguageProfileProvider.GetProfile(leftLanguageCode);
        }

        return LanguageProfileProvider.GetProfile("unknown");
    }

    private static DifficultyThresholds ReadDifficultyThresholds(
        CompareDifficultyRequest request,
        LanguageProfile languageProfile)
    {
        int longWordLength = Math.Max(
            2,
            request.LongWordLength ?? languageProfile.DefaultLongWordLength);
        int veryLongWordLength = Math.Max(
            longWordLength,
            request.VeryLongWordLength ?? languageProfile.DefaultVeryLongWordLength);

        return new DifficultyThresholds(longWordLength, veryLongWordLength);
    }
}
