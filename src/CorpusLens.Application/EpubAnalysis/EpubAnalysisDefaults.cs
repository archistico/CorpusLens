using CorpusLens.Domain.Analysis;

namespace CorpusLens.Application.EpubAnalysis;

public static class EpubAnalysisDefaults
{
    public const string SearchPattern = "*.epub";

    public static AnalysisSettings CreateSettings()
    {
        return new AnalysisSettings
        {
            NGramMinN = 2,
            NGramMaxN = 5,
            MinNGramCount = 2,
            TopWordsForNextWordAnalysis = 1000,
            MinNextWordPairCount = 2,
        };
    }
}
