using System.Collections.ObjectModel;
using CorpusLens.Application.Queries;
using CorpusLens.Desktop.Services;

namespace CorpusLens.Desktop.ViewModels;

public sealed class ArtifactExplorerViewModel : ViewModelBase
{
    private readonly Func<ArtifactExplorerRequest, CancellationToken, Task<ArtifactExplorerResult>> _artifactLoader;
    private readonly Func<string, bool, CancellationToken, Task> _pathLauncher;

    private string _artifactExplorerTitle = "Reports and exports";
    private string _artifactExplorerSummary = "Select a run to inspect its generated artifacts.";
    private string _artifactDetails = "Artifact details will appear here.";
    private string _outputDirectorySummary = "Output folder: unavailable";
    private string _outputDirectory = string.Empty;
    private bool _outputDirectoryExists;
    private ArtifactListItemViewModel? _selectedArtifact;

    public ArtifactExplorerViewModel(
        Func<ArtifactExplorerRequest, CancellationToken, Task<ArtifactExplorerResult>>? artifactLoader = null,
        Func<string, bool, CancellationToken, Task>? pathLauncher = null)
    {
        _artifactLoader = artifactLoader ?? LoadArtifactsFromApplicationAsync;
        _pathLauncher = pathLauncher ?? SystemPathLauncher.OpenAsync;
    }

    public ObservableCollection<ArtifactListItemViewModel> Artifacts { get; } = new();

    public string ArtifactExplorerTitle
    {
        get => _artifactExplorerTitle;
        private set => SetProperty(ref _artifactExplorerTitle, value);
    }

    public string ArtifactExplorerSummary
    {
        get => _artifactExplorerSummary;
        private set => SetProperty(ref _artifactExplorerSummary, value);
    }

    public string ArtifactDetails
    {
        get => _artifactDetails;
        private set => SetProperty(ref _artifactDetails, value);
    }

    public string OutputDirectorySummary
    {
        get => _outputDirectorySummary;
        private set => SetProperty(ref _outputDirectorySummary, value);
    }

    public string OutputDirectory
    {
        get => _outputDirectory;
        private set => SetProperty(ref _outputDirectory, value);
    }

    public bool OutputDirectoryExists
    {
        get => _outputDirectoryExists;
        private set
        {
            if (SetProperty(ref _outputDirectoryExists, value))
            {
                OnPropertyChanged(nameof(CanOpenOutputDirectory));
            }
        }
    }

    public ArtifactListItemViewModel? SelectedArtifact
    {
        get => _selectedArtifact;
        private set
        {
            if (SetProperty(ref _selectedArtifact, value))
            {
                ArtifactDetails = FormatArtifactDetails(value?.Artifact);
                OnPropertyChanged(nameof(CanOpenSelectedArtifact));
            }
        }
    }

    public bool CanOpenSelectedArtifact => SelectedArtifact?.Artifact.CanOpen == true;

    public bool CanOpenOutputDirectory => OutputDirectoryExists && !string.IsNullOrWhiteSpace(OutputDirectory);

    public async Task<string> LoadAsync(
        string databasePath,
        long analysisRunId,
        CancellationToken cancellationToken = default)
    {
        ArtifactExplorerResult result = await _artifactLoader(
                new ArtifactExplorerRequest(databasePath, analysisRunId),
                cancellationToken)
            .ConfigureAwait(true);
        cancellationToken.ThrowIfCancellationRequested();

        Artifacts.Clear();
        foreach (ArtifactExplorerItem artifact in result.Artifacts)
        {
            Artifacts.Add(new ArtifactListItemViewModel(artifact));
        }

        OutputDirectory = result.OutputDirectory;
        OutputDirectoryExists = result.OutputDirectoryExists;
        ArtifactExplorerTitle = $"Reports and exports — run {result.AnalysisRunId}";
        ArtifactExplorerSummary = string.Join(Environment.NewLine,
            $"Available: {result.AvailableCount:n0}",
            $"Missing recorded files: {result.MissingCount:n0}",
            $"Not generated or not recorded: {result.NotGeneratedCount:n0}",
            "CorpusLens never modifies these files; opening is delegated to the operating system.");
        OutputDirectorySummary = string.IsNullOrWhiteSpace(result.OutputDirectory)
            ? "Output folder: unavailable because no artifact path was recorded."
            : result.OutputDirectoryExists
                ? $"Output folder: {result.OutputDirectory}"
                : $"Output folder missing: {result.OutputDirectory}";

        SelectedArtifact = Artifacts.FirstOrDefault(item => item.Artifact.CanOpen)
            ?? Artifacts.FirstOrDefault();
        return $"Loaded {Artifacts.Count:n0} artifact entr{(Artifacts.Count == 1 ? "y" : "ies")}.";
    }

