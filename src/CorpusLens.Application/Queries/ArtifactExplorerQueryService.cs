using CorpusLens.Domain.Storage;

namespace CorpusLens.Application.Queries;

public sealed class ArtifactExplorerQueryService
{
    public async Task<ArtifactExplorerResult> GetArtifactsAsync(
        ArtifactExplorerRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.DatabasePath);

        if (request.AnalysisRunId <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(request),
                request.AnalysisRunId,
                "Analysis run ID must be greater than zero.");
        }

        string databasePath = Path.GetFullPath(request.DatabasePath);
        AnalysisRunQueryService runQueryService = new(databasePath);
        StoredAnalysisRun? run = await runQueryService
            .GetRunAsync(request.AnalysisRunId, cancellationToken)
            .ConfigureAwait(false);

        if (run is null)
        {
            throw new InvalidOperationException($"Analysis run {request.AnalysisRunId} was not found.");
        }

        cancellationToken.ThrowIfCancellationRequested();

        IReadOnlyList<string> storedPaths = new[]
        {
            run.ReportPath,
            run.WordsCsvPath,
            run.NGramsCsvPath,
            run.NextWordsCsvPath,
            run.ExtractedTextPath,
        };
        string preferredRelativeBase = FindPreferredRelativeBase(databasePath, storedPaths);

        List<ArtifactExplorerItem> artifacts = new()
        {
            CreateRecordedArtifact("report", "Markdown report", "report.md", run.ReportPath, databasePath, preferredRelativeBase),
            CreateRecordedArtifact("words", "Word frequencies CSV", "words.csv", run.WordsCsvPath, databasePath, preferredRelativeBase),
            CreateRecordedArtifact("ngrams", "N-grams CSV", "ngrams.csv", run.NGramsCsvPath, databasePath, preferredRelativeBase),
            CreateRecordedArtifact("next-words", "Next-word statistics CSV", "next_words.csv", run.NextWordsCsvPath, databasePath, preferredRelativeBase),
            CreateRecordedArtifact("extracted-text", "Extracted clean text", "extracted_text.txt", run.ExtractedTextPath, databasePath, preferredRelativeBase),
        };

        string outputDirectory = ResolveOutputDirectory(artifacts);
        artifacts.Add(CreateDiscoveredArtifact(
            "import-diagnostics",
            "EPUB import diagnostics",
            "import_diagnostics.md",
            outputDirectory));
        artifacts.Add(CreateDiscoveredArtifact(
            "import-failures",
            "EPUB import failures CSV",
            "import_failures.csv",
            outputDirectory));

        return new ArtifactExplorerResult(
            run.Id,
            outputDirectory,
            !string.IsNullOrWhiteSpace(outputDirectory) && Directory.Exists(outputDirectory),
            artifacts);
    }

    private static ArtifactExplorerItem CreateRecordedArtifact(
        string id,
        string displayName,
        string expectedFileName,
        string storedPath,
        string databasePath,
        string preferredRelativeBase)
    {
        if (string.IsNullOrWhiteSpace(storedPath))
        {
            return new ArtifactExplorerItem(
                id,
                displayName,
                expectedFileName,
                string.Empty,
                string.Empty,
                ArtifactAvailability.NotGenerated,
                false,
                false);
        }

        string resolvedPath = ResolveStoredPath(databasePath, storedPath, preferredRelativeBase);
        ArtifactAvailability availability = File.Exists(resolvedPath)
            ? ArtifactAvailability.Available
            : ArtifactAvailability.Missing;

        return new ArtifactExplorerItem(
            id,
            displayName,
            expectedFileName,
            storedPath,
            resolvedPath,
            availability,
            false,
            true);
    }

    private static ArtifactExplorerItem CreateDiscoveredArtifact(
        string id,
        string displayName,
        string expectedFileName,
        string outputDirectory)
    {
        string resolvedPath = string.IsNullOrWhiteSpace(outputDirectory)
            ? string.Empty
            : Path.Combine(outputDirectory, expectedFileName);
        ArtifactAvailability availability = !string.IsNullOrWhiteSpace(resolvedPath)
            && File.Exists(resolvedPath)
                ? ArtifactAvailability.Available
                : ArtifactAvailability.NotGenerated;

        return new ArtifactExplorerItem(
            id,
            displayName,
            expectedFileName,
            string.Empty,
            resolvedPath,
            availability,
            true,
            false);
    }

    private static string ResolveOutputDirectory(IReadOnlyList<ArtifactExplorerItem> artifacts)
    {
        string? availableDirectory = artifacts
            .Where(artifact => artifact.Availability == ArtifactAvailability.Available)
            .Select(artifact => SafeGetDirectoryName(artifact.ResolvedPath))
            .FirstOrDefault(directory => !string.IsNullOrWhiteSpace(directory) && Directory.Exists(directory));
        if (!string.IsNullOrWhiteSpace(availableDirectory))
        {
            return availableDirectory;
        }

        string? recordedDirectory = artifacts
            .Where(artifact => artifact.IsPathRecorded)
            .Select(artifact => SafeGetDirectoryName(artifact.ResolvedPath))
            .FirstOrDefault(directory => !string.IsNullOrWhiteSpace(directory));
        return recordedDirectory ?? string.Empty;
    }

    private static string FindPreferredRelativeBase(
        string databasePath,
        IReadOnlyList<string> storedPaths)
    {
        IReadOnlyList<string> baseDirectories = BuildBaseDirectories(databasePath);
        foreach (string storedPath in storedPaths.Where(path => !string.IsNullOrWhiteSpace(path)))
        {
            string trimmedPath = storedPath.Trim();
            if (Path.IsPathFullyQualified(trimmedPath))
            {
                continue;
            }

            string? matchingBase = baseDirectories.FirstOrDefault(baseDirectory =>
            {
                string candidate = SafeCombineAndNormalize(baseDirectory, trimmedPath, string.Empty);
                return !string.IsNullOrWhiteSpace(candidate) && File.Exists(candidate);
            });
            if (!string.IsNullOrWhiteSpace(matchingBase))
            {
                return matchingBase;
            }
        }

        return string.Empty;
    }

    private static string ResolveStoredPath(
        string databasePath,
        string storedPath,
        string preferredRelativeBase)
    {
        string trimmedPath = storedPath.Trim();
        if (Path.IsPathFullyQualified(trimmedPath))
        {
            return SafeGetFullPath(trimmedPath, trimmedPath);
        }

        List<string> baseDirectories = new();
        if (!string.IsNullOrWhiteSpace(preferredRelativeBase))
        {
            baseDirectories.Add(preferredRelativeBase);
        }

        baseDirectories.AddRange(BuildBaseDirectories(databasePath));
        List<string> candidates = baseDirectories
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(baseDirectory => SafeCombineAndNormalize(baseDirectory, trimmedPath, trimmedPath))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        string? existingPath = candidates.FirstOrDefault(File.Exists);
        return existingPath ?? candidates.FirstOrDefault() ?? trimmedPath;
    }

    private static IReadOnlyList<string> BuildBaseDirectories(string databasePath)
    {
        string fullDatabasePath = SafeGetFullPath(databasePath, databasePath);
        string databaseDirectory = Path.GetDirectoryName(fullDatabasePath) ?? Environment.CurrentDirectory;
        string? databaseParentDirectory = Directory.GetParent(databaseDirectory)?.FullName;

        List<string> baseDirectories = new()
        {
            Environment.CurrentDirectory,
        };
        if (!string.IsNullOrWhiteSpace(databaseParentDirectory))
        {
            baseDirectories.Add(databaseParentDirectory);
        }

        baseDirectories.Add(databaseDirectory);
        return baseDirectories.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
    }

    private static string SafeCombineAndNormalize(string baseDirectory, string relativePath, string fallback)
    {
        try
        {
            return SafeGetFullPath(Path.Combine(baseDirectory, relativePath), fallback);
        }
        catch (Exception exception) when (
            exception is ArgumentException
            or NotSupportedException
            or PathTooLongException)
        {
            return fallback;
        }
    }

    private static string SafeGetFullPath(string path, string fallback)
    {
        try
        {
            return Path.GetFullPath(path);
        }
        catch (Exception exception) when (
            exception is ArgumentException
            or NotSupportedException
            or PathTooLongException)
        {
            return fallback;
        }
    }

    private static string SafeGetDirectoryName(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return string.Empty;
        }

        try
        {
            return Path.GetDirectoryName(path) ?? string.Empty;
        }
        catch (Exception exception) when (
            exception is ArgumentException
            or NotSupportedException
            or PathTooLongException)
        {
            return string.Empty;
        }
    }
}
