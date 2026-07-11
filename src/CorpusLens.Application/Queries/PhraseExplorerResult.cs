namespace CorpusLens.Application.Queries;

public sealed record PhraseExplorerResult(
    int MinN,
    int MaxN,
    int MinCount,
    int MinChapters,
    int Limit,
    bool ContentBoundaryOnly,
    bool LongestOnly,
    int FetchedCount,
    int MatchedCount,
    IReadOnlyList<PhraseExplorerItem> Phrases);
