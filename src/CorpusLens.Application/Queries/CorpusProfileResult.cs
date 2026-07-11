using CorpusLens.Analysis.Language;
using CorpusLens.Domain.Storage;

namespace CorpusLens.Application.Queries;

public sealed record CorpusProfileResult(
    StoredAnalysisRunSummary Run,
    IReadOnlyList<StoredAnalysisRunBook> SourceBooks,
    IReadOnlyList<string> LanguageCodes,
    LanguageProfile LanguageProfile,
    DifficultyThresholds DifficultyThresholds,
    StoredDifficultyProfile? DifficultyProfile,
    IReadOnlyList<StoredWordStatistic> ContentWords,
    IReadOnlyList<StoredWordStatistic> FunctionWords,
    IReadOnlyList<StoredPhraseStatistic> Phrases)
{
    public int SourceBookCount => SourceBooks.Count;

    public int ChapterCount => SourceBooks.Count == 0
        ? 0
        : SourceBooks.Sum(book => book.ChapterCount);
}
