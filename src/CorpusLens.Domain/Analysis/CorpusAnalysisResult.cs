namespace CorpusLens.Domain.Analysis;

public sealed record CorpusAnalysisResult(
    CorpusSummary Summary,
    IReadOnlyList<WordFrequency> Words,
    IReadOnlyList<NGramFrequency> NGrams,
    IReadOnlyList<NextWordFrequency> NextWords,
    IReadOnlyList<AnalyzedSentence> Sentences);
