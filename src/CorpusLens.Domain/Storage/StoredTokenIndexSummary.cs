namespace CorpusLens.Domain.Storage;

public sealed record StoredTokenIndexSummary(
    long AnalysisRunId,
    int TokenCount,
    int WordTokenCount,
    int DistinctTokenCount,
    int StopWordTokenCount,
    int ContentTokenCount,
    int ChapterCount);
