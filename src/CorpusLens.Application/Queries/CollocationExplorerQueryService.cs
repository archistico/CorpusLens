using CorpusLens.Analysis.StopWords;
using CorpusLens.Domain.Storage;
using CorpusLens.Infrastructure.Storage;

namespace CorpusLens.Application.Queries;

public sealed class CollocationExplorerQueryService
{
    public async Task<CollocationExplorerResult> GetCollocationsAsync(
        CollocationExplorerRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.DatabasePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Word);

        int window = Math.Clamp(request.Window, 1, 10);
        int limit = Math.Clamp(request.Limit, 1, 100);
        int minCount = Math.Max(1, request.MinCount);
        double minDice = Math.Clamp(request.MinDice, 0.0, 1.0);
        int fetchLimit = Math.Min(Math.Max(limit * 20, 200), 1_000);

        SqliteCorpusStore store = new(request.DatabasePath);
        IReadOnlyList<StoredAnalysisRunBook> sourceBooks = await store
            .ListAnalysisRunBooksAsync(request.AnalysisRunId, cancellationToken)
            .ConfigureAwait(false);
        IReadOnlyList<string> languageCodes = CorpusProfileQueryService.ReadLanguageCodes(sourceBooks);

        IReadOnlyList<StoredCollocationStatistic> allCollocations = await store
            .ListCollocationsAsync(request.AnalysisRunId, request.Word, window, fetchLimit, cancellationToken)
            .ConfigureAwait(false);

        CollocationExplorerItem[] matched = allCollocations
            .Where(item => item.Count >= minCount && item.DiceCoefficient >= minDice)
            .Select(item => ToItem(item, languageCodes))
            .Where(item => MatchesFilter(item, request.Filter))
            .ToArray();

        CollocationExplorerItem[] shown = matched
            .Take(limit)
            .ToArray();

        return new CollocationExplorerResult(
            request.Word.Trim(),
            window,
            limit,
            minCount,
            minDice,
            request.Filter,
            matched.Length,
            shown);
    }

    private static CollocationExplorerItem ToItem(
        StoredCollocationStatistic collocation,
        IReadOnlyList<string> languageCodes)
    {
        bool isFunction = StopWordProvider.IsStopWord(collocation.Collocate, languageCodes);
        return new CollocationExplorerItem(
            collocation.Collocate,
            isFunction ? "function" : "content",
            collocation.Count,
            collocation.LeftCount,
            collocation.RightCount,
            collocation.RatePerTarget,
            collocation.AverageDistance,
            collocation.DiceCoefficient);
    }

    private static bool MatchesFilter(CollocationExplorerItem item, CollocationExplorerFilter filter)
    {
        return filter switch
        {
            CollocationExplorerFilter.ContentOnly => item.WordType == "content",
            CollocationExplorerFilter.FunctionOnly => item.WordType == "function",
            _ => true
        };
    }
}
