namespace CorpusLens.Domain.Storage;

public sealed record StoredNGramStatistic(
    long Id,
    long AnalysisRunId,
    long CorpusId,
    long BookId,
    int N,
    string Text,
    int Count,
    int DocumentCount,
    double FrequencyPerMillion);
