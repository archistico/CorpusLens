namespace CorpusLens.Domain.Analysis;

public sealed record WordFrequency(
    string Word,
    int Count,
    int DocumentCount,
    double FrequencyPerMillion);
