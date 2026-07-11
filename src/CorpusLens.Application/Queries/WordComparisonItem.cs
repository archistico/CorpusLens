namespace CorpusLens.Application.Queries;

public sealed record WordComparisonItem(
    string Word,
    int LeftCount,
    int LeftDocumentCount,
    double LeftFrequencyPerMillion,
    int RightCount,
    int RightDocumentCount,
    double RightFrequencyPerMillion,
    bool IsFunctionWord)
{
    public int TotalCount => LeftCount + RightCount;

    public double CombinedCount => TotalCount;

    public double LeftShare => CombinedCount == 0 ? 0 : LeftCount / CombinedCount;

    public double RightShare => CombinedCount == 0 ? 0 : RightCount / CombinedCount;

    public double DifferencePerMillion => LeftFrequencyPerMillion - RightFrequencyPerMillion;

    public double AbsoluteDifferencePerMillion => Math.Abs(DifferencePerMillion);

    public double Ratio => RightFrequencyPerMillion == 0
        ? (LeftFrequencyPerMillion == 0 ? 0 : double.PositiveInfinity)
        : LeftFrequencyPerMillion / RightFrequencyPerMillion;

    public string Direction
    {
        get
        {
            const double epsilon = 0.000001;
            if (Math.Abs(DifferencePerMillion) < epsilon)
            {
                return "tie";
            }

            return DifferencePerMillion > 0 ? "left" : "right";
        }
    }
}
