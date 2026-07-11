using CorpusLens.Domain.Storage;

namespace CorpusLens.Application.Queries;

public sealed record TokenIndexHealthResult(
    StoredAnalysisRunSummary Run,
    StoredTokenIndexDiagnostics? Diagnostics,
    IReadOnlyList<string> LanguageCodes,
    string DatabasePath,
    long DatabaseSizeBytes,
    IReadOnlyList<string> Warnings)
{
    public bool RunCompleted => string.Equals(Run.Status, "Completed", StringComparison.OrdinalIgnoreCase);

    public bool IsIndexed => Diagnostics is not null && Diagnostics.IsIndexed;

    public bool TokenCoverageOk => Diagnostics is not null && Diagnostics.WordTokenDelta == 0;

    public bool ChapterCoverageOk => Diagnostics is not null && Diagnostics.IndexedChapterCount == Diagnostics.ExpectedChapterCount;

    public bool SourceBookCoverageOk => Diagnostics is not null
        && (Diagnostics.SourceBookCount == 0
            ? Diagnostics.IndexedBookCount > 0
            : Diagnostics.IndexedSourceBookCount == Diagnostics.SourceBookCount);

    public bool RunPositionsOk => Diagnostics is not null && Diagnostics.RunPositionGapCount == 0;

    public bool OverallOk => Warnings.Count == 0;
}
