namespace CorpusLens.Domain.Storage;

public sealed record StoredPhraseStatistic(
    long AnalysisRunId,
    int N,
    string Phrase,
    int Count,
    int ChapterCount,
    double FrequencyPerMillion);
