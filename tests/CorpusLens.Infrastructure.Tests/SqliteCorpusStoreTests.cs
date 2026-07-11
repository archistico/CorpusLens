using CorpusLens.Domain.Analysis;
using CorpusLens.Domain.Books;
using CorpusLens.Domain.Storage;
using CorpusLens.Infrastructure.Storage;
using Microsoft.Data.Sqlite;
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
                new WordFrequency("alice", 4, 1, 444444.44, false),
                new WordFrequency("the", 3, 1, 333333.33, true),
                new WordFrequency("hello", 2, 1, 222222.22, false)
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
        StoredWordStatistic the = Assert.Single(words, word => word.Word == "the");
        StoredWordStatistic? aliceDetail = await store.GetWordStatisticAsync(run.Id, "ALICE");
        StoredWordStatistic? missingDetail = await store.GetWordStatisticAsync(run.Id, "missing");
        IReadOnlyList<StoredWordStatistic> contentWords = await store.ListTopWordsAsync(run.Id, filter: StoredWordFilter.ContentOnly);
        IReadOnlyList<StoredWordStatistic> functionWords = await store.ListTopWordsAsync(run.Id, filter: StoredWordFilter.FunctionOnly);

        Assert.Equal(4, alice.Count);
        Assert.False(alice.IsStopWord);
        Assert.True(the.IsStopWord);
        Assert.NotNull(aliceDetail);
        Assert.Equal(alice.Id, aliceDetail!.Id);
        Assert.Null(missingDetail);
        Assert.DoesNotContain(contentWords, word => word.IsStopWord);
        Assert.All(functionWords, word => Assert.True(word.IsStopWord));

        StoredNGramStatistic helloAlice = Assert.Single(ngrams, ngram => ngram.Text == "hello alice");
        Assert.Equal(2, helloAlice.N);
        Assert.Equal(2, helloAlice.Count);

        StoredNextWordStatistic nextWord = Assert.Single(nextWords, item => item.Word == "hello" && item.NextWord == "alice");
        IReadOnlyList<StoredNextWordStatistic> previousWords = await store.ListPreviousWordsAsync(run.Id, "alice");

        Assert.Equal(1.0, nextWord.Probability, precision: 6);
        Assert.Contains(previousWords, item => item.Word == "hello" && item.NextWord == "alice");
        Assert.Contains(previousWords, item => item.Word == "dear" && item.NextWord == "alice");

        Assert.Contains(categories, category => category.Category == PhraseCategory.Greeting && category.Count == 1);
        Assert.Contains(categories, category => category.Category == PhraseCategory.Question && category.Count == 1);
        Assert.Contains(categories, category => category.Category == PhraseCategory.Statement && category.Count == 1);
    }


    [Fact]
    public async Task SaveAnalysisRunAsync_ShouldPersistTokenIndex()
    {
        using TestDatabase database = new();
        string sourceFile = database.CreateSourceFile("alice.epub", "fake epub content");
        SqliteCorpusStore store = new(database.Path);
        StoredCorpus corpus = await store.CreateCorpusAsync("English Literature", "en");

        ImportedBook book = new(
            "book-1",
            "Alice",
            "Lewis Carroll",
            "en",
            sourceFile,
            new[]
            {
                new ImportedChapter(1, "Chapter I", "chapter1.xhtml", string.Empty, "Hello, Alice. The rabbit saw Alice.")
            });

        StoredBookImport storedImport = await store.SaveImportedBookAsync(corpus.Id, book);
        CorpusAnalysisResult analysis = new(
            new CorpusSummary(1, 2, 8, 6, 5, 3.0, 4.0),
            new[]
            {
                new WordFrequency("alice", 2, 1, 333333.33, false),
                new WordFrequency("hello", 1, 1, 166666.67, false),
                new WordFrequency("the", 1, 1, 166666.67, true),
                new WordFrequency("rabbit", 1, 1, 166666.67, false),
                new WordFrequency("saw", 1, 1, 166666.67, false)
            },
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

        StoredTokenIndexSummary? summary = await store.GetTokenIndexSummaryAsync(run.Id);
        IReadOnlyList<StoredTokenOccurrence> aliceOccurrences = await store.ListTokenOccurrencesAsync(run.Id, "ALICE", limit: 10);

        Assert.NotNull(summary);
        Assert.Equal(6, summary!.TokenCount);
        Assert.Equal(6, summary.WordTokenCount);
        Assert.Equal(5, summary.DistinctTokenCount);
        Assert.Equal(1, summary.StopWordTokenCount);
        Assert.Equal(5, summary.ContentTokenCount);
        Assert.Equal(1, summary.ChapterCount);
        Assert.Equal(2, aliceOccurrences.Count);
        Assert.All(aliceOccurrences, occurrence =>
        {
            Assert.Equal(run.Id, occurrence.AnalysisRunId);
            Assert.Equal("alice", occurrence.NormalizedToken);
            Assert.False(occurrence.IsStopWord);
            Assert.True(occurrence.IsWord);
        });
        Assert.Equal(2, aliceOccurrences[0].ChapterPosition);
        Assert.Equal(6, aliceOccurrences[1].ChapterPosition);
    }


    [Fact]
    public async Task ListWordContextsAsync_ShouldUseTokenIndexWhenAvailable()
    {
        using TestDatabase database = new();
        string sourceFile = database.CreateSourceFile("alice.epub", "fake epub content");
        SqliteCorpusStore store = new(database.Path);
        StoredCorpus corpus = await store.CreateCorpusAsync("English Literature", "en");

        ImportedBook book = new(
            "book-1",
            "Alice",
            "Lewis Carroll",
            "en",
            sourceFile,
            new[]
            {
                new ImportedChapter(1, "Chapter I", "chapter1.xhtml", string.Empty, "Hello, Alice. Rabbit saw Alice.")
            });

        StoredBookImport storedImport = await store.SaveImportedBookAsync(corpus.Id, book);
        CorpusAnalysisResult analysis = new(
            new CorpusSummary(1, 2, 8, 5, 4, 2.5, 4.0),
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

        await DeleteTokenOccurrencesAsync(database.Path, "alice");

        IReadOnlyList<StoredWordContext> contexts = await store.ListWordContextsAsync(run.Id, "alice", limit: 3, contextWords: 2);

        Assert.Empty(contexts);
    }

    [Fact]
    public async Task ListWordContextsAsync_ShouldFallBackToStoredChaptersWhenTokenIndexIsMissing()
    {
        using TestDatabase database = new();
        string sourceFile = database.CreateSourceFile("alice.epub", "fake epub content");
        SqliteCorpusStore store = new(database.Path);
        StoredCorpus corpus = await store.CreateCorpusAsync("English Literature", "en");

        ImportedBook book = new(
            "book-1",
            "Alice",
            "Lewis Carroll",
            "en",
            sourceFile,
            new[]
            {
                new ImportedChapter(1, "Chapter I", "chapter1.xhtml", string.Empty, "Hello, Alice. Rabbit saw Alice.")
            });

        StoredBookImport storedImport = await store.SaveImportedBookAsync(corpus.Id, book);
        CorpusAnalysisResult analysis = new(
            new CorpusSummary(1, 2, 8, 5, 4, 2.5, 4.0),
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

        await DeleteAllTokenOccurrencesAsync(database.Path);

        IReadOnlyList<StoredWordContext> contexts = await store.ListWordContextsAsync(run.Id, "alice", limit: 3, contextWords: 2);

        Assert.Equal(2, contexts.Count);
        Assert.All(contexts, context => Assert.Equal("Alice", context.MatchText));
    }


    [Fact]
    public async Task ListWordContextsAsync_ShouldReturnKwicContextsFromStoredChapters()
    {
        using TestDatabase database = new();
        string sourceFile = database.CreateSourceFile("alice.epub", "fake epub content");
        SqliteCorpusStore store = new(database.Path);
        StoredCorpus corpus = await store.CreateCorpusAsync("English Literature", "en");

        ImportedBook book = new(
            "book-1",
            "Alice",
            "Lewis Carroll",
            "en",
            sourceFile,
            new[]
            {
                new ImportedChapter(1, "Chapter I", "chapter1.xhtml", string.Empty, "Hello, Alice. Alice looked at the rabbit. The rabbit saw Alice again."),
                new ImportedChapter(2, "Chapter II", "chapter2.xhtml", string.Empty, "No Alice here? Yes, Alice is here.")
            });

        StoredBookImport storedImport = await store.SaveImportedBookAsync(corpus.Id, book);
        CorpusAnalysisResult analysis = new(
            new CorpusSummary(1, 5, 20, 16, 8, 4.0, 4.2),
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

        IReadOnlyList<StoredWordContext> contexts = await store.ListWordContextsAsync(run.Id, "ALICE", limit: 3, contextWords: 2);
        IReadOnlyList<StoredWordContext> limitedContexts = await store.ListWordContextsAsync(run.Id, "alice", limit: 1, contextWords: 2);
        IReadOnlyList<StoredWordContext> missingContexts = await store.ListWordContextsAsync(run.Id, "missing", limit: 3, contextWords: 2);

        Assert.Equal(3, contexts.Count);
        Assert.Single(limitedContexts);
        Assert.Empty(missingContexts);
        Assert.Equal("Alice", contexts[0].MatchText);
        Assert.Equal("Chapter I", contexts[0].ChapterTitle);
        Assert.Contains("Hello", contexts[0].LeftContext);
        Assert.Contains("Alice looked", contexts[0].RightContext);
        Assert.All(contexts, context => Assert.Equal(run.Id, context.AnalysisRunId));
    }


    [Fact]
    public async Task ListWordContextsAsync_ShouldMatchTypographicApostropheConNormalizedQuery()
    {
        using TestDatabase database = new();
        string sourceFile = database.CreateSourceFile("alice.epub", "fake epub content");
        SqliteCorpusStore store = new(database.Path);
        StoredCorpus corpus = await store.CreateCorpusAsync("English Literature", "en");

        ImportedBook book = new(
            "book-1",
            "Alice",
            "Lewis Carroll",
            "en",
            sourceFile,
            new[]
            {
                new ImportedChapter(1, "Chapter I", "chapter1.xhtml", string.Empty, "Alice’s sister said that Alice's cat won’t stay.")
            });

        StoredBookImport storedImport = await store.SaveImportedBookAsync(corpus.Id, book);
        CorpusAnalysisResult analysis = new(
            new CorpusSummary(1, 1, 8, 7, 7, 7.0, 4.2),
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

        IReadOnlyList<StoredWordContext> possessiveContexts = await store.ListWordContextsAsync(run.Id, "alice's", limit: 3, contextWords: 2);
        IReadOnlyList<StoredWordContext> contractionContexts = await store.ListWordContextsAsync(run.Id, "won't", limit: 3, contextWords: 2);

        Assert.Equal(2, possessiveContexts.Count);
        Assert.Single(contractionContexts);
        Assert.Contains(possessiveContexts, context => context.MatchText == "Alice’s");
        Assert.Contains(possessiveContexts, context => context.MatchText == "Alice's");
        Assert.Equal("won’t", contractionContexts[0].MatchText);
    }


    [Fact]
    public async Task ListWordContextsAsync_ShouldTrimPunctuationAroundKwicContexts()
    {
        using TestDatabase database = new();
        string sourceFile = database.CreateSourceFile("view.epub", "fake epub content");
        SqliteCorpusStore store = new(database.Path);
        StoredCorpus corpus = await store.CreateCorpusAsync("English Literature", "en");

        ImportedBook book = new(
            "book-1",
            "View",
            "Author",
            "en",
            sourceFile,
            new[]
            {
                new ImportedChapter(1, "Chapter I", "chapter1.xhtml", string.Empty, "The old man said: “I have a view,” and smiled.")
            });

        StoredBookImport storedImport = await store.SaveImportedBookAsync(corpus.Id, book);
        CorpusAnalysisResult analysis = new(
            new CorpusSummary(1, 1, 11, 9, 9, 9.0, 4.2),
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

        IReadOnlyList<StoredWordContext> contexts = await store.ListWordContextsAsync(run.Id, "said", limit: 1, contextWords: 4);

        StoredWordContext context = Assert.Single(contexts);
        Assert.Equal("The old man", context.LeftContext);
        Assert.StartsWith("I have a view", context.RightContext, StringComparison.Ordinal);
        Assert.False(context.RightContext.StartsWith(":", StringComparison.Ordinal));
        Assert.False(context.RightContext.StartsWith("“", StringComparison.Ordinal));
    }


    [Fact]
    public async Task ListAnalysisRunsAsync_ShouldReturnSavedRunWithCorpusAndBookTitle()
    {
        using TestDatabase database = new();
        string sourceFile = database.CreateSourceFile("alice.epub", "fake epub content");
        SqliteCorpusStore store = new(database.Path);
        StoredCorpus corpus = await store.CreateCorpusAsync("English Literature", "en");

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

        IReadOnlyList<StoredAnalysisRunSummary> runs = await store.ListAnalysisRunsAsync();
        StoredAnalysisRunSummary summary = Assert.Single(runs);
        StoredAnalysisRunSummary? found = await store.GetAnalysisRunSummaryAsync(run.Id);

        Assert.Equal(run.Id, summary.Id);
        Assert.Equal("English Literature", summary.CorpusName);
        Assert.Equal("Alice", summary.BookTitle);
        Assert.Equal(4, summary.WordTokenCount);
        Assert.NotNull(found);
        Assert.Equal(run.Id, found!.Id);
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



    [Fact]
    public async Task ListAnalysisRunBooksAsync_ShouldFallbackToRunBookForSingleBookRuns()
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
                new ImportedChapter(1, "Chapter I", "chapter1.xhtml", string.Empty, "Hello, Alice."),
                new ImportedChapter(2, "Chapter II", "chapter2.xhtml", string.Empty, "Bye, Alice.")
            });

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

        IReadOnlyList<StoredAnalysisRunBook> books = await store.ListAnalysisRunBooksAsync(run.Id);

        StoredAnalysisRunBook listedBook = Assert.Single(books);
        Assert.Equal(run.Id, listedBook.AnalysisRunId);
        Assert.Equal(storedImport.Book.Id, listedBook.BookId);
        Assert.Equal(1, listedBook.OrderIndex);
        Assert.Equal("Alice", listedBook.Title);
        Assert.Equal("Lewis Carroll", listedBook.Author);
        Assert.Equal(2, listedBook.ChapterCount);
        Assert.Equal("Hello, Alice.Bye, Alice.".Length, listedBook.CharacterCount);
    }

    [Fact]
    public async Task SaveAnalysisRunBooksAsync_ShouldPersistRealBooksForAggregateRun()
    {
        using TestDatabase database = new();
        string firstFile = database.CreateSourceFile("first.epub", "first fake epub content");
        string secondFile = database.CreateSourceFile("second.epub", "second fake epub content");
        string folderPath = database.CreateSourceFolder("books");

        SqliteCorpusStore store = new(database.Path);
        StoredCorpus corpus = await store.CreateCorpusAsync("English Literature", "en");

        ImportedBook aggregateBook = new(
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
        ImportedBook firstBook = new(
            "first",
            "First",
            "Author One",
            "en",
            firstFile,
            new[] { new ImportedChapter(1, "Chapter I", "chapter1.xhtml", string.Empty, "First text.") });
        ImportedBook secondBook = new(
            "second",
            "Second",
            "Author Two",
            "en",
            secondFile,
            new[] { new ImportedChapter(1, "Chapter I", "chapter1.xhtml", string.Empty, "Second text."), new ImportedChapter(2, "Chapter II", "chapter2.xhtml", string.Empty, "More text.") });

        StoredBookImport aggregateImport = await store.SaveImportedBookAsync(corpus.Id, aggregateBook);
        StoredBookImport firstImport = await store.SaveImportedBookAsync(corpus.Id, firstBook);
        StoredBookImport secondImport = await store.SaveImportedBookAsync(corpus.Id, secondBook);
        CorpusAnalysisResult analysis = new(
            new CorpusSummary(2, 2, 4, 4, 4, 2.0, 4.0),
            Array.Empty<WordFrequency>(),
            Array.Empty<NGramFrequency>(),
            Array.Empty<NextWordFrequency>(),
            Array.Empty<AnalyzedSentence>());

        StoredAnalysisRun run = await store.SaveAnalysisRunAsync(
            corpus.Id,
            aggregateImport.Book.Id,
            new AnalysisSettings(),
            analysis,
            "report.md",
            "words.csv",
            "ngrams.csv",
            "next_words.csv",
            "extracted_text.txt");

        IReadOnlyList<StoredAnalysisRunBook> linkedBooks = await store.SaveAnalysisRunBooksAsync(
            run.Id,
            new[] { firstImport, secondImport });
        IReadOnlyList<StoredAnalysisRunBook> listedBooks = await store.ListAnalysisRunBooksAsync(run.Id);

        Assert.Equal(aggregateImport.Book.Id, run.BookId);
        Assert.Equal(2, linkedBooks.Count);
        Assert.Equal(2, listedBooks.Count);
        Assert.Equal("First", listedBooks[0].Title);
        Assert.Equal("Author One", listedBooks[0].Author);
        Assert.Equal(1, listedBooks[0].OrderIndex);
        Assert.Equal(1, listedBooks[0].ChapterCount);
        Assert.Equal("Second", listedBooks[1].Title);
        Assert.Equal(2, listedBooks[1].OrderIndex);
        Assert.Equal(2, listedBooks[1].ChapterCount);
        Assert.Equal("Second text.More text.".Length, listedBooks[1].CharacterCount);
    }


    [Fact]
    public async Task ListWordBookDistributionAsync_ShouldCountWordAcrossSourceBooks()
    {
        using TestDatabase database = new();
        string firstFile = database.CreateSourceFile("moby.epub", "first fake epub content");
        string secondFile = database.CreateSourceFile("pirate.epub", "second fake epub content");
        string folderPath = database.CreateSourceFolder("books");

        SqliteCorpusStore store = new(database.Path);
        StoredCorpus corpus = await store.CreateCorpusAsync("English Literature", "en");

        ImportedBook aggregateBook = new(
            "folder-books",
            "EPUB folder: books",
            string.Empty,
            "en",
            folderPath,
            new[]
            {
                new ImportedChapter(1, "Moby — Chapter I", "moby.epub::chapter1.xhtml", string.Empty, "whale whale sea"),
                new ImportedChapter(2, "Pirate — Chapter I", "pirate.epub::chapter1.xhtml", string.Empty, "sea whale")
            });
        ImportedBook firstBook = new(
            "moby",
            "Moby Dick",
            "Herman Melville",
            "en",
            firstFile,
            new[] { new ImportedChapter(1, "Chapter I", "chapter1.xhtml", string.Empty, "whale whale sea") });
        ImportedBook secondBook = new(
            "pirate",
            "The Pirate",
            "Walter Scott",
            "en",
            secondFile,
            new[] { new ImportedChapter(1, "Chapter I", "chapter1.xhtml", string.Empty, "sea whale") });

        StoredBookImport aggregateImport = await store.SaveImportedBookAsync(corpus.Id, aggregateBook);
        StoredBookImport firstImport = await store.SaveImportedBookAsync(corpus.Id, firstBook);
        StoredBookImport secondImport = await store.SaveImportedBookAsync(corpus.Id, secondBook);
        CorpusAnalysisResult analysis = new(
            new CorpusSummary(2, 2, 5, 5, 2, 2.5, 4.0),
            Array.Empty<WordFrequency>(),
            Array.Empty<NGramFrequency>(),
            Array.Empty<NextWordFrequency>(),
            Array.Empty<AnalyzedSentence>());

        StoredAnalysisRun run = await store.SaveAnalysisRunAsync(
            corpus.Id,
            aggregateImport.Book.Id,
            new AnalysisSettings(),
            analysis,
            "report.md",
            "words.csv",
            "ngrams.csv",
            "next_words.csv",
            "extracted_text.txt");
        await store.SaveAnalysisRunBooksAsync(run.Id, new[] { firstImport, secondImport });

        IReadOnlyList<StoredWordBookStatistic> distribution = await store.ListWordBookDistributionAsync(run.Id, "WHALE", limit: 10);

        Assert.Equal(2, distribution.Count);
        Assert.Equal("Moby Dick", distribution[0].Title);
        Assert.Equal(2, distribution[0].Count);
        Assert.Equal(3, distribution[0].WordTokenCount);
        Assert.Equal(666666.67, distribution[0].FrequencyPerMillion, precision: 2);
        Assert.Equal("The Pirate", distribution[1].Title);
        Assert.Equal(1, distribution[1].Count);
    }

    [Fact]
    public async Task ListCollocationsAsync_ShouldCountWindowCollocatesAroundTargetWord()
    {
        using TestDatabase database = new();
        string sourceFile = database.CreateSourceFile("alice.epub", "fake epub content");
        SqliteCorpusStore store = new(database.Path);
        StoredCorpus corpus = await store.CreateCorpusAsync("English Kids", "en");

        ImportedBook book = new(
            "alice",
            "Alice",
            "Lewis Carroll",
            "en",
            sourceFile,
            new[]
            {
                new ImportedChapter(1, "Chapter I", "chapter1.xhtml", string.Empty, "Alice saw white rabbit. Alice followed rabbit.")
            });

        StoredBookImport storedImport = await store.SaveImportedBookAsync(corpus.Id, book);
        CorpusAnalysisResult analysis = new(
            new CorpusSummary(1, 2, 7, 7, 5, 3.5, 5.0),
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

        IReadOnlyList<StoredCollocationStatistic> collocations = await store.ListCollocationsAsync(run.Id, "ALICE", window: 2, limit: 10);

        StoredCollocationStatistic rabbit = Assert.Single(collocations, item => item.Collocate == "rabbit");
        Assert.Equal(2, rabbit.Count);
        Assert.Equal(1, rabbit.LeftCount);
        Assert.Equal(1, rabbit.RightCount);
        Assert.Equal(1.0, rabbit.RatePerTarget, precision: 2);
        Assert.Equal(1.5, rabbit.AverageDistance, precision: 2);
        Assert.Equal(1.0, rabbit.DiceCoefficient, precision: 2);

        StoredCollocationStatistic white = Assert.Single(collocations, item => item.Collocate == "white");
        Assert.Equal(2, white.Count);
        Assert.Equal(1, white.LeftCount);
        Assert.Equal(1, white.RightCount);
        Assert.Equal(2.0, white.AverageDistance, precision: 2);
        // The single "white" token appears inside the window of two target occurrences.
        // Dice is bounded by corpus word frequency, so the co-occurrence used for Dice is 1.
        Assert.Equal(0.67, white.DiceCoefficient, precision: 2);
    }

    [Fact]
    public async Task ListCollocationsAsync_ShouldUseTokenIndexWhenAvailable()
    {
        using TestDatabase database = new();
        string sourceFile = database.CreateSourceFile("alice.epub", "fake epub content");
        SqliteCorpusStore store = new(database.Path);
        StoredCorpus corpus = await store.CreateCorpusAsync("English Kids", "en");

        ImportedBook book = new(
            "alice",
            "Alice",
            "Lewis Carroll",
            "en",
            sourceFile,
            new[]
            {
                new ImportedChapter(1, "Chapter I", "chapter1.xhtml", string.Empty, "Alice saw white rabbit. Alice followed rabbit.")
            });

        StoredBookImport storedImport = await store.SaveImportedBookAsync(corpus.Id, book);
        CorpusAnalysisResult analysis = new(
            new CorpusSummary(1, 2, 7, 7, 5, 3.5, 5.0),
            new[]
            {
                new WordFrequency("alice", 2, 1, 285714.29, false),
                new WordFrequency("rabbit", 2, 1, 285714.29, false),
                new WordFrequency("saw", 1, 1, 142857.14, false),
                new WordFrequency("white", 1, 1, 142857.14, false),
                new WordFrequency("followed", 1, 1, 142857.14, false)
            },
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

        await UpdateChapterCleanTextAsync(database.Path, storedImport.Chapters[0].Id, "This changed chapter text has no target word.");

        IReadOnlyList<StoredCollocationStatistic> collocations = await store.ListCollocationsAsync(run.Id, "alice", window: 2, limit: 10);

        StoredCollocationStatistic rabbit = Assert.Single(collocations, item => item.Collocate == "rabbit");
        Assert.Equal(2, rabbit.Count);
        Assert.Equal(1, rabbit.LeftCount);
        Assert.Equal(1, rabbit.RightCount);
    }


    [Fact]
    public async Task ListCollocationsAsync_ShouldRankCharacteristicCollocatesBeforeFrequentFunctionWords()
    {
        using TestDatabase database = new();
        string sourceFile = database.CreateSourceFile("whale.epub", "fake epub content");
        SqliteCorpusStore store = new(database.Path);
        StoredCorpus corpus = await store.CreateCorpusAsync("English Sea", "en");

        ImportedBook book = new(
            "whale",
            "Whale Book",
            "Test Author",
            "en",
            sourceFile,
            new[]
            {
                new ImportedChapter(1, "Chapter I", "chapter1.xhtml", string.Empty,
                    "the sperm whale met the sperm whale. the whale saw the sea. the sky and the sea were blue.")
            });

        StoredBookImport storedImport = await store.SaveImportedBookAsync(corpus.Id, book);
        CorpusAnalysisResult analysis = new(
            new CorpusSummary(1, 3, 18, 18, 8, 6.0, 4.0),
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

        IReadOnlyList<StoredCollocationStatistic> collocations = await store.ListCollocationsAsync(run.Id, "whale", window: 2, limit: 10);

        Assert.NotEmpty(collocations);
        Assert.Equal("sperm", collocations[0].Collocate);
        Assert.True(collocations[0].DiceCoefficient > collocations.Single(item => item.Collocate == "the").DiceCoefficient);
    }

    [Fact]
    public async Task ListPhrasesAsync_ShouldMineRepeatedPhrasesWithoutCrossingPunctuation()
    {
        using TestDatabase database = new();
        string sourceFile = database.CreateSourceFile("italian.epub", "fake epub content");
        SqliteCorpusStore store = new(database.Path);
        StoredCorpus corpus = await store.CreateCorpusAsync("Italian Literature", "it");

        ImportedBook book = new(
            "italian",
            "Italian Book",
            "Test Author",
            "it",
            sourceFile,
            new[]
            {
                new ImportedChapter(1, "Chapter I", "chapter1.xhtml", string.Empty,
                    "Piazza del Duomo era piena. Nella piazza del Duomo arrivò gente. Piazza, del resto, era grande.")
            });

        StoredBookImport storedImport = await store.SaveImportedBookAsync(corpus.Id, book);
        CorpusAnalysisResult analysis = new(
            new CorpusSummary(1, 3, 16, 16, 11, 5.33, 5.0),
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

        IReadOnlyList<StoredPhraseStatistic> phrases = await store.ListPhrasesAsync(run.Id, minN: 2, maxN: 3, minCount: 2, limit: 10);

        StoredPhraseStatistic piazzaDelDuomo = Assert.Single(phrases, phrase => phrase.Phrase == "piazza del duomo");
        Assert.Equal(3, piazzaDelDuomo.N);
        Assert.Equal(2, piazzaDelDuomo.Count);
        Assert.Equal(1, piazzaDelDuomo.ChapterCount);
        Assert.True(piazzaDelDuomo.FrequencyPerMillion > 0);

        StoredPhraseStatistic piazzaDel = Assert.Single(phrases, phrase => phrase.Phrase == "piazza del");
        Assert.Equal(2, piazzaDel.Count);
    }


    [Fact]
    public async Task GetDifficultyProfileAsync_ShouldComputeRelativeDifficultyMetrics()
    {
        using TestDatabase database = new();
        string sourceFile = database.CreateSourceFile("difficulty.epub", "fake epub content");
        SqliteCorpusStore store = new(database.Path);
        StoredCorpus corpus = await store.CreateCorpusAsync("English Literature", "en");

        ImportedBook book = new(
            "book-1",
            "Difficulty Sample",
            "Author",
            "en",
            sourceFile,
            new[] { new ImportedChapter(1, "Chapter I", "chapter1.xhtml", string.Empty, "The cat met an extraordinary elephant.") });

        StoredBookImport storedImport = await store.SaveImportedBookAsync(corpus.Id, book);
        CorpusAnalysisResult analysis = new(
            new CorpusSummary(1, 2, 12, 10, 4, 5.0, 6.0),
            new[]
            {
                new WordFrequency("the", 2, 1, 200000, true),
                new WordFrequency("cat", 2, 1, 200000, false),
                new WordFrequency("elephant", 3, 1, 300000, false),
                new WordFrequency("extraordinary", 3, 1, 300000, false)
            },
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

        StoredDifficultyProfile? profile = await store.GetDifficultyProfileAsync(run.Id);

        Assert.NotNull(profile);
        Assert.Equal(run.Id, profile!.AnalysisRunId);
        Assert.Equal(8, profile.ContentWordTokens);
        Assert.Equal(2, profile.FunctionWordTokens);
        Assert.Equal(6, profile.LongWordTokens);
        Assert.Equal(3, profile.VeryLongWordTokens);
        Assert.Equal(0.8, profile.ContentWordShare, precision: 6);
        Assert.Equal(0.2, profile.FunctionWordShare, precision: 6);
        Assert.Equal(0.6, profile.LongWordShare, precision: 6);
        Assert.Equal(0.3, profile.VeryLongWordShare, precision: 6);
        Assert.Equal(400.0, profile.LexicalDiversityPerThousand, precision: 6);
        Assert.True(profile.HeuristicScore > 0);
    }


    private static async Task UpdateChapterCleanTextAsync(string databasePath, long chapterId, string cleanText)
    {
        await using SqliteConnection connection = new($"Data Source={databasePath};Pooling=False");
        await connection.OpenAsync();

        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText = "UPDATE Chapter SET CleanText = $cleanText, CharacterCount = $characterCount WHERE Id = $chapterId;";
        command.Parameters.AddWithValue("$cleanText", cleanText);
        command.Parameters.AddWithValue("$characterCount", cleanText.Length);
        command.Parameters.AddWithValue("$chapterId", chapterId);
        await command.ExecuteNonQueryAsync();
    }


    private static async Task DeleteTokenOccurrencesAsync(string databasePath, string normalizedToken)
    {
        await using SqliteConnection connection = new($"Data Source={databasePath};Pooling=False");
        await connection.OpenAsync();

        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText = "DELETE FROM TokenOccurrence WHERE NormalizedToken = $normalizedToken;";
        command.Parameters.AddWithValue("$normalizedToken", normalizedToken);
        await command.ExecuteNonQueryAsync();
    }

    private static async Task DeleteAllTokenOccurrencesAsync(string databasePath)
    {
        await using SqliteConnection connection = new($"Data Source={databasePath};Pooling=False");
        await connection.OpenAsync();

        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText = "DELETE FROM TokenOccurrence;";
        await command.ExecuteNonQueryAsync();
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
