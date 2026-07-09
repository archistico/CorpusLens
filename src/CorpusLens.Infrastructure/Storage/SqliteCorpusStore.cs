using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CorpusLens.Domain.Analysis;
using CorpusLens.Domain.Books;
using CorpusLens.Domain.Storage;
using Microsoft.Data.Sqlite;

namespace CorpusLens.Infrastructure.Storage;

public sealed class SqliteCorpusStore
{
    private const string EngineVersion = "0.4";

    private static readonly byte[] DirectoryHashSeparator = { 0 };

    private readonly string _databasePath;

    public SqliteCorpusStore(string databasePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(databasePath);
        _databasePath = databasePath;
    }

    public string DatabasePath => _databasePath;

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        string? directory = Path.GetDirectoryName(Path.GetFullPath(_databasePath));
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using SqliteConnection connection = CreateConnection();
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        await EnableForeignKeysAsync(connection, cancellationToken).ConfigureAwait(false);
        await ExecuteNonQueryAsync(connection, SchemaSql, cancellationToken).ConfigureAwait(false);
    }

    public async Task<StoredCorpus> CreateCorpusAsync(
        string name,
        string languageCode,
        string? description = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(languageCode);

        await InitializeAsync(cancellationToken).ConfigureAwait(false);

        DateTimeOffset now = DateTimeOffset.UtcNow;

        await using SqliteConnection connection = CreateConnection();
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        await EnableForeignKeysAsync(connection, cancellationToken).ConfigureAwait(false);

        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO Corpus (Name, LanguageCode, Description, CreatedAt, UpdatedAt)
            VALUES ($name, $languageCode, $description, $createdAt, $updatedAt);
            """;
        AddParameter(command, "$name", name.Trim());
        AddParameter(command, "$languageCode", languageCode.Trim());
        AddParameter(command, "$description", description?.Trim() ?? string.Empty);
        AddParameter(command, "$createdAt", FormatDateTime(now));
        AddParameter(command, "$updatedAt", FormatDateTime(now));

        try
        {
            await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (SqliteException exception) when (exception.SqliteErrorCode == 19)
        {
            throw new InvalidOperationException($"A corpus named '{name}' already exists.", exception);
        }

        long corpusId = await LastInsertRowIdAsync(connection, cancellationToken).ConfigureAwait(false);
        return new StoredCorpus(corpusId, name.Trim(), languageCode.Trim(), description?.Trim() ?? string.Empty, now, now);
    }

    public async Task<IReadOnlyList<StoredCorpus>> ListCorporaAsync(CancellationToken cancellationToken = default)
    {
        await InitializeAsync(cancellationToken).ConfigureAwait(false);

        List<StoredCorpus> corpora = new();

        await using SqliteConnection connection = CreateConnection();
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        await EnableForeignKeysAsync(connection, cancellationToken).ConfigureAwait(false);

        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id, Name, LanguageCode, Description, CreatedAt, UpdatedAt
            FROM Corpus
            ORDER BY Name COLLATE NOCASE;
            """;

        await using SqliteDataReader reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            corpora.Add(ReadCorpus(reader));
        }

        return corpora;
    }

    public async Task<StoredCorpus?> FindCorpusByNameAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        await InitializeAsync(cancellationToken).ConfigureAwait(false);

        await using SqliteConnection connection = CreateConnection();
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        await EnableForeignKeysAsync(connection, cancellationToken).ConfigureAwait(false);

        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id, Name, LanguageCode, Description, CreatedAt, UpdatedAt
            FROM Corpus
            WHERE Name = $name COLLATE NOCASE
            LIMIT 1;
            """;
        AddParameter(command, "$name", name.Trim());

        await using SqliteDataReader reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            return null;
        }

        return ReadCorpus(reader);
    }

    public async Task<StoredBookImport> SaveImportedBookAsync(
        long corpusId,
        ImportedBook book,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(book);
        await InitializeAsync(cancellationToken).ConfigureAwait(false);

        DateTimeOffset importedAt = DateTimeOffset.UtcNow;
        string fileHash = await ComputeSha256Async(book.SourceFilePath, cancellationToken).ConfigureAwait(false);

        await using SqliteConnection connection = CreateConnection();
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        await EnableForeignKeysAsync(connection, cancellationToken).ConfigureAwait(false);
        using SqliteTransaction transaction = connection.BeginTransaction();

        long bookId;
        try
        {
            await using (SqliteCommand command = connection.CreateCommand())
            {
                command.Transaction = transaction;
                command.CommandText = """
                    INSERT INTO Book
                        (CorpusId, Title, Author, LanguageCode, OriginalFilePath, FileHash, ImportedAt, Status, ErrorMessage)
                    VALUES
                        ($corpusId, $title, $author, $languageCode, $originalFilePath, $fileHash, $importedAt, $status, $errorMessage);
                    """;
                AddParameter(command, "$corpusId", corpusId);
                AddParameter(command, "$title", book.Title);
                AddParameter(command, "$author", book.Author);
                AddParameter(command, "$languageCode", book.LanguageCode);
                AddParameter(command, "$originalFilePath", book.SourceFilePath);
                AddParameter(command, "$fileHash", fileHash);
                AddParameter(command, "$importedAt", FormatDateTime(importedAt));
                AddParameter(command, "$status", "Imported");
                AddParameter(command, "$errorMessage", string.Empty);
                await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }

            bookId = await LastInsertRowIdAsync(connection, transaction, cancellationToken).ConfigureAwait(false);

            foreach (ImportedChapter chapter in book.Chapters)
            {
                await using SqliteCommand chapterCommand = connection.CreateCommand();
                chapterCommand.Transaction = transaction;
                chapterCommand.CommandText = """
                    INSERT INTO Chapter
                        (BookId, OrderIndex, Title, SourcePath, CleanText, CharacterCount)
                    VALUES
                        ($bookId, $orderIndex, $title, $sourcePath, $cleanText, $characterCount);
                    """;
                AddParameter(chapterCommand, "$bookId", bookId);
                AddParameter(chapterCommand, "$orderIndex", chapter.OrderIndex);
                AddParameter(chapterCommand, "$title", chapter.Title);
                AddParameter(chapterCommand, "$sourcePath", chapter.SourcePath);
                AddParameter(chapterCommand, "$cleanText", chapter.CleanText);
                AddParameter(chapterCommand, "$characterCount", chapter.CleanText.Length);
                await chapterCommand.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }

            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            throw;
        }

        StoredBook storedBook = new(
            bookId,
            corpusId,
            book.Title,
            book.Author,
            book.LanguageCode,
            book.SourceFilePath,
            fileHash,
            importedAt,
            "Imported",
            string.Empty);

        IReadOnlyList<StoredChapter> chapters = await ListChaptersAsync(bookId, cancellationToken).ConfigureAwait(false);
        return new StoredBookImport(storedBook, chapters);
    }

    public async Task<StoredAnalysisRun> SaveAnalysisRunAsync(
        long corpusId,
        long bookId,
        AnalysisSettings settings,
        CorpusAnalysisResult analysis,
        string reportPath,
        string wordsCsvPath,
        string ngramsCsvPath,
        string nextWordsCsvPath,
        string extractedTextPath,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(analysis);
        await InitializeAsync(cancellationToken).ConfigureAwait(false);

        DateTimeOffset startedAt = DateTimeOffset.UtcNow;
        DateTimeOffset completedAt = startedAt;
        string settingsJson = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = false });

        await using SqliteConnection connection = CreateConnection();
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        await EnableForeignKeysAsync(connection, cancellationToken).ConfigureAwait(false);
        using SqliteTransaction transaction = connection.BeginTransaction();

        long analysisRunId;
        try
        {
            await using (SqliteCommand command = connection.CreateCommand())
            {
                command.Transaction = transaction;
                command.CommandText = """
                    INSERT INTO AnalysisRun
                        (CorpusId, BookId, StartedAt, CompletedAt, Status, EngineVersion, SettingsJson,
                         SentenceCount, TokenCount, WordTokenCount, DistinctWordCount,
                         AverageWordsPerSentence, AverageCharactersPerWord,
                         ReportPath, WordsCsvPath, NGramsCsvPath, NextWordsCsvPath, ExtractedTextPath, ErrorMessage)
                    VALUES
                        ($corpusId, $bookId, $startedAt, $completedAt, $status, $engineVersion, $settingsJson,
                         $sentenceCount, $tokenCount, $wordTokenCount, $distinctWordCount,
                         $averageWordsPerSentence, $averageCharactersPerWord,
                         $reportPath, $wordsCsvPath, $ngramsCsvPath, $nextWordsCsvPath, $extractedTextPath, $errorMessage);
                    """;
                AddParameter(command, "$corpusId", corpusId);
                AddParameter(command, "$bookId", bookId);
                AddParameter(command, "$startedAt", FormatDateTime(startedAt));
                AddParameter(command, "$completedAt", FormatDateTime(completedAt));
                AddParameter(command, "$status", "Completed");
                AddParameter(command, "$engineVersion", EngineVersion);
                AddParameter(command, "$settingsJson", settingsJson);
                AddParameter(command, "$sentenceCount", analysis.Summary.SentenceCount);
                AddParameter(command, "$tokenCount", analysis.Summary.TokenCount);
                AddParameter(command, "$wordTokenCount", analysis.Summary.WordTokenCount);
                AddParameter(command, "$distinctWordCount", analysis.Summary.DistinctWordCount);
                AddParameter(command, "$averageWordsPerSentence", analysis.Summary.AverageWordsPerSentence);
                AddParameter(command, "$averageCharactersPerWord", analysis.Summary.AverageCharactersPerWord);
                AddParameter(command, "$reportPath", reportPath);
                AddParameter(command, "$wordsCsvPath", wordsCsvPath);
                AddParameter(command, "$ngramsCsvPath", ngramsCsvPath);
                AddParameter(command, "$nextWordsCsvPath", nextWordsCsvPath);
                AddParameter(command, "$extractedTextPath", extractedTextPath);
                AddParameter(command, "$errorMessage", string.Empty);
                await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }

            analysisRunId = await LastInsertRowIdAsync(connection, transaction, cancellationToken).ConfigureAwait(false);

            await SaveWordStatisticsAsync(connection, transaction, analysisRunId, corpusId, bookId, analysis.Words, cancellationToken).ConfigureAwait(false);
            await SaveNGramStatisticsAsync(connection, transaction, analysisRunId, corpusId, bookId, analysis.NGrams, cancellationToken).ConfigureAwait(false);
            await SaveNextWordStatisticsAsync(connection, transaction, analysisRunId, corpusId, bookId, analysis.NextWords, cancellationToken).ConfigureAwait(false);
            await SaveSentenceCategoryStatisticsAsync(connection, transaction, analysisRunId, corpusId, bookId, analysis.Sentences, cancellationToken).ConfigureAwait(false);

            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            throw;
        }

        return new StoredAnalysisRun(
            analysisRunId,
            corpusId,
            bookId,
            startedAt,
            completedAt,
            "Completed",
            EngineVersion,
            settingsJson,
            analysis.Summary.SentenceCount,
            analysis.Summary.TokenCount,
            analysis.Summary.WordTokenCount,
            analysis.Summary.DistinctWordCount,
            analysis.Summary.AverageWordsPerSentence,
            analysis.Summary.AverageCharactersPerWord,
            reportPath,
            wordsCsvPath,
            ngramsCsvPath,
            nextWordsCsvPath,
            extractedTextPath,
            string.Empty);
    }

    public async Task<IReadOnlyList<StoredChapter>> ListChaptersAsync(
        long bookId,
        CancellationToken cancellationToken = default)
    {
        await InitializeAsync(cancellationToken).ConfigureAwait(false);

        List<StoredChapter> chapters = new();

        await using SqliteConnection connection = CreateConnection();
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        await EnableForeignKeysAsync(connection, cancellationToken).ConfigureAwait(false);

        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id, BookId, OrderIndex, Title, SourcePath, CleanText, CharacterCount
            FROM Chapter
            WHERE BookId = $bookId
            ORDER BY OrderIndex;
            """;
        AddParameter(command, "$bookId", bookId);

        await using SqliteDataReader reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            chapters.Add(new StoredChapter(
                reader.GetInt64(0),
                reader.GetInt64(1),
                reader.GetInt32(2),
                reader.GetString(3),
                reader.GetString(4),
                reader.GetString(5),
                reader.GetInt32(6)));
        }

        return chapters;
    }

    public async Task<IReadOnlyList<StoredWordStatistic>> ListTopWordsAsync(
        long analysisRunId,
        int limit = 50,
        CancellationToken cancellationToken = default)
    {
        await InitializeAsync(cancellationToken).ConfigureAwait(false);
        int safeLimit = NormalizeLimit(limit);
        List<StoredWordStatistic> words = new();

        await using SqliteConnection connection = CreateConnection();
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        await EnableForeignKeysAsync(connection, cancellationToken).ConfigureAwait(false);

        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id, AnalysisRunId, CorpusId, BookId, Word, Count, DocumentCount, FrequencyPerMillion
            FROM WordStatistic
            WHERE AnalysisRunId = $analysisRunId
            ORDER BY Count DESC, Word COLLATE NOCASE
            LIMIT $limit;
            """;
        AddParameter(command, "$analysisRunId", analysisRunId);
        AddParameter(command, "$limit", safeLimit);

        await using SqliteDataReader reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            words.Add(ReadWordStatistic(reader));
        }

        return words;
    }

    public async Task<IReadOnlyList<StoredNGramStatistic>> ListTopNGramsAsync(
        long analysisRunId,
        int? n = null,
        int limit = 50,
        CancellationToken cancellationToken = default)
    {
        await InitializeAsync(cancellationToken).ConfigureAwait(false);
        int safeLimit = NormalizeLimit(limit);
        List<StoredNGramStatistic> ngrams = new();

        await using SqliteConnection connection = CreateConnection();
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        await EnableForeignKeysAsync(connection, cancellationToken).ConfigureAwait(false);

        await using SqliteCommand command = connection.CreateCommand();
        if (n is null)
        {
            command.CommandText = """
                SELECT Id, AnalysisRunId, CorpusId, BookId, N, Text, Count, DocumentCount, FrequencyPerMillion
                FROM NGramStatistic
                WHERE AnalysisRunId = $analysisRunId
                ORDER BY Count DESC, N, Text COLLATE NOCASE
                LIMIT $limit;
                """;
        }
        else
        {
            command.CommandText = """
                SELECT Id, AnalysisRunId, CorpusId, BookId, N, Text, Count, DocumentCount, FrequencyPerMillion
                FROM NGramStatistic
                WHERE AnalysisRunId = $analysisRunId AND N = $n
                ORDER BY Count DESC, Text COLLATE NOCASE
                LIMIT $limit;
                """;
            AddParameter(command, "$n", n.Value);
        }

        AddParameter(command, "$analysisRunId", analysisRunId);
        AddParameter(command, "$limit", safeLimit);

        await using SqliteDataReader reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            ngrams.Add(ReadNGramStatistic(reader));
        }

        return ngrams;
    }

    public async Task<IReadOnlyList<StoredNextWordStatistic>> ListTopNextWordsAsync(
        long analysisRunId,
        string? word = null,
        int limit = 50,
        CancellationToken cancellationToken = default)
    {
        await InitializeAsync(cancellationToken).ConfigureAwait(false);
        int safeLimit = NormalizeLimit(limit);
        List<StoredNextWordStatistic> nextWords = new();

        await using SqliteConnection connection = CreateConnection();
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        await EnableForeignKeysAsync(connection, cancellationToken).ConfigureAwait(false);

        await using SqliteCommand command = connection.CreateCommand();
        if (string.IsNullOrWhiteSpace(word))
        {
            command.CommandText = """
                SELECT Id, AnalysisRunId, CorpusId, BookId, Word, NextWord, Count, Probability
                FROM NextWordStatistic
                WHERE AnalysisRunId = $analysisRunId
                ORDER BY Count DESC, Word COLLATE NOCASE, NextWord COLLATE NOCASE
                LIMIT $limit;
                """;
        }
        else
        {
            command.CommandText = """
                SELECT Id, AnalysisRunId, CorpusId, BookId, Word, NextWord, Count, Probability
                FROM NextWordStatistic
                WHERE AnalysisRunId = $analysisRunId AND Word = $word COLLATE NOCASE
                ORDER BY Count DESC, NextWord COLLATE NOCASE
                LIMIT $limit;
                """;
            AddParameter(command, "$word", word.Trim());
        }

        AddParameter(command, "$analysisRunId", analysisRunId);
        AddParameter(command, "$limit", safeLimit);

        await using SqliteDataReader reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            nextWords.Add(ReadNextWordStatistic(reader));
        }

        return nextWords;
    }

    public async Task<IReadOnlyList<StoredSentenceCategoryStatistic>> ListSentenceCategoryStatisticsAsync(
        long analysisRunId,
        CancellationToken cancellationToken = default)
    {
        await InitializeAsync(cancellationToken).ConfigureAwait(false);
        List<StoredSentenceCategoryStatistic> categories = new();

        await using SqliteConnection connection = CreateConnection();
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        await EnableForeignKeysAsync(connection, cancellationToken).ConfigureAwait(false);

        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id, AnalysisRunId, CorpusId, BookId, Category, Count, Percentage
            FROM SentenceCategoryStatistic
            WHERE AnalysisRunId = $analysisRunId
            ORDER BY Count DESC, Category COLLATE NOCASE;
            """;
        AddParameter(command, "$analysisRunId", analysisRunId);

        await using SqliteDataReader reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            categories.Add(ReadSentenceCategoryStatistic(reader));
        }

        return categories;
    }

    private static async Task SaveWordStatisticsAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        long analysisRunId,
        long corpusId,
        long bookId,
        IReadOnlyList<WordFrequency> words,
        CancellationToken cancellationToken)
    {
        foreach (WordFrequency word in words)
        {
            await using SqliteCommand command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = """
                INSERT INTO WordStatistic
                    (AnalysisRunId, CorpusId, BookId, Word, Count, DocumentCount, FrequencyPerMillion)
                VALUES
                    ($analysisRunId, $corpusId, $bookId, $word, $count, $documentCount, $frequencyPerMillion);
                """;
            AddParameter(command, "$analysisRunId", analysisRunId);
            AddParameter(command, "$corpusId", corpusId);
            AddParameter(command, "$bookId", bookId);
            AddParameter(command, "$word", word.Word);
            AddParameter(command, "$count", word.Count);
            AddParameter(command, "$documentCount", word.DocumentCount);
            AddParameter(command, "$frequencyPerMillion", word.FrequencyPerMillion);
            await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    private static async Task SaveNGramStatisticsAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        long analysisRunId,
        long corpusId,
        long bookId,
        IReadOnlyList<NGramFrequency> ngrams,
        CancellationToken cancellationToken)
    {
        foreach (NGramFrequency ngram in ngrams)
        {
            await using SqliteCommand command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = """
                INSERT INTO NGramStatistic
                    (AnalysisRunId, CorpusId, BookId, N, Text, Count, DocumentCount, FrequencyPerMillion)
                VALUES
                    ($analysisRunId, $corpusId, $bookId, $n, $text, $count, $documentCount, $frequencyPerMillion);
                """;
            AddParameter(command, "$analysisRunId", analysisRunId);
            AddParameter(command, "$corpusId", corpusId);
            AddParameter(command, "$bookId", bookId);
            AddParameter(command, "$n", ngram.N);
            AddParameter(command, "$text", ngram.Text);
            AddParameter(command, "$count", ngram.Count);
            AddParameter(command, "$documentCount", ngram.DocumentCount);
            AddParameter(command, "$frequencyPerMillion", ngram.FrequencyPerMillion);
            await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    private static async Task SaveNextWordStatisticsAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        long analysisRunId,
        long corpusId,
        long bookId,
        IReadOnlyList<NextWordFrequency> nextWords,
        CancellationToken cancellationToken)
    {
        foreach (NextWordFrequency nextWord in nextWords)
        {
            await using SqliteCommand command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = """
                INSERT INTO NextWordStatistic
                    (AnalysisRunId, CorpusId, BookId, Word, NextWord, Count, Probability)
                VALUES
                    ($analysisRunId, $corpusId, $bookId, $word, $nextWord, $count, $probability);
                """;
            AddParameter(command, "$analysisRunId", analysisRunId);
            AddParameter(command, "$corpusId", corpusId);
            AddParameter(command, "$bookId", bookId);
            AddParameter(command, "$word", nextWord.Word);
            AddParameter(command, "$nextWord", nextWord.NextWord);
            AddParameter(command, "$count", nextWord.Count);
            AddParameter(command, "$probability", nextWord.Probability);
            await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    private static async Task SaveSentenceCategoryStatisticsAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        long analysisRunId,
        long corpusId,
        long bookId,
        IReadOnlyList<AnalyzedSentence> sentences,
        CancellationToken cancellationToken)
    {
        int totalSentenceCount = sentences.Count;
        if (totalSentenceCount == 0)
        {
            return;
        }

        foreach (IGrouping<PhraseCategory, AnalyzedSentence> group in sentences.GroupBy(sentence => sentence.Category))
        {
            int count = group.Count();
            double percentage = count * 100.0 / totalSentenceCount;

            await using SqliteCommand command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = """
                INSERT INTO SentenceCategoryStatistic
                    (AnalysisRunId, CorpusId, BookId, Category, Count, Percentage)
                VALUES
                    ($analysisRunId, $corpusId, $bookId, $category, $count, $percentage);
                """;
            AddParameter(command, "$analysisRunId", analysisRunId);
            AddParameter(command, "$corpusId", corpusId);
            AddParameter(command, "$bookId", bookId);
            AddParameter(command, "$category", group.Key.ToString());
            AddParameter(command, "$count", count);
            AddParameter(command, "$percentage", percentage);
            await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    private SqliteConnection CreateConnection()
    {
        SqliteConnectionStringBuilder builder = new()
        {
            DataSource = _databasePath,
            Mode = SqliteOpenMode.ReadWriteCreate,
            Pooling = false
        };

        return new SqliteConnection(builder.ToString());
    }

    private static async Task EnableForeignKeysAsync(
        SqliteConnection connection,
        CancellationToken cancellationToken)
    {
        await ExecuteNonQueryAsync(connection, "PRAGMA foreign_keys = ON;", cancellationToken).ConfigureAwait(false);
    }

    private static async Task ExecuteNonQueryAsync(
        SqliteConnection connection,
        string sql,
        CancellationToken cancellationToken)
    {
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText = sql;
        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    private static async Task<long> LastInsertRowIdAsync(
        SqliteConnection connection,
        CancellationToken cancellationToken)
    {
        return await LastInsertRowIdAsync(connection, null, cancellationToken).ConfigureAwait(false);
    }

    private static async Task<long> LastInsertRowIdAsync(
        SqliteConnection connection,
        SqliteTransaction? transaction,
        CancellationToken cancellationToken)
    {
        await using SqliteCommand command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = "SELECT last_insert_rowid();";
        object? value = await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        return Convert.ToInt64(value, CultureInfo.InvariantCulture);
    }

    private static StoredCorpus ReadCorpus(SqliteDataReader reader)
    {
        return new StoredCorpus(
            reader.GetInt64(0),
            reader.GetString(1),
            reader.GetString(2),
            reader.GetString(3),
            ParseDateTime(reader.GetString(4)),
            ParseDateTime(reader.GetString(5)));
    }

    private static StoredWordStatistic ReadWordStatistic(SqliteDataReader reader)
    {
        return new StoredWordStatistic(
            reader.GetInt64(0),
            reader.GetInt64(1),
            reader.GetInt64(2),
            reader.GetInt64(3),
            reader.GetString(4),
            reader.GetInt32(5),
            reader.GetInt32(6),
            reader.GetDouble(7));
    }

    private static StoredNGramStatistic ReadNGramStatistic(SqliteDataReader reader)
    {
        return new StoredNGramStatistic(
            reader.GetInt64(0),
            reader.GetInt64(1),
            reader.GetInt64(2),
            reader.GetInt64(3),
            reader.GetInt32(4),
            reader.GetString(5),
            reader.GetInt32(6),
            reader.GetInt32(7),
            reader.GetDouble(8));
    }

    private static StoredNextWordStatistic ReadNextWordStatistic(SqliteDataReader reader)
    {
        return new StoredNextWordStatistic(
            reader.GetInt64(0),
            reader.GetInt64(1),
            reader.GetInt64(2),
            reader.GetInt64(3),
            reader.GetString(4),
            reader.GetString(5),
            reader.GetInt32(6),
            reader.GetDouble(7));
    }

    private static StoredSentenceCategoryStatistic ReadSentenceCategoryStatistic(SqliteDataReader reader)
    {
        return new StoredSentenceCategoryStatistic(
            reader.GetInt64(0),
            reader.GetInt64(1),
            reader.GetInt64(2),
            reader.GetInt64(3),
            Enum.Parse<PhraseCategory>(reader.GetString(4)),
            reader.GetInt32(5),
            reader.GetDouble(6));
    }

    private static string FormatDateTime(DateTimeOffset value)
    {
        return value.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture);
    }

    private static DateTimeOffset ParseDateTime(string value)
    {
        return DateTimeOffset.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
    }

    private static void AddParameter(SqliteCommand command, string name, object value)
    {
        command.Parameters.AddWithValue(name, value);
    }

    private static int NormalizeLimit(int limit)
    {
        return Math.Clamp(limit, 1, 1000);
    }

    private static async Task<string> ComputeSha256Async(
        string filePath,
        CancellationToken cancellationToken)
    {
        if (File.Exists(filePath))
        {
            await using FileStream stream = File.OpenRead(filePath);
            byte[] hash = await SHA256.HashDataAsync(stream, cancellationToken).ConfigureAwait(false);
            return ToHex(hash);
        }

        if (Directory.Exists(filePath))
        {
            return await ComputeDirectorySha256Async(filePath, cancellationToken).ConfigureAwait(false);
        }

        return string.Empty;
    }

    private static async Task<string> ComputeDirectorySha256Async(
        string directoryPath,
        CancellationToken cancellationToken)
    {
        string fullDirectoryPath = Path.GetFullPath(directoryPath);
        using IncrementalHash incrementalHash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);

        foreach (string filePath in Directory
            .EnumerateFiles(fullDirectoryPath, "*.epub", SearchOption.AllDirectories)
            .OrderBy(file => file, StringComparer.OrdinalIgnoreCase))
        {
            cancellationToken.ThrowIfCancellationRequested();

            string relativePath = Path.GetRelativePath(fullDirectoryPath, filePath).Replace('\\', '/');
            byte[] relativePathBytes = Encoding.UTF8.GetBytes(relativePath);
            incrementalHash.AppendData(relativePathBytes);
            incrementalHash.AppendData(DirectoryHashSeparator);

            await using FileStream stream = File.OpenRead(filePath);
            byte[] fileHash = await SHA256.HashDataAsync(stream, cancellationToken).ConfigureAwait(false);
            incrementalHash.AppendData(fileHash);
        }

        return ToHex(incrementalHash.GetHashAndReset());
    }

    private static string ToHex(byte[] hash)
    {
        StringBuilder builder = new(hash.Length * 2);
        foreach (byte value in hash)
        {
            builder.Append(value.ToString("x2", CultureInfo.InvariantCulture));
        }

        return builder.ToString();
    }

    private const string SchemaSql = """
        CREATE TABLE IF NOT EXISTS Corpus
        (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Name TEXT NOT NULL COLLATE NOCASE UNIQUE,
            LanguageCode TEXT NOT NULL,
            Description TEXT NOT NULL DEFAULT '',
            CreatedAt TEXT NOT NULL,
            UpdatedAt TEXT NOT NULL
        );

        CREATE TABLE IF NOT EXISTS Book
        (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            CorpusId INTEGER NOT NULL,
            Title TEXT NOT NULL,
            Author TEXT NOT NULL DEFAULT '',
            LanguageCode TEXT NOT NULL,
            OriginalFilePath TEXT NOT NULL,
            FileHash TEXT NOT NULL DEFAULT '',
            ImportedAt TEXT NOT NULL,
            Status TEXT NOT NULL,
            ErrorMessage TEXT NOT NULL DEFAULT '',
            FOREIGN KEY (CorpusId) REFERENCES Corpus(Id) ON DELETE CASCADE
        );

        CREATE INDEX IF NOT EXISTS IX_Book_CorpusId ON Book (CorpusId);
        CREATE INDEX IF NOT EXISTS IX_Book_FileHash ON Book (FileHash);

        CREATE TABLE IF NOT EXISTS Chapter
        (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            BookId INTEGER NOT NULL,
            OrderIndex INTEGER NOT NULL,
            Title TEXT NOT NULL DEFAULT '',
            SourcePath TEXT NOT NULL DEFAULT '',
            CleanText TEXT NOT NULL DEFAULT '',
            CharacterCount INTEGER NOT NULL DEFAULT 0,
            FOREIGN KEY (BookId) REFERENCES Book(Id) ON DELETE CASCADE
        );

        CREATE INDEX IF NOT EXISTS IX_Chapter_BookId_OrderIndex ON Chapter (BookId, OrderIndex);

        CREATE TABLE IF NOT EXISTS AnalysisRun
        (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            CorpusId INTEGER NOT NULL,
            BookId INTEGER NOT NULL,
            StartedAt TEXT NOT NULL,
            CompletedAt TEXT NOT NULL,
            Status TEXT NOT NULL,
            EngineVersion TEXT NOT NULL,
            SettingsJson TEXT NOT NULL,
            SentenceCount INTEGER NOT NULL,
            TokenCount INTEGER NOT NULL,
            WordTokenCount INTEGER NOT NULL,
            DistinctWordCount INTEGER NOT NULL,
            AverageWordsPerSentence REAL NOT NULL,
            AverageCharactersPerWord REAL NOT NULL,
            ReportPath TEXT NOT NULL,
            WordsCsvPath TEXT NOT NULL,
            NGramsCsvPath TEXT NOT NULL,
            NextWordsCsvPath TEXT NOT NULL,
            ExtractedTextPath TEXT NOT NULL,
            ErrorMessage TEXT NOT NULL DEFAULT '',
            FOREIGN KEY (CorpusId) REFERENCES Corpus(Id) ON DELETE CASCADE,
            FOREIGN KEY (BookId) REFERENCES Book(Id) ON DELETE CASCADE
        );

        CREATE INDEX IF NOT EXISTS IX_AnalysisRun_CorpusId ON AnalysisRun (CorpusId);
        CREATE INDEX IF NOT EXISTS IX_AnalysisRun_BookId ON AnalysisRun (BookId);

        CREATE TABLE IF NOT EXISTS WordStatistic
        (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            AnalysisRunId INTEGER NOT NULL,
            CorpusId INTEGER NOT NULL,
            BookId INTEGER NOT NULL,
            Word TEXT NOT NULL,
            Count INTEGER NOT NULL,
            DocumentCount INTEGER NOT NULL,
            FrequencyPerMillion REAL NOT NULL,
            FOREIGN KEY (AnalysisRunId) REFERENCES AnalysisRun(Id) ON DELETE CASCADE,
            FOREIGN KEY (CorpusId) REFERENCES Corpus(Id) ON DELETE CASCADE,
            FOREIGN KEY (BookId) REFERENCES Book(Id) ON DELETE CASCADE
        );

        CREATE INDEX IF NOT EXISTS IX_WordStatistic_AnalysisRunId_Count ON WordStatistic (AnalysisRunId, Count DESC);
        CREATE INDEX IF NOT EXISTS IX_WordStatistic_CorpusId_Word ON WordStatistic (CorpusId, Word);

        CREATE TABLE IF NOT EXISTS NGramStatistic
        (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            AnalysisRunId INTEGER NOT NULL,
            CorpusId INTEGER NOT NULL,
            BookId INTEGER NOT NULL,
            N INTEGER NOT NULL,
            Text TEXT NOT NULL,
            Count INTEGER NOT NULL,
            DocumentCount INTEGER NOT NULL,
            FrequencyPerMillion REAL NOT NULL,
            FOREIGN KEY (AnalysisRunId) REFERENCES AnalysisRun(Id) ON DELETE CASCADE,
            FOREIGN KEY (CorpusId) REFERENCES Corpus(Id) ON DELETE CASCADE,
            FOREIGN KEY (BookId) REFERENCES Book(Id) ON DELETE CASCADE
        );

        CREATE INDEX IF NOT EXISTS IX_NGramStatistic_AnalysisRunId_N_Count ON NGramStatistic (AnalysisRunId, N, Count DESC);
        CREATE INDEX IF NOT EXISTS IX_NGramStatistic_CorpusId_Text ON NGramStatistic (CorpusId, Text);

        CREATE TABLE IF NOT EXISTS NextWordStatistic
        (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            AnalysisRunId INTEGER NOT NULL,
            CorpusId INTEGER NOT NULL,
            BookId INTEGER NOT NULL,
            Word TEXT NOT NULL,
            NextWord TEXT NOT NULL,
            Count INTEGER NOT NULL,
            Probability REAL NOT NULL,
            FOREIGN KEY (AnalysisRunId) REFERENCES AnalysisRun(Id) ON DELETE CASCADE,
            FOREIGN KEY (CorpusId) REFERENCES Corpus(Id) ON DELETE CASCADE,
            FOREIGN KEY (BookId) REFERENCES Book(Id) ON DELETE CASCADE
        );

        CREATE INDEX IF NOT EXISTS IX_NextWordStatistic_AnalysisRunId_Count ON NextWordStatistic (AnalysisRunId, Count DESC);
        CREATE INDEX IF NOT EXISTS IX_NextWordStatistic_AnalysisRunId_Word_Count ON NextWordStatistic (AnalysisRunId, Word, Count DESC);

        CREATE TABLE IF NOT EXISTS SentenceCategoryStatistic
        (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            AnalysisRunId INTEGER NOT NULL,
            CorpusId INTEGER NOT NULL,
            BookId INTEGER NOT NULL,
            Category TEXT NOT NULL,
            Count INTEGER NOT NULL,
            Percentage REAL NOT NULL,
            FOREIGN KEY (AnalysisRunId) REFERENCES AnalysisRun(Id) ON DELETE CASCADE,
            FOREIGN KEY (CorpusId) REFERENCES Corpus(Id) ON DELETE CASCADE,
            FOREIGN KEY (BookId) REFERENCES Book(Id) ON DELETE CASCADE
        );

        CREATE INDEX IF NOT EXISTS IX_SentenceCategoryStatistic_AnalysisRunId ON SentenceCategoryStatistic (AnalysisRunId);
        """;
}
