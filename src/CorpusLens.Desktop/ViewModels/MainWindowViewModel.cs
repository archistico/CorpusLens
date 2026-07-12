using System.Collections.ObjectModel;
using System.ComponentModel;
using CorpusLens.Application.EpubAnalysis;
using CorpusLens.Application.Queries;
using CorpusLens.Application.Storage;
using CorpusLens.Domain.Storage;

namespace CorpusLens.Desktop.ViewModels;

public sealed class MainWindowViewModel : ViewModelBase
{
    private readonly Func<string, CancellationToken, Task<IReadOnlyList<StoredAnalysisRunSummary>>> _runLoader;
    private CancellationTokenSource? _currentOperationCancellation;
    private long _operationVersion;
    private string _databasePath = "No database selected";
    private RunListItemViewModel? _selectedRun;

    public MainWindowViewModel(
        DashboardViewModel? dashboard = null,
        BooksExplorerViewModel? books = null,
        ChaptersExplorerViewModel? chaptersExplorer = null,
        WordExplorerViewModel? wordExplorer = null,
        NGramExplorerViewModel? ngramExplorer = null,
        ArtifactExplorerViewModel? artifactExplorer = null,
        CollocationsExplorerViewModel? collocationsExplorer = null,
        PhraseExplorerViewModel? phraseExplorer = null,
        CompareRunsViewModel? compareRuns = null,
        CorpusManagementViewModel? corpora = null,
        EpubAnalysisViewModel? epubAnalysis = null,
        DesktopOperationStateViewModel? operationState = null,
        Func<string, CancellationToken, Task<IReadOnlyList<StoredAnalysisRunSummary>>>? runLoader = null)
    {
        Dashboard = dashboard ?? new DashboardViewModel();
        Books = books ?? new BooksExplorerViewModel();
        ChaptersExplorer = chaptersExplorer ?? new ChaptersExplorerViewModel();
        WordExplorer = wordExplorer ?? new WordExplorerViewModel();
        NGramExplorer = ngramExplorer ?? new NGramExplorerViewModel();
        ArtifactExplorer = artifactExplorer ?? new ArtifactExplorerViewModel();
        CollocationsExplorer = collocationsExplorer ?? new CollocationsExplorerViewModel();
        PhraseExplorer = phraseExplorer ?? new PhraseExplorerViewModel();
        CompareRuns = compareRuns ?? new CompareRunsViewModel();
        Corpora = corpora ?? new CorpusManagementViewModel();
        EpubAnalysis = epubAnalysis ?? new EpubAnalysisViewModel();
        OperationState = operationState ?? new DesktopOperationStateViewModel();
        _runLoader = runLoader ?? LoadRunsFromApplicationAsync;

        ForwardPropertyChanges(Dashboard);
        ForwardPropertyChanges(Books);
        ForwardPropertyChanges(ChaptersExplorer);
        ForwardPropertyChanges(WordExplorer);
        ForwardPropertyChanges(NGramExplorer);
        ForwardPropertyChanges(ArtifactExplorer);
        ForwardPropertyChanges(CollocationsExplorer);
        ForwardPropertyChanges(PhraseExplorer);
        ForwardPropertyChanges(CompareRuns);
        ForwardPropertyChanges(Corpora);
        ForwardPropertyChanges(EpubAnalysis);
        EpubAnalysis.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(EpubAnalysisViewModel.ProgressPercent))
            {
                OnPropertyChanged(nameof(EpubAnalysisProgressPercent));
            }

            if (args.PropertyName == nameof(EpubAnalysisViewModel.ProgressSummary))
            {
                OnPropertyChanged(nameof(EpubAnalysisProgressSummary));
            }

            if (args.PropertyName == nameof(EpubAnalysisViewModel.ResultSummary))
            {
                OnPropertyChanged(nameof(EpubAnalysisResultSummary));
            }

            if (args.PropertyName == nameof(EpubAnalysisViewModel.CanOpenOutputDirectory))
            {
                OnPropertyChanged(nameof(CanOpenLatestAnalysisOutput));
            }

            if (args.PropertyName == nameof(EpubAnalysisViewModel.CanOpenDiagnostics))
            {
                OnPropertyChanged(nameof(CanOpenLatestAnalysisDiagnostics));
            }

