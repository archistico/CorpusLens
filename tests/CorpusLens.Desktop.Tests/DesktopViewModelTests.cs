using CorpusLens.Application.Queries;
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
        return new StoredAnalysisRunSummary(
            id,
            10,
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
