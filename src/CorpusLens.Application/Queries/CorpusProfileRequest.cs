namespace CorpusLens.Application.Queries;

public sealed record CorpusProfileRequest(
    string DatabasePath,
    long AnalysisRunId,
    int WordLimit = 10,
    int PhraseLimit = 10,
    int MinPhraseCount = 3,
    int MinPhraseChapters = 2,
    int? LongWordLength = null,
    int? VeryLongWordLength = null);
