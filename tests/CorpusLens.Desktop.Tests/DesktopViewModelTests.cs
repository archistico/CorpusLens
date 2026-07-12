using CorpusLens.Application.Queries;
using CorpusLens.Application.Storage;
using CorpusLens.Desktop.ViewModels;
using CorpusLens.Domain.Analysis;
using CorpusLens.Domain.Books;
using CorpusLens.Domain.Storage;
using CorpusLens.Infrastructure.Storage;
using Xunit;

namespace CorpusLens.Desktop.Tests;

public sealed class DesktopViewModelTests
{
    [Fact]
    public async Task BooksExplorer_LoadsSummaryAndSelectsFirstBook()
    {
        StoredAnalysisRunBook[] books =
        {
            new(7, 11, 1, "First", "Author A", "it", "first.epub", "hash-1", 4, 1_000),
            new(7, 12, 2, "Second", "Author B", "it", "second.epub", "hash-2", 6, 2_000),
        };
        BooksExplorerViewModel viewModel = new((_, _, _) =>
            Task.FromResult<IReadOnlyList<StoredAnalysisRunBook>>(books));

        await viewModel.LoadAsync("corpuslens.db", 7);

        Assert.Equal(2, viewModel.RunBooks.Count);
        Assert.Same(viewModel.RunBooks[0], viewModel.SelectedRunBook);
        Assert.Contains("Source books: 2", viewModel.BooksExplorerSummary);
        Assert.Contains("Chapters: 10", viewModel.BooksExplorerSummary);
        Assert.Contains("Characters:", viewModel.BooksExplorerSummary);
        Assert.Contains("Title: First", viewModel.RunBookDetails);
    }

    [Fact]
    public async Task WordExplorer_UsesInjectedQueryAndFormatsResult()
    {
        StoredWordStatistic word = new(1, 3, 4, 5, "amore", 42, 8, 123.45, false);
        WordExplorerViewModel viewModel = new((request, _) =>
        {
            Assert.Equal(3, request.AnalysisRunId);
            Assert.Equal("amore", request.Word);
            return Task.FromResult(new WordExplorerResult(
                word,
                Array.Empty<StoredNextWordStatistic>(),
                Array.Empty<StoredNextWordStatistic>(),
                Array.Empty<StoredWordContext>(),
                Array.Empty<StoredWordBookStatistic>()));
        });

        string status = await viewModel.SearchAsync("corpuslens.db", 3, "amore");

        Assert.Equal("Word 'amore' loaded.", status);
        Assert.Equal("Word explorer — amore", viewModel.WordExplorerTitle);
        Assert.Contains("Type: content", viewModel.WordExplorerSummary);
        Assert.Contains("Count: 42", viewModel.WordExplorerSummary);
    }

    [Fact]
    public async Task CollocationsExplorer_PassesSelectedFilterToRequest()
    {
        CollocationExplorerRequest? capturedRequest = null;
        CollocationsExplorerViewModel viewModel = new((request, _) =>
        {
            capturedRequest = request;
            return Task.FromResult(new CollocationExplorerResult(
                request.Word,
                request.Window,
                request.Limit,
                request.MinCount,
                request.MinDice,
                request.Filter,
                1,
                new[] { new CollocationExplorerItem("grande", "content", 5, 2, 3, 0.25, 1.4, 0.08) }));
        });
        viewModel.SetFilter(CollocationExplorerFilter.ContentOnly);

        await viewModel.SearchAsync("corpuslens.db", 9, "casa", "4", "2", "0.05", "20");

        Assert.NotNull(capturedRequest);
        Assert.Equal(CollocationExplorerFilter.ContentOnly, capturedRequest!.Filter);
        Assert.Equal("Filter: content words only", viewModel.CollocationFilterLabel);
        Assert.Contains("grande", viewModel.CollocationResults);
    }

