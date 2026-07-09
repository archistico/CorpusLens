namespace CorpusLens.Domain.Analysis;

public sealed record NGramFrequency(
    int N,
    string Text,
    int Count,
    int DocumentCount,
    double FrequencyPerMillion);
