using CorpusLens.Analysis.Language;
using CorpusLens.Domain.Storage;

namespace CorpusLens.Application.Queries;

public sealed record CompareDifficultyResult(
    RunComparisonContext Context,
    LanguageProfile LanguageProfile,
    DifficultyThresholds Thresholds,
    StoredDifficultyProfile LeftProfile,
    StoredDifficultyProfile RightProfile)
{
    public double ScoreDifference => LeftProfile.HeuristicScore - RightProfile.HeuristicScore;

    public string Direction
    {
        get
        {
            const double epsilon = 0.000001;
            if (Math.Abs(ScoreDifference) < epsilon)
            {
                return "tie";
            }

            return ScoreDifference > 0 ? "left" : "right";
        }
    }
}
