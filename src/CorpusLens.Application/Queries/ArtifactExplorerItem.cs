namespace CorpusLens.Application.Queries;

public sealed record ArtifactExplorerItem(
    string Id,
    string DisplayName,
    string ExpectedFileName,
    string StoredPath,
    string ResolvedPath,
    ArtifactAvailability Availability,
    bool IsOptional,
    bool IsPathRecorded)
{
    public bool CanOpen => Availability == ArtifactAvailability.Available;

    public string AvailabilityLabel => Availability switch
    {
        ArtifactAvailability.Available => "Available",
        ArtifactAvailability.Missing => "Missing",
        _ => "Not generated",
    };
}
