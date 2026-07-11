namespace CorpusLens.Application.Queries;

public sealed record CollocationExplorerItem(
    string Collocate,
    string WordType,
    int Count,
    int LeftCount,
    int RightCount,
    double RatePerTarget,
    double AverageDistance,
    double DiceCoefficient);