    public void SetSelectedArtifact(ArtifactListItemViewModel? artifact)
    {
        SelectedArtifact = artifact;
    }

    public async Task<string> OpenSelectedAsync(CancellationToken cancellationToken = default)
    {
        ArtifactExplorerItem? artifact = SelectedArtifact?.Artifact;
        if (artifact is null)
        {
            return "Select an artifact first.";
        }

        if (!artifact.CanOpen || string.IsNullOrWhiteSpace(artifact.ResolvedPath))
        {
            return artifact.Availability == ArtifactAvailability.Missing
                ? $"The recorded file is missing: {artifact.ResolvedPath}"
                : $"{artifact.DisplayName} was not generated or was not recorded for this run.";
        }

        await _pathLauncher(artifact.ResolvedPath, false, cancellationToken).ConfigureAwait(true);
        return $"Opened {artifact.DisplayName}.";
    }

    public async Task<string> OpenOutputDirectoryAsync(CancellationToken cancellationToken = default)
    {
        if (!CanOpenOutputDirectory)
        {
            return string.IsNullOrWhiteSpace(OutputDirectory)
                ? "No output folder can be resolved for this run."
                : $"The output folder is missing: {OutputDirectory}";
        }

        await _pathLauncher(OutputDirectory, true, cancellationToken).ConfigureAwait(true);
        return "Opened the run output folder.";
    }

    public void Clear(string message)
    {
        Artifacts.Clear();
        SelectedArtifact = null;
        OutputDirectory = string.Empty;
        OutputDirectoryExists = false;
        ArtifactExplorerTitle = "Reports and exports";
        ArtifactExplorerSummary = message;
        ArtifactDetails = "Artifact details will appear here.";
        OutputDirectorySummary = "Output folder: unavailable";
    }

    private static string FormatArtifactDetails(ArtifactExplorerItem? artifact)
    {
        if (artifact is null)
        {
            return "Select an artifact to inspect its path and availability.";
        }

        string storedPath = artifact.IsPathRecorded
            ? artifact.StoredPath
            : "Not stored in the analysis-run record";
        string resolvedPath = string.IsNullOrWhiteSpace(artifact.ResolvedPath)
            ? "Unavailable"
            : artifact.ResolvedPath;
        string statusExplanation = artifact.Availability switch
        {
            ArtifactAvailability.Available => "The file exists and can be opened.",
            ArtifactAvailability.Missing => "The run records this path, but the file no longer exists there.",
            _ => artifact.IsOptional
                ? "This optional artifact was not found in the resolved output folder."
                : "No path was recorded, so this artifact was not generated or cannot be located.",
        };

        return string.Join(Environment.NewLine,
            $"Name: {artifact.DisplayName}",
            $"Expected file: {artifact.ExpectedFileName}",
            $"Status: {artifact.AvailabilityLabel}",
            $"Recorded path: {storedPath}",
            $"Resolved path: {resolvedPath}",
            $"Optional: {(artifact.IsOptional ? "yes" : "no")}",
            string.Empty,
            statusExplanation);
    }

    private static Task<ArtifactExplorerResult> LoadArtifactsFromApplicationAsync(
        ArtifactExplorerRequest request,
        CancellationToken cancellationToken)
    {
        return Task.Run(async () =>
        {
            ArtifactExplorerQueryService service = new();
            return await service.GetArtifactsAsync(request, cancellationToken).ConfigureAwait(false);
        }, cancellationToken);
    }
}
