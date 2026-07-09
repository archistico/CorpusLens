namespace CorpusLens.Domain.Storage;

public sealed record StoredWordStatistic(
    long Id,
    long AnalysisRunId,
    long CorpusId,
    long BookId,
    string Word,
    int Count,
    int DocumentCount,
    double FrequencyPerMillion,
    bool IsStopWord);
