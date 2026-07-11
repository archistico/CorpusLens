namespace CorpusLens.Application.Queries;

public sealed record CompareWordsResult(
    RunComparisonContext Context,
    ComparisonWordFilter WordFilter,
    ComparisonPresenceFilter PresenceFilter,
    int MinCount,
    int FetchLimit,
    int MatchedCount,
    IReadOnlyList<WordComparisonItem> Comparisons);
