using CorpusLens.Domain.Storage;

namespace CorpusLens.Application.Queries;

public sealed record RunComparisonContext(
    StoredAnalysisRunSummary LeftRun,
    StoredAnalysisRunSummary RightRun,
    string? LeftLanguageCode,
    string? RightLanguageCode)
{
    public bool HasDifferentLanguages => !string.IsNullOrWhiteSpace(LeftLanguageCode)
        && !string.IsNullOrWhiteSpace(RightLanguageCode)
        && !string.Equals(LeftLanguageCode, RightLanguageCode, StringComparison.OrdinalIgnoreCase);
}
