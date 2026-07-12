namespace CorpusLens.Application.Queries;

public sealed record ArtifactExplorerRequest(
    string DatabasePath,
    long AnalysisRunId);
