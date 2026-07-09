namespace CorpusLens.Domain.Analysis;

public sealed record AnalysisSettings
{
    public int MinWordLength { get; init; } = 1;

    public bool LowercaseWords { get; init; } = true;

    public bool IncludePunctuationTokens { get; init; }

    public int NGramMinN { get; init; } = 2;

    public int NGramMaxN { get; init; } = 5;

    public int MinNGramCount { get; init; } = 2;

    public int TopWordsForNextWordAnalysis { get; init; } = 1000;

    public int MinNextWordPairCount { get; init; } = 2;
}
