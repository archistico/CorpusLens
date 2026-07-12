namespace CorpusLens.Application.Queries;

public sealed record NGramExplorerRequest(
    string DatabasePath,
    long AnalysisRunId,
    int? N,
    int MinCount,
    int Limit,
    string? SearchTerm,
    NGramExplorerFilter Filter,
    NGramExplorerSort Sort);
