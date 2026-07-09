namespace CorpusLens.Domain.Storage;

public sealed record StoredAnalysisRunSummary(
    long Id,
    long CorpusId,
    string CorpusName,
    long BookId,
    string BookTitle,
    DateTimeOffset StartedAt,
    DateTimeOffset CompletedAt,
    string Status,
    int SentenceCount,
    int TokenCount,
    int WordTokenCount,
    int DistinctWordCount,
    double AverageWordsPerSentence,
    double AverageCharactersPerWord,
    string ReportPath);
