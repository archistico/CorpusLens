using CorpusLens.Domain.Analysis;

namespace CorpusLens.Domain.Storage;

public sealed record StoredSentenceCategoryStatistic(
    long Id,
    long AnalysisRunId,
    long CorpusId,
    long BookId,
    PhraseCategory Category,
    int Count,
    double Percentage);