    [Fact]
    public async Task PhraseExplorer_PassesFlagsAndUpdatesSummary()
    {
        PhraseExplorerRequest? capturedRequest = null;
        PhraseExplorerViewModel viewModel = new((request, _) =>
        {
            capturedRequest = request;
            return Task.FromResult(new PhraseExplorerResult(
                request.MinN,
                request.MaxN,
                request.MinCount,
                request.MinChapters,
                request.Limit,
                request.ContentBoundaryOnly,
                request.LongestOnly,
                5,
                2,
                new[] { new PhraseExplorerItem("prima volta", 2, 12, 4, 44.2, "content/content") }));
        });
        viewModel.SetContentBoundary(true);
        viewModel.SetLongestOnly(true);

        await viewModel.SearchAsync("corpuslens.db", 4, "2", "5", "3", "2", "30");

        Assert.NotNull(capturedRequest);
        Assert.True(capturedRequest!.ContentBoundaryOnly);
        Assert.True(capturedRequest!.LongestOnly);
        Assert.Contains("Nested phrases: longest only", viewModel.PhraseExplorerSummary);
        Assert.Contains("prima volta", viewModel.PhraseResults);
    }

    [Fact]
    public void CompareRuns_SelectsDifferentDefaultRightRun()
    {
        RunListItemViewModel left = new(CreateRun(1, "Left"));
        RunListItemViewModel right = new(CreateRun(2, "Right"));
        CompareRunsViewModel viewModel = new();

        viewModel.EnsureDefaultRightRun(new[] { left, right }, left);

        Assert.Same(right, viewModel.ComparisonRightRun);
    }

    [Fact]
    public void OperationState_UpdatesBusyAndStatusTogether()
    {
        DesktopOperationStateViewModel state = new();

        state.Begin("Loading...");
        Assert.True(state.IsBusy);
        Assert.Equal("Loading...", state.StatusMessage);

        state.Complete("Done");
        Assert.False(state.IsBusy);
        Assert.Equal("Done", state.StatusMessage);
    }

