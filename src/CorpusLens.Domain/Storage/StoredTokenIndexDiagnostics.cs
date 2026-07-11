namespace CorpusLens.Domain.Storage;

public sealed record StoredTokenIndexDiagnostics(
    long AnalysisRunId,
    int ExpectedWordTokenCount,
    int SourceBookCount,
    int ExpectedChapterCount,
    int IndexedTokenCount,
    int IndexedWordTokenCount,
    int DistinctTokenCount,
    int StopWordTokenCount,
    int ContentTokenCount,
    int IndexedBookCount,
    int IndexedSourceBookCount,
    int IndexedChapterCount,
    int? FirstRunPosition,
    int? LastRunPosition,
    int RunPositionGapCount)
{
    public bool IsIndexed => IndexedTokenCount > 0;

    public int WordTokenDelta => IndexedWordTokenCount - ExpectedWordTokenCount;

    public double WordTokenCoveragePercentage => ExpectedWordTokenCount == 0
        ? 0
        : IndexedWordTokenCount * 100.0 / ExpectedWordTokenCount;

    public bool CanUseTokenIndexForContextQueries => IndexedTokenCount > 0;

    public bool CanUseTokenIndexForWordBookDistribution => IndexedSourceBookCount > 0;
}
