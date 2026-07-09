namespace CorpusLens.Domain.Storage;

public sealed record StoredCollocationStatistic(
    long AnalysisRunId,
    string Word,
    string Collocate,
    int Count,
    int LeftCount,
    int RightCount,
    double RatePerTarget,
    double AverageDistance);
