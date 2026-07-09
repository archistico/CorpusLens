namespace CorpusLens.Domain.Storage;

public sealed record StoredAnalysisRun(
    long Id,
    long CorpusId,
    long BookId,
    DateTimeOffset StartedAt,
    DateTimeOffset CompletedAt,
    string Status,
    string EngineVersion,
    string SettingsJson,
    int SentenceCount,
    int TokenCount,
    int WordTokenCount,
    int DistinctWordCount,
    double AverageWordsPerSentence,
    double AverageCharactersPerWord,
    string ReportPath,
    string WordsCsvPath,
    string NGramsCsvPath,
    string NextWordsCsvPath,
    string ExtractedTextPath,
    string ErrorMessage);
