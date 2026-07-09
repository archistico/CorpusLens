namespace CorpusLens.Domain.Storage;

public sealed record StoredWordBookStatistic(
    long AnalysisRunId,
    long BookId,
    int OrderIndex,
    string Title,
    string Author,
    int ChapterCount,
    int CharacterCount,
    int WordTokenCount,
    int Count,
    double FrequencyPerMillion);
