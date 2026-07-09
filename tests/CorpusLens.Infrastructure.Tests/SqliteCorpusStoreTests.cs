using CorpusLens.Domain.Analysis;
using CorpusLens.Domain.Books;
using CorpusLens.Domain.Storage;
using CorpusLens.Infrastructure.Storage;
using Xunit;

namespace CorpusLens.Infrastructure.Tests;

public sealed class SqliteCorpusStoreTests
{
    [Fact]
    public async Task CreateCorpusAsync_ShouldCreateDatabaseAndCorpus()
    {
        using TestDatabase database = new();
        SqliteCorpusStore store = new(database.Path);

        StoredCorpus corpus = await store.CreateCorpusAsync("English Kids", "en", "Children books");
        IReadOnlyList<StoredCorpus> corpora = await store.ListCorporaAsync();

        Assert.True(File.Exists(database.Path));
        Assert.True(corpus.Id > 0);
        StoredCorpus storedCorpus = Assert.Single(corpora, item => item.Name == "English Kids");
        Assert.Equal("en", storedCorpus.LanguageCode);
        Assert.Equal("Children books", storedCorpus.Description);
    }

    [Fact]
    public async Task SaveImportedBookAsync_ShouldPersistBookAndChapters()
    {
        using TestDatabase database = new();
        string sourceFile = database.CreateSourceFile("alice.epub", "fake epub content");
        SqliteCorpusStore store = new(database.Path);
        StoredCorpus corpus = await store.CreateCorpusAsync("English Kids", "en");

        ImportedBook book = new(
            "book-1",
            "Alice",
            "Lewis Carroll",
            "en",
            sourceFile,
            new[]
            {
                new ImportedChapter(1, "Chapter I", "chapter1.xhtml", "<p>Hello</p>", "Hello, Alice."),
                new ImportedChapter(2, "Chapter II", "chapter2.xhtml", "<p>Bye</p>", "Bye, Alice.")
            });

        StoredBookImport storedImport = await store.SaveImportedBookAsync(corpus.Id, book);
        IReadOnlyList<StoredChapter> chapters = await store.ListChaptersAsync(storedImport.Book.Id);

        Assert.True(storedImport.Book.Id > 0);
        Assert.Equal("Alice", storedImport.Book.Title);
        Assert.Equal("Lewis Carroll", storedImport.Book.Author);
        Assert.Equal(64, storedImport.Book.FileHash.Length);
        Assert.Equal(2, chapters.Count);
        Assert.Equal("Chapter I", chapters[0].Title);
        Assert.Equal("Hello, Alice.", chapters[0].CleanText);
        Assert.Equal("Chapter II", chapters[1].Title);
    }

    [Fact]
    public async Task SaveAnalysisRunAsync_ShouldPersistSummaryAndReportPaths()
    {
        using TestDatabase database = new();
        string sourceFile = database.CreateSourceFile("alice.epub", "fake epub content");
        SqliteCorpusStore store = new(database.Path);
        StoredCorpus corpus = await store.CreateCorpusAsync("English Kids", "en");

        ImportedBook book = new(
            "book-1",
            "Alice",
            "Lewis Carroll",
            "en",
            sourceFile,
            new[] { new ImportedChapter(1, "Chapter I", "chapter1.xhtml", string.Empty, "Hello, Alice.") });

        StoredBookImport storedImport = await store.SaveImportedBookAsync(corpus.Id, book);
        CorpusAnalysisResult analysis = new(
            new CorpusSummary(1, 2, 6, 4, 3, 2.0, 4.5),
            Array.Empty<WordFrequency>(),
            Array.Empty<NGramFrequency>(),
            Array.Empty<NextWordFrequency>(),
            Array.Empty<AnalyzedSentence>());

        StoredAnalysisRun run = await store.SaveAnalysisRunAsync(
            corpus.Id,
            storedImport.Book.Id,
            new AnalysisSettings(),
            analysis,
            "report.md",
            "words.csv",
            "ngrams.csv",
            "next_words.csv",
            "extracted_text.txt");

        Assert.True(run.Id > 0);
        Assert.Equal(corpus.Id, run.CorpusId);
        Assert.Equal(storedImport.Book.Id, run.BookId);
        Assert.Equal(2, run.SentenceCount);
        Assert.Equal(6, run.TokenCount);
        Assert.Equal("report.md", run.ReportPath);
        Assert.Equal("Completed", run.Status);
    }

    private sealed class TestDatabase : IDisposable
    {
        private readonly string _directoryPath;

        public TestDatabase()
        {
            _directoryPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "CorpusLensTests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_directoryPath);
            Path = System.IO.Path.Combine(_directoryPath, "corpuslens.db");
        }

        public string Path { get; }

        public string CreateSourceFile(string fileName, string content)
        {
            string filePath = System.IO.Path.Combine(_directoryPath, fileName);
            File.WriteAllText(filePath, content);
            return filePath;
        }

        public void Dispose()
        {
            if (Directory.Exists(_directoryPath))
            {
                Directory.Delete(_directoryPath, recursive: true);
            }
        }
    }
}
