namespace CorpusLens.Domain.Analysis;

public sealed record CorpusSummary(
    int DocumentCount,
    int SentenceCount,
    int TokenCount,
    int WordTokenCount,
    int DistinctWordCount,
    double AverageWordsPerSentence,
    double AverageCharactersPerWord);
