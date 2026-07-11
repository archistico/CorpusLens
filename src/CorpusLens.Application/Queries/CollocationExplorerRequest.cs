namespace CorpusLens.Application.Queries;

public sealed record CollocationExplorerRequest(
    string DatabasePath,
    long AnalysisRunId,
    string Word,
    int Window = 4,
    int Limit = 30,
    int MinCount = 1,
    double MinDice = 0.0,
    CollocationExplorerFilter Filter = CollocationExplorerFilter.All);