            if (args.PropertyName == nameof(EpubAnalysisViewModel.CanOpenFailures))
            {
                OnPropertyChanged(nameof(CanOpenLatestAnalysisFailures));
            }
        };
        Corpora.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(CorpusManagementViewModel.Summary))
            {
                OnPropertyChanged(nameof(CorpusSummary));
            }

            if (args.PropertyName == nameof(CorpusManagementViewModel.Details))
            {
                OnPropertyChanged(nameof(CorpusDetails));
            }

            if (args.PropertyName == nameof(CorpusManagementViewModel.SelectedCorpus))
            {
                OnPropertyChanged(nameof(EpubAnalysisCorpusSummary));
            }
        };
        ForwardPropertyChanges(OperationState);
    }

    public DashboardViewModel Dashboard { get; }

    public BooksExplorerViewModel Books { get; }

    public ChaptersExplorerViewModel ChaptersExplorer { get; }

    public WordExplorerViewModel WordExplorer { get; }

    public NGramExplorerViewModel NGramExplorer { get; }

    public ArtifactExplorerViewModel ArtifactExplorer { get; }

    public CollocationsExplorerViewModel CollocationsExplorer { get; }

    public PhraseExplorerViewModel PhraseExplorer { get; }

    public CompareRunsViewModel CompareRuns { get; }

    public CorpusManagementViewModel Corpora { get; }

    public EpubAnalysisViewModel EpubAnalysis { get; }

    public DesktopOperationStateViewModel OperationState { get; }

    public string DatabasePath
    {
        get => _databasePath;
        private set => SetProperty(ref _databasePath, value);
    }

    public ObservableCollection<RunListItemViewModel> Runs { get; } = new();

    public ObservableCollection<RunListItemViewModel> VisibleRuns { get; } = new();

    public ObservableCollection<CorpusListItemViewModel> CorpusItems => Corpora.Corpora;

    public ObservableCollection<RunBookListItemViewModel> RunBooks => Books.RunBooks;

    public ObservableCollection<ChapterListItemViewModel> BookChapters => ChaptersExplorer.Chapters;

    public ObservableCollection<ArtifactListItemViewModel> RunArtifacts => ArtifactExplorer.Artifacts;

    public RunListItemViewModel? SelectedRun
    {
        get => _selectedRun;
        private set => SetProperty(ref _selectedRun, value);
    }

    public string StatusMessage => OperationState.StatusMessage;

    public bool IsBusy => OperationState.IsBusy;

    public CorpusListItemViewModel? SelectedCorpus => Corpora.SelectedCorpus;

    public string CorpusSummary => Corpora.Summary;

    public string CorpusDetails => Corpora.Details;

    public IReadOnlyList<CorpusLanguageOption> SupportedCorpusLanguages => Corpora.SupportedLanguages;

    public CorpusLanguageOption? DefaultCorpusLanguage => Corpora.DefaultLanguage;

    public string EpubAnalysisCorpusSummary => SelectedCorpus?.Corpus is StoredCorpus corpus
        ? $"Target corpus: {corpus.Name} [{corpus.LanguageCode}]"
        : "Select one specific corpus before starting an EPUB analysis.";

    public int EpubAnalysisProgressPercent => EpubAnalysis.ProgressPercent;

    public string EpubAnalysisProgressSummary => EpubAnalysis.ProgressSummary;

    public string EpubAnalysisResultSummary => EpubAnalysis.ResultSummary;

    public bool CanOpenLatestAnalysisOutput => EpubAnalysis.CanOpenOutputDirectory;

    public bool CanOpenLatestAnalysisDiagnostics => EpubAnalysis.CanOpenDiagnostics;

    public bool CanOpenLatestAnalysisFailures => EpubAnalysis.CanOpenFailures;

    public string RunTitle => Dashboard.RunTitle;

    public string RunSubtitle => Dashboard.RunSubtitle;

    public string CoreMetrics => Dashboard.CoreMetrics;

    public string ProfileSummary => Dashboard.ProfileSummary;

    public string DifficultySummary => Dashboard.DifficultySummary;

    public string TopContentWords => Dashboard.TopContentWords;

    public string TopFunctionWords => Dashboard.TopFunctionWords;

    public string RecurringPhrases => Dashboard.RecurringPhrases;

    public string TokenIndexSummary => Dashboard.TokenIndexSummary;

    public string QueryPathSummary => Dashboard.QueryPathSummary;

    public string ReportPath => Dashboard.ReportPath;

    public string BooksExplorerTitle => Books.BooksExplorerTitle;

    public string BooksExplorerSummary => Books.BooksExplorerSummary;

    public string RunBookDetails => Books.RunBookDetails;

    public RunBookListItemViewModel? SelectedRunBook => Books.SelectedRunBook;

    public string ChapterExplorerTitle => ChaptersExplorer.ChapterExplorerTitle;

    public string ChapterExplorerSummary => ChaptersExplorer.ChapterExplorerSummary;

    public string ChapterDetails => ChaptersExplorer.ChapterDetails;

    public string ChapterPreview => ChaptersExplorer.ChapterPreview;

    public string ChapterSearchSummary => ChaptersExplorer.ChapterSearchSummary;

    public ChapterListItemViewModel? SelectedChapter => ChaptersExplorer.SelectedChapter;

    public int ChapterPreviewSelectionStart => ChaptersExplorer.ChapterPreviewSelectionStart;

    public int ChapterPreviewSelectionLength => ChaptersExplorer.ChapterPreviewSelectionLength;

    public string WordExplorerTitle => WordExplorer.WordExplorerTitle;

    public string WordExplorerSummary => WordExplorer.WordExplorerSummary;

    public string WordNextWords => WordExplorer.WordNextWords;

    public string WordPreviousWords => WordExplorer.WordPreviousWords;

    public string WordKwic => WordExplorer.WordKwic;

    public string WordBookDistribution => WordExplorer.WordBookDistribution;

    public string NGramExplorerTitle => NGramExplorer.NGramExplorerTitle;

    public string NGramExplorerSummary => NGramExplorer.NGramExplorerSummary;

    public string NGramResults => NGramExplorer.NGramResults;

    public string NGramSizeLabel => NGramExplorer.NGramSizeLabel;

    public string NGramFilterLabel => NGramExplorer.NGramFilterLabel;

    public string NGramSortLabel => NGramExplorer.NGramSortLabel;

    public int? NGramSize => NGramExplorer.NGramSize;

    public NGramExplorerFilter NGramFilter => NGramExplorer.NGramFilter;

    public NGramExplorerSort NGramSort => NGramExplorer.NGramSort;

    public string ArtifactExplorerTitle => ArtifactExplorer.ArtifactExplorerTitle;

    public string ArtifactExplorerSummary => ArtifactExplorer.ArtifactExplorerSummary;

    public string ArtifactDetails => ArtifactExplorer.ArtifactDetails;

    public string OutputDirectorySummary => ArtifactExplorer.OutputDirectorySummary;

    public ArtifactListItemViewModel? SelectedArtifact => ArtifactExplorer.SelectedArtifact;

    public bool CanOpenSelectedArtifact => ArtifactExplorer.CanOpenSelectedArtifact;

    public bool CanOpenOutputDirectory => ArtifactExplorer.CanOpenOutputDirectory;

    public string CollocationExplorerTitle => CollocationsExplorer.CollocationExplorerTitle;

    public string CollocationExplorerSummary => CollocationsExplorer.CollocationExplorerSummary;

    public string CollocationResults => CollocationsExplorer.CollocationResults;

    public string CollocationFilterLabel => CollocationsExplorer.CollocationFilterLabel;

    public CollocationExplorerFilter CollocationFilter => CollocationsExplorer.CollocationFilter;

    public string PhraseExplorerTitle => PhraseExplorer.PhraseExplorerTitle;

    public string PhraseExplorerSummary => PhraseExplorer.PhraseExplorerSummary;

    public string PhraseResults => PhraseExplorer.PhraseResults;

    public string PhraseBoundaryLabel => PhraseExplorer.PhraseBoundaryLabel;

    public bool PhraseContentBoundaryOnly => PhraseExplorer.PhraseContentBoundaryOnly;

    public bool PhraseLongestOnly => PhraseExplorer.PhraseLongestOnly;

    public RunListItemViewModel? ComparisonRightRun => CompareRuns.ComparisonRightRun;

    public string ComparisonExplorerTitle => CompareRuns.ComparisonExplorerTitle;

    public string ComparisonExplorerSummary => CompareRuns.ComparisonExplorerSummary;

    public string ComparisonWordSummary => CompareRuns.ComparisonWordSummary;

    public string ComparisonWords => CompareRuns.ComparisonWords;

    public string ComparisonDifficulty => CompareRuns.ComparisonDifficulty;

    public string ComparisonWordFilterLabel => CompareRuns.ComparisonWordFilterLabel;

    public string ComparisonPresenceLabel => CompareRuns.ComparisonPresenceLabel;

    public ComparisonWordFilter ComparisonWordFilter => CompareRuns.ComparisonWordFilter;

    public ComparisonPresenceFilter ComparisonPresenceFilter => CompareRuns.ComparisonPresenceFilter;

    public async Task OpenDatabaseAsync(string databasePath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(databasePath))
        {
            return;
        }

        if (!File.Exists(databasePath))
        {
            OperationState.SetStatus($"Database not found: {databasePath}");
            return;
        }

        if (!string.Equals(DatabasePath, databasePath, StringComparison.OrdinalIgnoreCase))
        {
            Runs.Clear();
            VisibleRuns.Clear();
            Corpora.Clear("Loading corpora from database...");
            ClearSelectedRun("Loading runs from database...");
        }

        DatabasePath = databasePath;
        await RefreshRunsAsync(cancellationToken).ConfigureAwait(true);
    }

    public async Task RefreshRunsAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(DatabasePath))
        {
            CancelCurrentOperation();
            Runs.Clear();
            VisibleRuns.Clear();
            Corpora.Clear("Open a CorpusLens SQLite database first.");
            ClearSelectedRun("No valid database selected.");
            OperationState.SetStatus("Open a CorpusLens SQLite database first.");
            return;
        }

        long? preferredRunId = SelectedRun?.Id;
        await ExecuteBusyAsync(
            "Loading corpora and runs from database...",
            token => ReloadRunsCoreAsync(preferredRunId, token),
            "Loading cancelled.",
            ex => $"Error loading database: {ex.Message}",
            ex =>
            {
                Runs.Clear();
                VisibleRuns.Clear();
                Corpora.Clear("Could not load corpora from this database.");
                ClearSelectedRun("Could not load runs from this database.");
            },
            cancellationToken).ConfigureAwait(true);
    }


    public async Task SelectCorpusAsync(
        CorpusListItemViewModel? corpus,
        CancellationToken cancellationToken = default)
    {
        Corpora.SetSelectedCorpus(corpus);
        RebuildVisibleRuns();
        CompareRuns.EnsureDefaultRightRun(Runs, SelectedRun);
        CorpusListItemViewModel? selectedCorpus = Corpora.SelectedCorpus;
        bool isSpecificCorpus = selectedCorpus is { IsAllCorpora: false };

        if (VisibleRuns.Count == 0)
        {
            ClearSelectedRun(isSpecificCorpus
                ? "The selected corpus does not contain analysis runs."
                : "The database does not contain analysis runs.");
            OperationState.SetStatus(isSpecificCorpus
                ? $"Corpus '{selectedCorpus!.Name}' selected. No runs found."
                : "All corpora selected. No runs found.");
            return;
        }

        if (SelectedRun is not null && VisibleRuns.Contains(SelectedRun))
        {
            OperationState.SetStatus(isSpecificCorpus
                ? $"Showing {VisibleRuns.Count} run(s) for corpus '{selectedCorpus!.Name}'."
                : $"Showing all {VisibleRuns.Count} run(s)." );
            return;
        }

        await SelectRunAsync(VisibleRuns[0], cancellationToken).ConfigureAwait(true);
    }

    public async Task CreateCorpusAsync(
        string? name,
        string? languageCode,
        string? description,
        bool confirmed,
        CancellationToken cancellationToken = default)
    {
        if (!File.Exists(DatabasePath))
        {
            OperationState.SetStatus("Open a CorpusLens SQLite database before creating a corpus.");
            return;
        }

        if (!confirmed)
        {
            OperationState.SetStatus("Confirm the persistent write before creating the corpus.");
            return;
        }

        if (!Corpora.TryValidateCreateInput(
            name,
            languageCode,
            out string normalizedName,
            out string normalizedLanguage,
            out string validationError))
        {
            OperationState.SetStatus(validationError);
            return;
        }

        await ExecuteBusyAsync(
            $"Creating corpus '{normalizedName}'...",
            async token =>
            {
                StoredCorpus created = await Corpora.CreateAsync(
                    DatabasePath,
                    normalizedName,
                    normalizedLanguage,
                    description,
                    Runs,
                    token).ConfigureAwait(true);
                RebuildVisibleRuns();
                ClearSelectedRun("The new corpus does not contain analysis runs yet.");
                return $"Corpus '{created.Name}' [{created.LanguageCode}] created successfully.";
            },
            "Corpus creation cancelled.",
            ex => $"Could not create corpus: {ex.Message}",
            null,
            cancellationToken).ConfigureAwait(true);
    }

    public bool IsSelectedCorpusLanguageCompatible(string? languageCode)
    {
        return Corpora.IsSelectedCorpusLanguageCompatible(languageCode);
    }

    public async Task AnalyzeEpubFolderAsync(
        string? inputFolder,
        string? outputDirectory,
        bool recursive,
        bool confirmed,
        CancellationToken cancellationToken = default)
    {
        if (!EpubAnalysis.TryCreateRequest(
            DatabasePath,
            SelectedCorpus,
            inputFolder,
            outputDirectory,
            recursive,
            confirmed,
            out AnalyzeEpubFolderAndSaveRequest? request,
            out string validationError))
        {
            OperationState.SetStatus(validationError);
            EpubAnalysis.MarkFailed(validationError);
            return;
        }

        AnalyzeEpubFolderAndSaveRequest validatedRequest = request!;
        await ExecuteBusyAsync(
            $"Analyzing EPUB folder for corpus '{validatedRequest.CorpusName}'...",
            async token =>
            {
                AnalyzeEpubFolderAndSaveResult result = await EpubAnalysis
                    .ExecuteAsync(validatedRequest, token)
                    .ConfigureAwait(true);
                string reloadMessage = await ReloadRunsCoreAsync(result.AnalysisRun.Id, CancellationToken.None)
                    .ConfigureAwait(true);
                return $"Run {result.AnalysisRun.Id} completed. {reloadMessage}";
            },
            "EPUB analysis cancelled.",
            ex => $"EPUB analysis failed: {ex.Message}",
            ex => EpubAnalysis.MarkFailed($"EPUB analysis failed: {ex.Message}"),
            cancellationToken,
            EpubAnalysis.MarkCancelled,
            acceptCompletedOperationAfterCancellation: true).ConfigureAwait(true);
    }

    public async Task OpenLatestAnalysisOutputAsync(CancellationToken cancellationToken = default)
    {
        await OpenLatestAnalysisTargetAsync(
            EpubAnalysis.OpenOutputDirectoryAsync,
            "Could not open the latest analysis output folder",
            cancellationToken).ConfigureAwait(true);
    }

    public async Task OpenLatestAnalysisDiagnosticsAsync(CancellationToken cancellationToken = default)
    {
        await OpenLatestAnalysisTargetAsync(
            EpubAnalysis.OpenDiagnosticsAsync,
            "Could not open the latest import diagnostics",
            cancellationToken).ConfigureAwait(true);
    }

    public async Task OpenLatestAnalysisFailuresAsync(CancellationToken cancellationToken = default)
    {
        await OpenLatestAnalysisTargetAsync(
            EpubAnalysis.OpenFailuresAsync,
            "Could not open the latest import-failures CSV",
            cancellationToken).ConfigureAwait(true);
    }

    public async Task SelectRunAsync(
        RunListItemViewModel? runItem,
        CancellationToken cancellationToken = default)
    {
        await ExecuteBusyAsync(
            runItem is null ? "Clearing selection..." : $"Loading run {runItem.Id}...",
            async token =>
            {
                await SelectRunCoreAsync(runItem, token).ConfigureAwait(true);
                return runItem is null ? "No run selected." : $"Run {runItem.Id} loaded.";
            },
            "Run loading cancelled.",
            ex => $"Error loading run: {ex.Message}",
            null,
            cancellationToken).ConfigureAwait(true);
    }

    public void SetSelectedRunBook(RunBookListItemViewModel? book)
    {
        Books.SetSelectedRunBook(book);
        ChaptersExplorer.Clear(book is null
            ? "Select a source book to inspect its chapters."
            : "Load the selected book to inspect its chapters.");
    }

    public async Task SelectRunBookAsync(
        RunBookListItemViewModel? book,
        CancellationToken cancellationToken = default)
    {
        if (ReferenceEquals(book, SelectedRunBook))
        {
            return;
        }

        await ExecuteBusyAsync(
            book is null ? "Clearing book selection..." : $"Loading chapters for '{book.Book.Title}'...",
            async token =>
            {
                Books.SetSelectedRunBook(book);
                if (book is null)
                {
                    ChaptersExplorer.Clear("Select a source book to inspect its chapters.");
                    return "No source book selected.";
                }

                ChaptersExplorer.Clear("Loading persisted chapters...");
                return await ChaptersExplorer.LoadAsync(DatabasePath, book, token).ConfigureAwait(true);
            },
            "Chapter loading cancelled.",
            ex => $"Error loading chapters: {ex.Message}",
            ex => ChaptersExplorer.Clear($"Could not load chapters: {ex.Message}"),
            cancellationToken).ConfigureAwait(true);
    }

    public void SetSelectedChapter(ChapterListItemViewModel? chapter)
    {
        ChaptersExplorer.SetSelectedChapter(chapter);
    }

    public void SearchChapterText(string? searchText)
    {
        ChaptersExplorer.Search(searchText);
    }

    public void MoveToNextChapterMatch()
    {
        ChaptersExplorer.MoveToNextSearchMatch();
    }

    public void MoveToPreviousChapterMatch()
    {
        ChaptersExplorer.MoveToPreviousSearchMatch();
    }

    public async Task SearchWordAsync(
        string? wordText,
        CancellationToken cancellationToken = default)
    {
        string normalizedInput = wordText?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(normalizedInput))
        {
            WordExplorer.Clear("Enter a word to search in the selected run.");
            OperationState.SetStatus("Enter a word to search.");
            return;
        }

        if (SelectedRun is null)
        {
            WordExplorer.Clear("Select a run before searching a word.");
            OperationState.SetStatus("Select a run before searching a word.");
            return;
        }

        long runId = SelectedRun.Id;
        await ExecuteBusyAsync(
            $"Searching '{normalizedInput}' in run {runId}...",
            token => WordExplorer.SearchAsync(DatabasePath, runId, normalizedInput, token),
            "Word search cancelled.",
            ex => $"Error searching word: {ex.Message}",
            ex => WordExplorer.Clear($"Word search error: {ex.Message}"),
            cancellationToken).ConfigureAwait(true);
    }

    public void SetNGramSize(int? n)
    {
        NGramExplorer.SetSize(n);
    }

    public void SetNGramFilter(NGramExplorerFilter filter)
    {
        NGramExplorer.SetFilter(filter);
    }

    public void SetNGramSort(NGramExplorerSort sort)
    {
        NGramExplorer.SetSort(sort);
    }

    public async Task SearchNGramsAsync(
        string? searchTerm,
        string? minCountText,
        string? limitText,
        CancellationToken cancellationToken = default)
    {
        if (SelectedRun is null)
        {
            NGramExplorer.Clear("Select a run before searching n-grams.");
            OperationState.SetStatus("Select a run before searching n-grams.");
            return;
        }

        long runId = SelectedRun.Id;
        await ExecuteBusyAsync(
            $"Loading n-grams for run {runId}...",
            token => NGramExplorer.SearchAsync(
                DatabasePath,
                runId,
                searchTerm,
                minCountText,
                limitText,
                token),
            "N-gram search cancelled.",
            ex => $"Error searching n-grams: {ex.Message}",
            ex => NGramExplorer.Clear($"N-gram search error: {ex.Message}"),
            cancellationToken).ConfigureAwait(true);
    }

    public void SetCollocationFilter(CollocationExplorerFilter filter)
    {
        CollocationsExplorer.SetFilter(filter);
    }

    public async Task SearchCollocationsAsync(
        string? wordText,
        string? windowText,
        string? minCountText,
        string? minDiceText,
        string? limitText,
        CancellationToken cancellationToken = default)
    {
        string normalizedInput = wordText?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(normalizedInput))
        {
            CollocationsExplorer.Clear("Enter a word to search collocations.");
            OperationState.SetStatus("Enter a word to search collocations.");
            return;
        }

        if (SelectedRun is null)
        {
            CollocationsExplorer.Clear("Select a run before searching collocations.");
            OperationState.SetStatus("Select a run before searching collocations.");
            return;
        }

        long runId = SelectedRun.Id;
        await ExecuteBusyAsync(
            $"Loading collocations for '{normalizedInput}'...",
            token => CollocationsExplorer.SearchAsync(
                DatabasePath,
                runId,
                normalizedInput,
                windowText,
                minCountText,
                minDiceText,
                limitText,
                token),
            "Collocation search cancelled.",
            ex => $"Error searching collocations: {ex.Message}",
            ex => CollocationsExplorer.Clear($"Collocation search error: {ex.Message}"),
            cancellationToken).ConfigureAwait(true);
    }

    public void SetPhraseContentBoundary(bool enabled)
    {
        PhraseExplorer.SetContentBoundary(enabled);
    }

    public void SetPhraseLongestOnly(bool enabled)
    {
        PhraseExplorer.SetLongestOnly(enabled);
    }

    public async Task SearchPhrasesAsync(
        string? minNText,
        string? maxNText,
        string? minCountText,
        string? minChaptersText,
        string? limitText,
        CancellationToken cancellationToken = default)
    {
        if (SelectedRun is null)
        {
            PhraseExplorer.Clear("Select a run before searching phrases.");
            OperationState.SetStatus("Select a run before searching phrases.");
            return;
        }

        long runId = SelectedRun.Id;
        await ExecuteBusyAsync(
            $"Loading phrases for run {runId}...",
            token => PhraseExplorer.SearchAsync(
                DatabasePath,
                runId,
                minNText,
                maxNText,
                minCountText,
                minChaptersText,
                limitText,
                token),
            "Phrase search cancelled.",
            ex => $"Error searching phrases: {ex.Message}",
            ex => PhraseExplorer.Clear($"Phrase search error: {ex.Message}"),
            cancellationToken).ConfigureAwait(true);
    }

    public void SetSelectedArtifact(ArtifactListItemViewModel? artifact)
    {
        ArtifactExplorer.SetSelectedArtifact(artifact);
    }

    public async Task RefreshArtifactsAsync(CancellationToken cancellationToken = default)
    {
        if (SelectedRun is null)
        {
            ArtifactExplorer.Clear("Select a run to inspect reports and exports.");
            OperationState.SetStatus("Select a run before loading artifacts.");
            return;
        }

        long runId = SelectedRun.Id;
        await ExecuteBusyAsync(
            $"Loading reports and exports for run {runId}...",
            token => ArtifactExplorer.LoadAsync(DatabasePath, runId, token),
            "Artifact loading cancelled.",
            ex => $"Error loading artifacts: {ex.Message}",
            ex => ArtifactExplorer.Clear($"Could not load reports and exports: {ex.Message}"),
            cancellationToken).ConfigureAwait(true);
    }

    public async Task OpenSelectedArtifactAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            string message = await ArtifactExplorer.OpenSelectedAsync(cancellationToken).ConfigureAwait(true);
            OperationState.SetStatus(message);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            OperationState.SetStatus("Opening the artifact was cancelled.");
        }
        catch (Exception ex)
        {
            OperationState.SetStatus($"Could not open artifact: {ex.Message}");
        }
    }

    public async Task OpenArtifactOutputDirectoryAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            string message = await ArtifactExplorer.OpenOutputDirectoryAsync(cancellationToken).ConfigureAwait(true);
            OperationState.SetStatus(message);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            OperationState.SetStatus("Opening the output folder was cancelled.");
        }
        catch (Exception ex)
        {
            OperationState.SetStatus($"Could not open output folder: {ex.Message}");
        }
    }

    public void SetComparisonRightRun(RunListItemViewModel? run)
    {
        CompareRuns.SetRightRun(run);
    }

    public void SetComparisonWordFilter(ComparisonWordFilter filter)
    {
        CompareRuns.SetWordFilter(filter);
    }

    public void SetComparisonPresenceFilter(ComparisonPresenceFilter filter)
    {
        CompareRuns.SetPresenceFilter(filter);
    }

    public async Task CompareRunsAsync(
        string? wordText,
        string? minCountText,
        string? limitText,
        CancellationToken cancellationToken = default)
    {
        if (SelectedRun is null)
        {
            CompareRuns.Clear("Select a left run before comparing corpora.");
            OperationState.SetStatus("Select a left run before comparing corpora.");
            return;
        }

        if (ComparisonRightRun is null)
        {
            CompareRuns.Clear("Choose a right run before comparing corpora.");
            OperationState.SetStatus("Choose a right run before comparing corpora.");
            return;
        }

        if (SelectedRun.Id == ComparisonRightRun.Id)
        {
            CompareRuns.Clear("Choose two different runs.");
            OperationState.SetStatus("Choose two different runs to compare.");
            return;
        }

        RunListItemViewModel leftRun = SelectedRun;
        long rightRunId = ComparisonRightRun.Id;
        await ExecuteBusyAsync(
            $"Comparing run {leftRun.Id} and run {rightRunId}...",
            token => CompareRuns.CompareAsync(
                DatabasePath,
                leftRun,
                wordText,
                minCountText,
                limitText,
                token),
            "Run comparison cancelled.",
            ex => $"Error comparing runs: {ex.Message}",
            ex => CompareRuns.Clear($"Comparison error: {ex.Message}"),
            cancellationToken).ConfigureAwait(true);
    }

    public void CancelCurrentOperation()
    {
        _currentOperationCancellation?.Cancel();
    }

    private async Task<string> ReloadRunsCoreAsync(
        long? preferredRunId,
        CancellationToken cancellationToken)
    {
        string databasePath = DatabasePath;
        IReadOnlyList<StoredAnalysisRunSummary> runs = await _runLoader(databasePath, cancellationToken)
            .ConfigureAwait(true);
        cancellationToken.ThrowIfCancellationRequested();

        Runs.Clear();
        foreach (StoredAnalysisRunSummary run in runs)
        {
            Runs.Add(new RunListItemViewModel(run));
        }

        await Corpora.LoadAsync(databasePath, Runs, cancellationToken).ConfigureAwait(true);
        RebuildVisibleRuns();
        CompareRuns.EnsureDefaultRightRun(Runs, SelectedRun);
        if (VisibleRuns.Count == 0)
        {
            string message = Runs.Count == 0
                ? "The database was opened, but it does not contain analysis runs."
                : "The selected corpus does not contain analysis runs.";
            ClearSelectedRun(message);
            return $"Loaded {Math.Max(0, Corpora.Corpora.Count - 1):n0} corpus/corpora and {Runs.Count:n0} run(s).";
        }

        RunListItemViewModel runToSelect = preferredRunId is not null
            ? VisibleRuns.FirstOrDefault(run => run.Id == preferredRunId.Value) ?? VisibleRuns[0]
            : VisibleRuns[0];
        await SelectRunCoreAsync(runToSelect, cancellationToken).ConfigureAwait(true);
        return $"Loaded {Math.Max(0, Corpora.Corpora.Count - 1):n0} corpus/corpora and {Runs.Count:n0} run(s).";
    }

    private async Task OpenLatestAnalysisTargetAsync(
        Func<CancellationToken, Task<string>> openAction,
        string errorPrefix,
        CancellationToken cancellationToken)
    {
        try
        {
            string message = await openAction(cancellationToken).ConfigureAwait(true);
            OperationState.SetStatus(message);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            OperationState.SetStatus("Opening the latest analysis artifact was cancelled.");
        }
        catch (Exception ex)
        {
            OperationState.SetStatus($"{errorPrefix}: {ex.Message}");
        }
    }

    private async Task SelectRunCoreAsync(
        RunListItemViewModel? runItem,
        CancellationToken cancellationToken)
    {
        SelectedRun = runItem;
        if (runItem is null)
        {
            ClearSelectedRun("No run selected.");
            return;
        }

        StoredAnalysisRunSummary run = runItem.Summary;
        Dashboard.Prepare(run);
        Books.Clear("Loading source books...");
        ChaptersExplorer.Clear("Loading chapters after the source books...");
        WordExplorer.Clear("Search a word in the selected run.");
        NGramExplorer.Clear("Search n-grams in the selected run.");
        ArtifactExplorer.Clear("Loading reports and exports...");
        CollocationsExplorer.Clear("Search collocations in the selected run.");
        PhraseExplorer.Clear("Search phrases in the selected run.");
        CompareRuns.EnsureDefaultRightRun(Runs, runItem);
        CompareRuns.Clear("Choose a right run and compare corpora.");

        Task dashboardTask = Dashboard.LoadAsync(DatabasePath, run.Id, cancellationToken);
        Task booksTask = LoadBooksAndChaptersSafelyAsync(DatabasePath, run.Id, cancellationToken);
        Task artifactsTask = LoadArtifactsSafelyAsync(DatabasePath, run.Id, cancellationToken);
        await Task.WhenAll(dashboardTask, booksTask, artifactsTask).ConfigureAwait(true);
    }

    private async Task LoadArtifactsSafelyAsync(
        string databasePath,
        long runId,
        CancellationToken cancellationToken)
    {
        try
        {
            await ArtifactExplorer.LoadAsync(databasePath, runId, cancellationToken).ConfigureAwait(true);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            ArtifactExplorer.Clear($"Could not load reports and exports: {ex.Message}");
        }
    }

    private async Task LoadBooksAndChaptersSafelyAsync(
        string databasePath,
        long runId,
        CancellationToken cancellationToken)
    {
        try
        {
            await Books.LoadAsync(databasePath, runId, cancellationToken).ConfigureAwait(true);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            Books.Clear($"Could not load source books: {ex.Message}");
            ChaptersExplorer.Clear("Chapters cannot be loaded because the source-book list is unavailable.");
            return;
        }

        cancellationToken.ThrowIfCancellationRequested();
        if (Books.SelectedRunBook is null)
        {
            ChaptersExplorer.Clear("No source book is available for chapter browsing.");
            return;
        }

        try
        {
            await ChaptersExplorer
                .LoadAsync(databasePath, Books.SelectedRunBook, cancellationToken)
                .ConfigureAwait(true);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            ChaptersExplorer.Clear($"Could not load chapters: {ex.Message}");
        }
    }


    private void RebuildVisibleRuns()
    {
        VisibleRuns.Clear();
        long? corpusId = Corpora.SelectedCorpusId;
        foreach (RunListItemViewModel run in Runs)
        {
            if (corpusId is null || run.Summary.CorpusId == corpusId.Value)
            {
                VisibleRuns.Add(run);
            }
        }

        if (SelectedRun is not null && VisibleRuns.Contains(SelectedRun))
        {
            OnPropertyChanged(nameof(SelectedRun));
        }
    }

    private void ClearSelectedRun(string message)
    {
        SelectedRun = null;
        Dashboard.Clear(message);
        Books.Clear("Select a run to inspect its source books.");
        ChaptersExplorer.Clear("Select a source book to inspect its chapters.");
        WordExplorer.Clear("Select a run and search a word.");
        NGramExplorer.Clear("Select a run and search n-grams.");
        ArtifactExplorer.Clear("Select a run to inspect reports and exports.");
        CollocationsExplorer.Clear("Select a run and search collocations.");
        PhraseExplorer.Clear("Select a run and search phrases.");
        CompareRuns.EnsureDefaultRightRun(Runs, null);
        CompareRuns.Clear("Select a left run before comparing corpora.");
    }

    private async Task ExecuteBusyAsync(
        string startMessage,
        Func<CancellationToken, Task<string>> operation,
        string cancellationMessage,
        Func<Exception, string> errorMessage,
        Action<Exception>? onError,
        CancellationToken externalCancellationToken,
        Action? onCancelled = null,
        bool acceptCompletedOperationAfterCancellation = false)
    {
        using CancellationTokenSource linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(
            externalCancellationToken);
        long operationVersion = Interlocked.Increment(ref _operationVersion);
        CancellationTokenSource? previous = Interlocked.Exchange(
            ref _currentOperationCancellation,
            linkedCancellation);
        previous?.Cancel();

        OperationState.Begin(startMessage);
        try
        {
            string completionMessage = await operation(linkedCancellation.Token).ConfigureAwait(true);
            if (!acceptCompletedOperationAfterCancellation)
            {
                linkedCancellation.Token.ThrowIfCancellationRequested();
            }
            if (operationVersion == Volatile.Read(ref _operationVersion))
            {
                OperationState.SetStatus(completionMessage);
            }
        }
        catch (OperationCanceledException) when (linkedCancellation.IsCancellationRequested)
        {
            if (operationVersion == Volatile.Read(ref _operationVersion))
            {
                onCancelled?.Invoke();
                OperationState.SetStatus(cancellationMessage);
            }
        }
        catch (Exception ex)
        {
            if (operationVersion == Volatile.Read(ref _operationVersion))
            {
                onError?.Invoke(ex);
                OperationState.SetStatus(errorMessage(ex));
            }
        }
        finally
        {
            if (operationVersion == Volatile.Read(ref _operationVersion))
            {
                Interlocked.CompareExchange(
                    ref _currentOperationCancellation,
                    null,
                    linkedCancellation);
                OperationState.End();
            }
        }
    }

    private void ForwardPropertyChanges(INotifyPropertyChanged child)
    {
        child.PropertyChanged += (_, args) =>
        {
            if (!string.IsNullOrWhiteSpace(args.PropertyName))
            {
                OnPropertyChanged(args.PropertyName);
            }
        };
    }

    private static Task<IReadOnlyList<StoredAnalysisRunSummary>> LoadRunsFromApplicationAsync(
        string databasePath,
        CancellationToken cancellationToken)
    {
        return Task.Run(async () =>
        {
            AnalysisRunQueryService service = new(databasePath);
            return await service.ListRunsAsync(limit: 1000, cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        }, cancellationToken);
    }
}
