using CorpusLens.Application.EpubAnalysis;
using CorpusLens.Application.Storage;
using CorpusLens.Desktop.Services;
using CorpusLens.Domain.Storage;

namespace CorpusLens.Desktop.ViewModels;

public sealed class EpubAnalysisViewModel : ViewModelBase
{
    private readonly Func<AnalyzeEpubFolderAndSaveRequest, CancellationToken, IProgress<EpubAnalysisProgress>?, Task<AnalyzeEpubFolderAndSaveResult>> _analyzer;
    private readonly Func<Action<EpubAnalysisProgress>, IProgress<EpubAnalysisProgress>> _progressFactory;
    private readonly Func<string, bool, CancellationToken, Task> _pathLauncher;

    private int _progressPercent;
    private string _progressSummary = "Ready to analyze an EPUB folder.";
    private string _resultSummary = "No desktop EPUB analysis has been completed in this session.";
    private string _lastOutputDirectory = string.Empty;
    private string _lastDiagnosticsPath = string.Empty;
    private string _lastFailuresPath = string.Empty;

    public EpubAnalysisViewModel(
        Func<AnalyzeEpubFolderAndSaveRequest, CancellationToken, IProgress<EpubAnalysisProgress>?, Task<AnalyzeEpubFolderAndSaveResult>>? analyzer = null,
        Func<Action<EpubAnalysisProgress>, IProgress<EpubAnalysisProgress>>? progressFactory = null,
        Func<string, bool, CancellationToken, Task>? pathLauncher = null)
    {
        _analyzer = analyzer ?? AnalyzeFromApplicationAsync;
        _progressFactory = progressFactory ?? (handler => new Progress<EpubAnalysisProgress>(handler));
        _pathLauncher = pathLauncher ?? SystemPathLauncher.OpenAsync;
    }

    public int ProgressPercent
    {
        get => _progressPercent;
        private set => SetProperty(ref _progressPercent, Math.Clamp(value, 0, 100));
    }

    public string ProgressSummary
    {
        get => _progressSummary;
        private set => SetProperty(ref _progressSummary, value);
    }

    public string ResultSummary
    {
        get => _resultSummary;
        private set => SetProperty(ref _resultSummary, value);
    }

    public string LastOutputDirectory
    {
        get => _lastOutputDirectory;
        private set
        {
            if (SetProperty(ref _lastOutputDirectory, value))
            {
                OnPropertyChanged(nameof(CanOpenOutputDirectory));
            }
        }
    }

    public string LastDiagnosticsPath
    {
        get => _lastDiagnosticsPath;
        private set
        {
            if (SetProperty(ref _lastDiagnosticsPath, value))
            {
                OnPropertyChanged(nameof(CanOpenDiagnostics));
            }
        }
    }

    public string LastFailuresPath
    {
        get => _lastFailuresPath;
        private set
        {
            if (SetProperty(ref _lastFailuresPath, value))
            {
                OnPropertyChanged(nameof(CanOpenFailures));
            }
        }
    }

    public bool CanOpenOutputDirectory => Directory.Exists(LastOutputDirectory);

    public bool CanOpenDiagnostics => File.Exists(LastDiagnosticsPath);

    public bool CanOpenFailures => File.Exists(LastFailuresPath);

    public bool TryCreateRequest(
        string databasePath,
        CorpusListItemViewModel? selectedCorpus,
        string? inputFolder,
        string? outputDirectory,
        bool recursive,
        bool confirmed,
        out AnalyzeEpubFolderAndSaveRequest? request,
        out string error)
    {
        request = null;
        error = string.Empty;

        if (!File.Exists(databasePath))
        {
            error = "Open a valid CorpusLens database before starting an EPUB analysis.";
            return false;
        }

        StoredCorpus? corpus = selectedCorpus?.Corpus;
        if (corpus is null)
        {
            error = "Select one specific corpus instead of All corpora.";
            return false;
        }

        if (!CorpusLanguageCatalog.TryNormalizeSupportedCode(corpus.LanguageCode, out string normalizedLanguage))
        {
            error = $"Corpus '{corpus.Name}' uses unsupported language code '{corpus.LanguageCode}'.";
            return false;
        }

        if (!confirmed)
        {
            error = "Confirm creation of analysis artifacts and a persistent run.";
            return false;
        }

        string normalizedInput = inputFolder?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(normalizedInput) || !Directory.Exists(normalizedInput))
        {
            error = $"EPUB input folder not found: {normalizedInput}";
            return false;
        }