    [Fact]
    public async Task ChapterExplorerQueryService_DerivesMetricsFromPersistedCleanText()
    {
        string directoryPath = Path.Combine(
            Path.GetTempPath(),
            $"corpuslens-chapter-explorer-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directoryPath);

        try
        {
            string databasePath = Path.Combine(directoryPath, "corpuslens.db");
            string sourcePath = Path.Combine(directoryPath, "book.epub");
            await File.WriteAllTextAsync(sourcePath, "fake epub content");

            SqliteCorpusStore store = new(databasePath);
            StoredCorpus corpus = await store.CreateCorpusAsync("Test corpus", "en");
            ImportedBook book = new(
                "book-1",
                "Test book",
                "Test author",
                "en",
                sourcePath,
                new[]
                {
                    new ImportedChapter(
                        1,
                        "Chapter one",
                        "chapter-1.xhtml",
                        string.Empty,
                        "One sentence. Another sentence!"),
                    new ImportedChapter(
                        2,
                        "Copyright",
                        "copyright.xhtml",
                        string.Empty,
                        "Project Gutenberg notice."),
                });
            StoredBookImport storedBook = await store.SaveImportedBookAsync(corpus.Id, book);

            ChapterExplorerQueryService service = new();
            ChapterExplorerResult result = await service.GetChaptersAsync(
                new ChapterExplorerRequest(databasePath, storedBook.Book.Id));

            Assert.Equal(2, result.Chapters.Count);
            ChapterExplorerItem first = result.Chapters[0];
            Assert.Equal(4, first.WordCount);
            Assert.Equal(2, first.SentenceCount);
            Assert.True(first.IsVeryShort);
            Assert.False(first.IsPotentiallySuspicious);

            ChapterExplorerItem second = result.Chapters[1];
            Assert.True(second.IsPotentiallySuspicious);
            Assert.Contains("potentially suspicious", second.QualityLabel);
        }
        finally
        {
            Directory.Delete(directoryPath, recursive: true);
        }
    }

    [Fact]
    public async Task ChaptersExplorer_LoadsOrderedChaptersAndSelectsFirstChapter()
    {
        ChapterExplorerItem second = CreateChapter(
            id: 22,
            orderIndex: 2,
            title: "Second",
            cleanText: "Second chapter. It has two sentences!",
            characterCount: 37,
            wordCount: 6,
            sentenceCount: 2,
            isVeryShort: true);
        ChapterExplorerItem first = CreateChapter(
            id: 21,
            orderIndex: 1,
            title: "First",
            cleanText: "First chapter text.",
            characterCount: 19,
            wordCount: 3,
            sentenceCount: 1,
            isVeryShort: true);
        ChaptersExplorerViewModel viewModel = new((request, _) =>
        {
            Assert.Equal(11, request.BookId);
            return Task.FromResult(new ChapterExplorerResult(request.BookId, new[] { second, first }));
        });
        RunBookListItemViewModel book = new(
            new StoredAnalysisRunBook(7, 11, 1, "Book", "Author", "it", "book.epub", "hash", 2, 56));

        string status = await viewModel.LoadAsync("corpuslens.db", book);

        Assert.Equal(2, viewModel.Chapters.Count);
        Assert.Equal(1, viewModel.Chapters[0].Chapter.OrderIndex);
        Assert.Same(viewModel.Chapters[0], viewModel.SelectedChapter);
        Assert.Equal("First chapter text.", viewModel.ChapterPreview);
        Assert.Contains("Quality warnings: 2", viewModel.ChapterExplorerSummary);
        Assert.Contains("Loaded 2 chapter(s)", status);
    }

    [Fact]
    public async Task ChaptersExplorer_SearchesPreviewAndWrapsBetweenMatches()
    {
        ChapterExplorerItem chapter = CreateChapter(
            id: 21,
            orderIndex: 1,
            title: "First",
            cleanText: "Casa grande, casa piccola, CASA.",
            characterCount: 32,
            wordCount: 5,
            sentenceCount: 1,
            isVeryShort: true);
        ChaptersExplorerViewModel viewModel = new((request, _) =>
            Task.FromResult(new ChapterExplorerResult(request.BookId, new[] { chapter })));
        RunBookListItemViewModel book = new(
            new StoredAnalysisRunBook(7, 11, 1, "Book", "Author", "it", "book.epub", "hash", 1, 32));
        await viewModel.LoadAsync("corpuslens.db", book);

        viewModel.Search("casa");
        Assert.Equal(0, viewModel.ChapterPreviewSelectionStart);
        Assert.Equal(4, viewModel.ChapterPreviewSelectionLength);
        Assert.Contains("Showing 1 of 3", viewModel.ChapterSearchSummary);

        viewModel.MoveToNextSearchMatch();
        Assert.Equal(13, viewModel.ChapterPreviewSelectionStart);
        Assert.Contains("Showing 2 of 3", viewModel.ChapterSearchSummary);

        viewModel.MoveToPreviousSearchMatch();
        Assert.Equal(0, viewModel.ChapterPreviewSelectionStart);
    }

    [Fact]
    public async Task NGramExplorer_PassesOptionsAndFormatsResults()
    {
        NGramExplorerRequest? capturedRequest = null;
        NGramExplorerViewModel viewModel = new((request, _) =>
        {
            capturedRequest = request;
            return Task.FromResult(new NGramExplorerResult(
                request.N,
                request.MinCount,
                request.Limit,
                request.SearchTerm?.Trim() ?? string.Empty,
                request.Filter,
                request.Sort,
                4,
                2,
                new[]
                {
                    new NGramExplorerItem("white rabbit", 2, 12, 3, 44.5, "C-C"),
                    new NGramExplorerItem("rabbit hole", 2, 8, 2, 29.7, "C-C"),
                }));
        });
        viewModel.SetSize(2);
        viewModel.SetFilter(NGramExplorerFilter.ContentBoundary);
        viewModel.SetSort(NGramExplorerSort.DocumentCount);

        string status = await viewModel.SearchAsync("corpuslens.db", 5, "rabbit", "3", "20");

        Assert.NotNull(capturedRequest);
        Assert.Equal(2, capturedRequest!.N);
        Assert.Equal(3, capturedRequest.MinCount);
        Assert.Equal(20, capturedRequest.Limit);
        Assert.Equal("rabbit", capturedRequest.SearchTerm);
        Assert.Equal(NGramExplorerFilter.ContentBoundary, capturedRequest.Filter);
        Assert.Equal(NGramExplorerSort.DocumentCount, capturedRequest.Sort);
        Assert.Equal("Loaded 2 n-gram(s).", status);
        Assert.Contains("Sort: document count", viewModel.NGramExplorerSummary);
        Assert.Contains("white rabbit", viewModel.NGramResults);
        Assert.Contains("C-C", viewModel.NGramResults);
    }

    [Fact]
    public async Task NGramExplorerQueryService_FiltersByTermAndWordPattern()
    {
        string directoryPath = Path.Combine(
            Path.GetTempPath(),
            $"corpuslens-ngram-explorer-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directoryPath);

        try
        {
            string databasePath = Path.Combine(directoryPath, "corpuslens.db");
            string sourcePath = Path.Combine(directoryPath, "book.epub");
            await File.WriteAllTextAsync(sourcePath, "fake epub content");

            SqliteCorpusStore store = new(databasePath);
            StoredCorpus corpus = await store.CreateCorpusAsync("Test corpus", "en");
            ImportedBook book = new(
                "book-1",
                "Test book",
                "Test author",
                "en",
                sourcePath,
                new[]
                {
                    new ImportedChapter(
                        1,
                        "Chapter one",
                        "chapter-1.xhtml",
                        string.Empty,
                        "The white rabbit went into the rabbit hole."),
                });
            StoredBookImport storedBook = await store.SaveImportedBookAsync(corpus.Id, book);
            CorpusAnalysisResult analysis = new(
                new CorpusSummary(1, 1, 9, 9, 7, 9.0, 4.0),
                Array.Empty<WordFrequency>(),
                new[]
                {
                    new NGramFrequency(2, "the rabbit", 14, 1, 155555.56),
                    new NGramFrequency(2, "white rabbit", 12, 1, 133333.33),
                    new NGramFrequency(2, "in the", 10, 1, 111111.11),
                    new NGramFrequency(2, "rabbit hole", 8, 1, 88888.89),
                    new NGramFrequency(3, "the white rabbit", 6, 1, 66666.67),
                },
                Array.Empty<NextWordFrequency>(),
                Array.Empty<AnalyzedSentence>());
            StoredAnalysisRun run = await store.SaveAnalysisRunAsync(
                corpus.Id,
                storedBook.Book.Id,
                new AnalysisSettings(),
                analysis,
                "report.md",
                "words.csv",
                "ngrams.csv",
                "next_words.csv",
                "extracted_text.txt");

            NGramExplorerQueryService service = new();
            NGramExplorerResult result = await service.GetNGramsAsync(new NGramExplorerRequest(
                databasePath,
                run.Id,
                2,
                1,
                20,
                "RABBIT",
                NGramExplorerFilter.ContentBoundary,
                NGramExplorerSort.Count));

            Assert.Equal(3, result.FetchedCount);
            Assert.Equal(2, result.MatchedCount);
            Assert.Collection(
                result.NGrams,
                first =>
                {
                    Assert.Equal("white rabbit", first.Text);
                    Assert.Equal("C-C", first.WordPattern);
                },
                second => Assert.Equal("rabbit hole", second.Text));
        }
        finally
        {
            Directory.Delete(directoryPath, recursive: true);
        }
    }

    [Fact]
    public async Task ArtifactExplorerQueryService_ResolvesRelativePathsAndClassifiesAvailability()
    {
        string projectDirectory = Path.Combine(
            Path.GetTempPath(),
            $"corpuslens-artifacts-{Guid.NewGuid():N}");
        string dataDirectory = Path.Combine(projectDirectory, "data");
        string artifactDirectory = Path.Combine(projectDirectory, "artifacts", "run-1");
        Directory.CreateDirectory(dataDirectory);
        Directory.CreateDirectory(artifactDirectory);

        try
        {
            string databasePath = Path.Combine(dataDirectory, "corpuslens.db");
            string sourcePath = Path.Combine(projectDirectory, "book.epub");
            await File.WriteAllTextAsync(sourcePath, "fake epub content");
            await File.WriteAllTextAsync(Path.Combine(artifactDirectory, "report.md"), "# Report");
            await File.WriteAllTextAsync(Path.Combine(artifactDirectory, "next_words.csv"), "word,next_word");
            await File.WriteAllTextAsync(Path.Combine(artifactDirectory, "import_diagnostics.md"), "# Diagnostics");

            SqliteCorpusStore store = new(databasePath);
            StoredCorpus corpus = await store.CreateCorpusAsync("Test corpus", "en");
            ImportedBook book = new(
                "book-1",
                "Test book",
                "Test author",
                "en",
                sourcePath,
                new[]
                {
                    new ImportedChapter(
                        1,
                        "Chapter one",
                        "chapter-1.xhtml",
                        string.Empty,
                        "One short chapter."),
                });
            StoredBookImport storedBook = await store.SaveImportedBookAsync(corpus.Id, book);
            CorpusAnalysisResult analysis = new(
                new CorpusSummary(1, 1, 3, 3, 3, 3.0, 4.0),
                Array.Empty<WordFrequency>(),
                Array.Empty<NGramFrequency>(),
                Array.Empty<NextWordFrequency>(),
                Array.Empty<AnalyzedSentence>());
            string relativeOutput = Path.Combine("artifacts", "run-1");
            StoredAnalysisRun run = await store.SaveAnalysisRunAsync(
                corpus.Id,
                storedBook.Book.Id,
                new AnalysisSettings(),
                analysis,
                Path.Combine(relativeOutput, "report.md"),
                Path.Combine(relativeOutput, "words.csv"),
                string.Empty,
                Path.Combine(relativeOutput, "next_words.csv"),
                string.Empty);

            ArtifactExplorerQueryService service = new();
            ArtifactExplorerResult result = await service.GetArtifactsAsync(
                new ArtifactExplorerRequest(databasePath, run.Id));

            Assert.Equal(Path.GetFullPath(artifactDirectory), Path.GetFullPath(result.OutputDirectory));
            Assert.True(result.OutputDirectoryExists);
            Assert.Equal(3, result.AvailableCount);
            Assert.Equal(1, result.MissingCount);
            Assert.Equal(3, result.NotGeneratedCount);

            ArtifactExplorerItem report = result.Artifacts.Single(item => item.Id == "report");
            Assert.Equal(ArtifactAvailability.Available, report.Availability);
            Assert.Equal(
                Path.GetFullPath(Path.Combine(artifactDirectory, "report.md")),
                Path.GetFullPath(report.ResolvedPath));

            ArtifactExplorerItem words = result.Artifacts.Single(item => item.Id == "words");
            Assert.Equal(ArtifactAvailability.Missing, words.Availability);
            Assert.True(words.IsPathRecorded);
            Assert.Equal(
                Path.GetFullPath(Path.Combine(artifactDirectory, "words.csv")),
                Path.GetFullPath(words.ResolvedPath));

            ArtifactExplorerItem ngrams = result.Artifacts.Single(item => item.Id == "ngrams");
            Assert.Equal(ArtifactAvailability.NotGenerated, ngrams.Availability);
            Assert.False(ngrams.IsPathRecorded);

            ArtifactExplorerItem diagnostics = result.Artifacts.Single(
                item => item.Id == "import-diagnostics");
            Assert.Equal(ArtifactAvailability.Available, diagnostics.Availability);
            Assert.True(diagnostics.IsOptional);
        }
        finally
        {
            Directory.Delete(projectDirectory, recursive: true);
        }
    }

    [Fact]
    public async Task ArtifactExplorer_LoadsSelectsAndOpensOnlyAvailableTargets()
    {
        List<(string Path, bool IsDirectory)> openedTargets = new();
        ArtifactExplorerResult result = new(
            12,
            Path.GetFullPath("artifacts/run-12"),
            true,
            new[]
            {
                new ArtifactExplorerItem(
                    "report",
                    "Markdown report",
                    "report.md",
                    "report.md",
                    Path.GetFullPath("artifacts/run-12/report.md"),
                    ArtifactAvailability.Available,
                    false,
                    true),
                new ArtifactExplorerItem(
                    "words",
                    "Word frequencies CSV",
                    "words.csv",
                    "words.csv",
                    Path.GetFullPath("artifacts/run-12/words.csv"),
                    ArtifactAvailability.Missing,
                    false,
                    true),
            });
        ArtifactExplorerViewModel viewModel = new(
            (_, _) => Task.FromResult(result),
            (path, isDirectory, _) =>
            {
                openedTargets.Add((path, isDirectory));
                return Task.CompletedTask;
            });

        string loadStatus = await viewModel.LoadAsync("corpuslens.db", 12);

        Assert.Equal("Loaded 2 artifact entries.", loadStatus);
        Assert.Equal(2, viewModel.Artifacts.Count);
        Assert.NotNull(viewModel.SelectedArtifact);
        Assert.Equal("report", viewModel.SelectedArtifact!.Artifact.Id);
        Assert.True(viewModel.CanOpenSelectedArtifact);
        Assert.True(viewModel.CanOpenOutputDirectory);
        Assert.Contains("Available: 1", viewModel.ArtifactExplorerSummary);

        string openStatus = await viewModel.OpenSelectedAsync();
        string folderStatus = await viewModel.OpenOutputDirectoryAsync();

        Assert.Equal("Opened Markdown report.", openStatus);
        Assert.Equal("Opened the run output folder.", folderStatus);
        Assert.Collection(
            openedTargets,
            target =>
            {
                Assert.False(target.IsDirectory);
                Assert.EndsWith("report.md", target.Path, StringComparison.OrdinalIgnoreCase);
            },
            target =>
            {
                Assert.True(target.IsDirectory);
                Assert.Equal(result.OutputDirectory, target.Path);
            });

        viewModel.SetSelectedArtifact(viewModel.Artifacts[1]);
        string missingStatus = await viewModel.OpenSelectedAsync();

        Assert.False(viewModel.CanOpenSelectedArtifact);
        Assert.Contains("recorded file is missing", missingStatus);
        Assert.Equal(2, openedTargets.Count);
    }


    [Fact]
    public void CorpusLanguageCatalog_NormalizesSupportedRegionalCodes()
    {
        Assert.True(CorpusLanguageCatalog.TryNormalizeSupportedCode("IT-it", out string italian));
        Assert.Equal("it", italian);
        Assert.False(CorpusLanguageCatalog.TryNormalizeSupportedCode("es", out _));
        Assert.Equal(4, CorpusLanguageCatalog.ListSupportedLanguages().Count);
    }

    [Fact]
    public async Task CorpusManagement_LoadsCorporaWithRunCountsAndFiltersLanguage()
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        StoredCorpus[] storedCorpora =
        {
            new(10, "English", "en", "English books", now, now),
            new(11, "Italian", "it", "Italian books", now, now),
        };
        CorpusManagementViewModel viewModel = new(
            (_, _) => Task.FromResult<IReadOnlyList<StoredCorpus>>(storedCorpora));
        RunListItemViewModel[] runs =
        {
            new(CreateRun(1, 10, "English")),
            new(CreateRun(2, 10, "English")),
            new(CreateRun(3, 11, "Italian")),
        };

        await viewModel.LoadAsync("corpuslens.db", runs);

        Assert.Equal(3, viewModel.Corpora.Count);
        Assert.True(viewModel.Corpora[0].IsAllCorpora);
        Assert.Equal(3, viewModel.Corpora[0].RunCount);
        Assert.Equal(2, viewModel.Corpora.Single(item => item.Id == 10).RunCount);
        Assert.Equal(1, viewModel.Corpora.Single(item => item.Id == 11).RunCount);
        Assert.Contains("Corpora: 2", viewModel.Summary);
        Assert.False(viewModel.TryValidateCreateInput(
            " italian ",
            "it",
            out _,
            out _,
            out string duplicateError));
        Assert.Contains("already exists", duplicateError);

        CorpusListItemViewModel italian = viewModel.Corpora.Single(item => item.Id == 11);
        viewModel.SetSelectedCorpus(italian);

        Assert.True(viewModel.IsSelectedCorpusLanguageCompatible("it-IT"));
        Assert.False(viewModel.IsSelectedCorpusLanguageCompatible("en"));
        Assert.Contains("Language: it", viewModel.Details);
    }

    [Fact]
    public async Task CorpusManagement_CreatesNormalizedCorpusAndSelectsIt()
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        CreateCorpusRequest? capturedRequest = null;
        CorpusManagementViewModel viewModel = new(
            (_, _) => Task.FromResult<IReadOnlyList<StoredCorpus>>(Array.Empty<StoredCorpus>()),
            (request, _) =>
            {
                capturedRequest = request;
                return Task.FromResult(new StoredCorpus(
                    42,
                    request.Name,
                    request.LanguageCode,
                    request.Description ?? string.Empty,
                    now,
                    now));
            });
        await viewModel.LoadAsync("corpuslens.db", Array.Empty<RunListItemViewModel>());

        StoredCorpus created = await viewModel.CreateAsync(
            "corpuslens.db",
            "  French classics  ",
            "fr-FR",
            "  Public-domain corpus  ",
            Array.Empty<RunListItemViewModel>());

        Assert.NotNull(capturedRequest);
        Assert.Equal("French classics", capturedRequest!.Name);
        Assert.Equal("fr", capturedRequest.LanguageCode);
        Assert.Equal("Public-domain corpus", capturedRequest.Description);
        Assert.Equal(42, created.Id);
        Assert.Equal(2, viewModel.Corpora.Count);
        Assert.Equal(42, viewModel.SelectedCorpusId);
        Assert.False(viewModel.SelectedCorpus!.IsAllCorpora);
    }

