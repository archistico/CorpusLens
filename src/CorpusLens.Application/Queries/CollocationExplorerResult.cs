namespace CorpusLens.Application.Queries;

public sealed record CollocationExplorerResult(
    string Word,
    int Window,
    int Limit,
    int MinCount,
    double MinDice,
    CollocationExplorerFilter Filter,
    int MatchedCount,
    IReadOnlyList<CollocationExplorerItem> Collocations);
