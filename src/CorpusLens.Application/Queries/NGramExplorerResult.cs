namespace CorpusLens.Application.Queries;

public sealed record NGramExplorerResult(
    int? N,
    int MinCount,
    int Limit,
    string SearchTerm,
    NGramExplorerFilter Filter,
    NGramExplorerSort Sort,
    int FetchedCount,
    int MatchedCount,
    IReadOnlyList<NGramExplorerItem> NGrams);