        string normalizedOutput = outputDirectory?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(normalizedOutput))
        {
            error = "Choose an output folder for reports and extracted text.";
            return false;
        }

        try
        {
            normalizedInput = Path.GetFullPath(normalizedInput);
            normalizedOutput = Path.GetFullPath(normalizedOutput);
            SearchOption searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            bool hasEpub = Directory
                .EnumerateFiles(normalizedInput, EpubAnalysisDefaults.SearchPattern, searchOption)
                .Any(file => string.Equals(Path.GetExtension(file), ".epub", StringComparison.OrdinalIgnoreCase));
            if (!hasEpub)
            {
                error = recursive
                    ? "No EPUB files were found in the selected folder or its subfolders."
                    : "No EPUB files were found in the selected folder. Enable recursive scanning when needed.";
                return false;
            }
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
        {
            error = $"Could not validate the selected folders: {exception.Message}";
            return false;
        }

        request = new AnalyzeEpubFolderAndSaveRequest(
            normalizedInput,
            normalizedLanguage,
            normalizedOutput,
            EpubAnalysisDefaults.CreateSettings(),
            EpubAnalysisDefaults.SearchPattern,
            recursive,
            databasePath,
            corpus.Name);
        return true;
    }

    public async Task<AnalyzeEpubFolderAndSaveResult> ExecuteAsync(
        AnalyzeEpubFolderAndSaveRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ProgressPercent = 0;
        ProgressSummary = "Starting EPUB analysis...";
        ResultSummary = "Analysis in progress. Existing completed runs remain unchanged.";

        IProgress<EpubAnalysisProgress> progress = _progressFactory(UpdateProgress);
        AnalyzeEpubFolderAndSaveResult result = await _analyzer(request, cancellationToken, progress)
            .ConfigureAwait(true);
        AnalyzeEpubFolderResult analysis = result.AnalysisResult;
        LastOutputDirectory = Path.GetFullPath(request.OutputDirectory);
        LastDiagnosticsPath = Path.GetFullPath(analysis.ImportDiagnosticsPath);
        LastFailuresPath = Path.GetFullPath(analysis.ImportFailuresCsvPath);
        ProgressPercent = 100;
        ProgressSummary = $"Run {result.AnalysisRun.Id} completed.";
        ResultSummary = string.Join(
            Environment.NewLine,
            $"Run: {result.AnalysisRun.Id}",
            $"Corpus: {result.Corpus.Name} [{result.Corpus.LanguageCode}]",
            $"Imported EPUB files: {analysis.SourceBooks.Count:n0}",
            $"Skipped/failed EPUB files: {analysis.Failures.Count:n0}",
            $"Chapters: {analysis.SourceBooks.Sum(book => book.Chapters.Count):n0}",
            $"Word tokens: {analysis.Analysis.Summary.WordTokenCount:n0}",
            $"Distinct words: {analysis.Analysis.Summary.DistinctWordCount:n0}",
            $"Output: {LastOutputDirectory}",
            analysis.Failures.Count == 0
                ? "Import diagnostics: no EPUB failures were recorded."
                : $"Import diagnostics contain {analysis.Failures.Count:n0} skipped file(s).");
        return result;
    }

    public void MarkCancelled()
    {
        ProgressSummary = "EPUB analysis cancelled. No new run was retained.";
        ResultSummary = "The operation was cancelled. Generated filesystem artifacts may remain in the selected output folder, but partial database books and runs are removed.";
    }

    public void MarkFailed(string message)
    {
        ProgressSummary = "EPUB analysis failed.";
        ResultSummary = message;
    }

    public async Task<string> OpenOutputDirectoryAsync(CancellationToken cancellationToken = default)
    {
        if (!CanOpenOutputDirectory)
        {
            return "No completed analysis output folder is available.";
        }

        await _pathLauncher(LastOutputDirectory, true, cancellationToken).ConfigureAwait(true);
        return "Opened the latest analysis output folder.";
    }

    public async Task<string> OpenDiagnosticsAsync(CancellationToken cancellationToken = default)
    {
        if (!CanOpenDiagnostics)
        {
            return "No import diagnostics file is available.";
        }

        await _pathLauncher(LastDiagnosticsPath, false, cancellationToken).ConfigureAwait(true);
        return "Opened the latest EPUB import diagnostics.";
    }

    public async Task<string> OpenFailuresAsync(CancellationToken cancellationToken = default)
    {
        if (!CanOpenFailures)
        {
            return "No import-failures CSV is available.";
        }

        await _pathLauncher(LastFailuresPath, false, cancellationToken).ConfigureAwait(true);
        return "Opened the latest import-failures CSV.";
    }

    private void UpdateProgress(EpubAnalysisProgress progress)
    {
        ProgressPercent = progress.NormalizedPercent;
        string counts = progress.TotalFiles > 0
            ? $" · files {progress.CompletedFiles:n0}/{progress.TotalFiles:n0} · imported {progress.ImportedFiles:n0} · skipped {progress.FailedFiles:n0}"
            : string.Empty;
        ProgressSummary = $"{progress.NormalizedPercent:n0}% · {progress.Message}{counts}";
    }

    private static Task<AnalyzeEpubFolderAndSaveResult> AnalyzeFromApplicationAsync(
        AnalyzeEpubFolderAndSaveRequest request,
        CancellationToken cancellationToken,
        IProgress<EpubAnalysisProgress>? progress)
    {
        AnalyzeEpubFolderAndSaveUseCase useCase = new();
        return useCase.ExecuteAsync(request, cancellationToken, progress);
    }
}
