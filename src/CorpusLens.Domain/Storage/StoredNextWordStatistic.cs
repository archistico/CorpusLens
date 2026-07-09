namespace CorpusLens.Domain.Storage;

public sealed record StoredNextWordStatistic(
    long Id,
    long AnalysisRunId,
    long CorpusId,
    long BookId,
    string Word,
    string NextWord,
    int Count,
    double Probability);
