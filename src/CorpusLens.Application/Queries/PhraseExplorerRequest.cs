namespace CorpusLens.Application.Queries;

public sealed record PhraseExplorerRequest(
    string DatabasePath,
    long AnalysisRunId,
    int MinN,
    int MaxN,
    int MinCount,
    int MinChapters,
    int Limit,
    bool ContentBoundaryOnly,
    bool LongestOnly);
