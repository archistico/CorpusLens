using CorpusLens.Analysis.StopWords;
using CorpusLens.Domain.Storage;
using CorpusLens.Infrastructure.Storage;

namespace CorpusLens.Application.Queries;

public sealed class NGramExplorerQueryService
{
    public async Task<NGramExplorerResult> GetNGramsAsync(
        NGramExplorerRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.DatabasePath);

        int? n = request.N is null ? null : Math.Clamp(request.N.Value, 2, 8);
        int minCount = Math.Max(1, request.MinCount);
        int limit = Math.Clamp(request.Limit, 1, 100);
        string searchTerm = NormalizeSearchTerm(request.SearchTerm);
        int fetchLimit = Math.Min(Math.Max(limit * 20, 300), 1_000);

        SqliteCorpusStore store = new(request.DatabasePath);
        IReadOnlyList<StoredAnalysisRunBook> sourceBooks = await store
            .ListAnalysisRunBooksAsync(request.AnalysisRunId, cancellationToken)
            .ConfigureAwait(false);
        IReadOnlyList<string> languageCodes = CorpusProfileQueryService.ReadLanguageCodes(sourceBooks);

        IReadOnlyList<StoredNGramStatistic> stored = await store
            .ListNGramsAsync(
                request.AnalysisRunId,
                n,
                minCount,
                searchTerm,
                MapSort(request.Sort),
                fetchLimit,
                cancellationToken)
            .ConfigureAwait(false);

        NGramExplorerItem[] matched = stored
            .Select(item => ToItem(item, languageCodes))
            .Where(item => MatchesFilter(item, request.Filter))
            .ToArray();

        NGramExplorerItem[] shown = matched
            .Take(limit)
            .ToArray();

        return new NGramExplorerResult(
            n,
            minCount,
            limit,
            searchTerm,
            request.Filter,
            request.Sort,
            stored.Count,
            matched.Length,
            shown);
    }

    private static NGramExplorerItem ToItem(
        StoredNGramStatistic statistic,
        IReadOnlyList<string> languageCodes)
    {
        string[] words = SplitWords(statistic.Text);
        bool[] functionFlags = words
            .Select(word => StopWordProvider.IsStopWord(word, languageCodes))
            .ToArray();

        string pattern = functionFlags.Length == 0
            ? "empty"
            : string.Join('-', functionFlags.Select(isFunction => isFunction ? "F" : "C"));

        return new NGramExplorerItem(
            statistic.Text,
            statistic.N,
            statistic.Count,
            statistic.DocumentCount,
            statistic.FrequencyPerMillion,
            pattern);
    }

    private static bool MatchesFilter(NGramExplorerItem item, NGramExplorerFilter filter)
    {
        string[] markers = item.WordPattern.Split('-', StringSplitOptions.RemoveEmptyEntries);
        return filter switch
        {
            NGramExplorerFilter.ContentOnly => markers.Length > 0 && markers.All(marker => marker == "C"),
            NGramExplorerFilter.FunctionOnly => markers.Length > 0 && markers.All(marker => marker == "F"),
            NGramExplorerFilter.ContentBoundary => markers.Length > 0
                && markers[0] == "C"
                && markers[^1] == "C",
            _ => true,
        };
    }

    private static StoredNGramSort MapSort(NGramExplorerSort sort)
    {
        return sort switch
        {
            NGramExplorerSort.FrequencyPerMillion => StoredNGramSort.FrequencyPerMillion,
            NGramExplorerSort.DocumentCount => StoredNGramSort.DocumentCount,
            NGramExplorerSort.Text => StoredNGramSort.Text,
            _ => StoredNGramSort.Count,
        };
    }

    private static string NormalizeSearchTerm(string? searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return string.Empty;
        }

        return string.Join(' ', searchTerm
            .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .ToLowerInvariant();
    }

    private static string[] SplitWords(string text)
    {
        return text.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }
}
