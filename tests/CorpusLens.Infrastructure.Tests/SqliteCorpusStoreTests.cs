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

    [Fact]
    public async Task SaveAnalysisRunAsync_ShouldPersistStatistics()
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
            new CorpusSummary(1, 3, 12, 9, 5, 3.0, 4.0),
            new[]
            {
                new WordFrequency("alice", 4, 1, 444444.44),
                new WordFrequency("hello", 2, 1, 222222.22)
            },
            new[]
            {
                new NGramFrequency(2, "hello alice", 2, 1, 222222.22),
                new NGramFrequency(3, "hello dear alice", 1, 1, 111111.11)
            },
            new[]
            {
                new NextWordFrequency("hello", "alice", 2, 1.0),
                new NextWordFrequency("dear", "alice", 1, 0.5)
            },
            new[]
            {
                new AnalyzedSentence("Hello, Alice.", "hello, alice.", PhraseCategory.Greeting, 2),
                new AnalyzedSentence("Do you know Alice?", "do you know alice?", PhraseCategory.Question, 4),
                new AnalyzedSentence("Alice smiled.", "alice smiled.", PhraseCategory.Statement, 2)
            });

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

        IReadOnlyList<StoredWordStatistic> words = await store.ListTopWordsAsync(run.Id);
        IReadOnlyList<StoredNGramStatistic> ngrams = await store.ListTopNGramsAsync(run.Id);
        IReadOnlyList<StoredNextWordStatistic> nextWords = await store.ListTopNextWordsAsync(run.Id);
        IReadOnlyList<StoredSentenceCategoryStatistic> categories = await store.ListSentenceCategoryStatisticsAsync(run.Id);

        StoredWordStatistic alice = Assert.Single(words, word => word.Word == "alice");
        Assert.Equal(4, alice.Count);

        StoredNGramStatistic helloAlice = Assert.Single(ngrams, ngram => ngram.Text == "hello alice");
        Assert.Equal(2, helloAlice.N);
        Assert.Equal(2, helloAlice.Count);

        StoredNextWordStatistic nextWord = Assert.Single(nextWords, item => item.Word == "hello" && item.NextWord == "alice");
        Assert.Equal(1.0, nextWord.Probability, precision: 6);

        Assert.Contains(categories, category => category.Category == PhraseCategory.Greeting && category.Count == 1);
        Assert.Contains(categories, category => category.Category == PhraseCategory.Question && category.Count == 1);
        Assert.Contains(categories, category => category.Category == PhraseCategory.Statement && category.Count == 1);
    }


    [Fact]
    public async Task SaveImportedBookAsync_ShouldPersistAggregateFolderBook()
    {
        using TestDatabase database = new();
        string folderPath = database.CreateSourceFolder("books");
        database.CreateSourceFile(System.IO.Path.Combine("books", "first.epub"), "first fake epub content");
        database.CreateSourceFile(System.IO.Path.Combine("books", "second.epub"), "second fake epub content");

        SqliteCorpusStore store = new(database.Path);
        StoredCorpus corpus = await store.CreateCorpusAsync("English Literature", "en");

        ImportedBook book = new(
            "folder-books",
            "EPUB folder: books",
            string.Empty,
            "en",
            folderPath,
            new[]
            {
                new ImportedChapter(1, "First — Chapter I", "first.epub::chapter1.xhtml", string.Empty, "First text."),
                new ImportedChapter(2, "Second — Chapter I", "second.epub::chapter1.xhtml", string.Empty, "Second text.")
            });

        StoredBookImport storedImport = await store.SaveImportedBookAsync(corpus.Id, book);

        Assert.Equal("EPUB folder: books", storedImport.Book.Title);
        Assert.Equal(64, storedImport.Book.FileHash.Length);
        Assert.Equal(2, storedImport.Chapters.Count);
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
            string? directoryPath = System.IO.Path.GetDirectoryName(filePath);
            if (!string.IsNullOrWhiteSpace(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            File.WriteAllText(filePath, content);
            return filePath;
        }

        public string CreateSourceFolder(string folderName)
        {
            string folderPath = System.IO.Path.Combine(_directoryPath, folderName);
            Directory.CreateDirectory(folderPath);
            return folderPath;
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
