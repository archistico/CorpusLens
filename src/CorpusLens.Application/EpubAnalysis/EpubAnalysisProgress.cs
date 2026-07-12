namespace CorpusLens.Application.EpubAnalysis;

public sealed record EpubAnalysisProgress(
    EpubAnalysisStage Stage,
    int Percent,
    string Message,
    int CompletedFiles = 0,
    int TotalFiles = 0,
    int ImportedFiles = 0,
    int FailedFiles = 0)
{
    public int NormalizedPercent => Math.Clamp(Percent, 0, 100);
}
