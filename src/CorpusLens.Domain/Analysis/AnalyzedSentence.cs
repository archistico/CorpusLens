namespace CorpusLens.Domain.Analysis;

public sealed record AnalyzedSentence(
    string Text,
    string NormalizedText,
    PhraseCategory Category,
    int TokenCount);
