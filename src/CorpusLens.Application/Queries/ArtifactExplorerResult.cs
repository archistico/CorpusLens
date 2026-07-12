namespace CorpusLens.Application.Queries;

public sealed record ArtifactExplorerResult(
    long AnalysisRunId,
    string OutputDirectory,
    bool OutputDirectoryExists,
    IReadOnlyList<ArtifactExplorerItem> Artifacts)
{
    public int AvailableCount => Artifacts.Count(artifact => artifact.Availability == ArtifactAvailability.Available);

    public int MissingCount => Artifacts.Count(artifact => artifact.Availability == ArtifactAvailability.Missing);

    public int NotGeneratedCount => Artifacts.Count(artifact => artifact.Availability == ArtifactAvailability.NotGenerated);
}
