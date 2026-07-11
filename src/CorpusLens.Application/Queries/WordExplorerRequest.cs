namespace CorpusLens.Application.Queries;

public sealed record WordExplorerRequest(
    string DatabasePath,
    long AnalysisRunId,
    string Word,
    int RelatedWordLimit = 10,
    int ContextLimit = 10,
    int ContextWords = 8,
    int BookLimit = 10);
