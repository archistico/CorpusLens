using CorpusLens.Application.Queries;

namespace CorpusLens.Desktop.ViewModels;

public sealed class ArtifactListItemViewModel
{
    public ArtifactListItemViewModel(ArtifactExplorerItem artifact)
    {
        Artifact = artifact;
    }

    public ArtifactExplorerItem Artifact { get; }

    public string DisplayTitle => Artifact.DisplayName;

    public string DisplaySubtitle => $"{Artifact.AvailabilityLabel} · {Artifact.ExpectedFileName}";

    public override string ToString()
    {
        return $"[{Artifact.AvailabilityLabel}] {Artifact.DisplayName} — {Artifact.ExpectedFileName}";
    }
}