    [Fact]
    public async Task CreateCorpusUseCase_RejectsUnsupportedLanguageBeforeWritingDatabase()
    {
        string directoryPath = Path.Combine(
            Path.GetTempPath(),
            $"corpuslens-unsupported-language-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directoryPath);
        string databasePath = Path.Combine(directoryPath, "corpuslens.db");
        try
        {
            CreateCorpusUseCase useCase = new();

            ArgumentException exception = await Assert.ThrowsAsync<ArgumentException>(
                () => useCase.ExecuteAsync(new CreateCorpusRequest(
                    databasePath,
                    "Spanish",
                    "es",
                    null)));

            Assert.Contains("Unsupported corpus language", exception.Message);
            Assert.False(File.Exists(databasePath));
        }
        finally
        {
            Directory.Delete(directoryPath, recursive: true);
        }
    }

    [Fact]
    public async Task MainWindow_CreateCorpusRequiresExplicitConfirmation()
    {
        string databasePath = Path.GetTempFileName();
        bool creatorCalled = false;
        try
        {
            CorpusManagementViewModel corpora = new(
                (_, _) => Task.FromResult<IReadOnlyList<StoredCorpus>>(Array.Empty<StoredCorpus>()),
                (_, _) =>
                {
                    creatorCalled = true;
                    return Task.FromException<StoredCorpus>(
                        new InvalidOperationException("Creator should not be called."));
                });
            MainWindowViewModel viewModel = new(
                corpora: corpora,
                runLoader: (_, _) => Task.FromResult<IReadOnlyList<StoredAnalysisRunSummary>>(
                    Array.Empty<StoredAnalysisRunSummary>()));
            await viewModel.OpenDatabaseAsync(databasePath);

            await viewModel.CreateCorpusAsync("Test", "en", null, confirmed: false);

            Assert.False(creatorCalled);
            Assert.Contains("Confirm the persistent write", viewModel.StatusMessage);
        }
        finally
        {
            File.Delete(databasePath);
        }
    }

    private static ChapterExplorerItem CreateChapter(
        long id,
        int orderIndex,
        string title,
        string cleanText,
        int characterCount,
        int wordCount,
        int sentenceCount,
        bool isVeryShort = false)
    {
        return new ChapterExplorerItem(
            id,
            11,
            orderIndex,
            title,
            $"chapter-{orderIndex}.xhtml",
            cleanText,
            characterCount,
            wordCount,
            sentenceCount,
            false,
            isVeryShort,
            false,
            false);
    }

    private static StoredAnalysisRunSummary CreateRun(long id, string corpusName)
    {
        return CreateRun(id, 10, corpusName);
    }

    private static StoredAnalysisRunSummary CreateRun(long id, long corpusId, string corpusName)
    {
        return new StoredAnalysisRunSummary(
            id,
            corpusId,
            corpusName,
            20,
            "EPUB folder: it",
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow,
            "Completed",
            100,
            1_000,
            900,
            400,
            12.3,
            4.8,
            "report.md");
    }
}
