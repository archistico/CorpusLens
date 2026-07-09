namespace CorpusLens.Domain.Analysis;

public sealed record NextWordFrequency(
    string Word,
    string NextWord,
    int Count,
    double Probability);
