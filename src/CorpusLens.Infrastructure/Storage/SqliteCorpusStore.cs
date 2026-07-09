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
    private const string EngineVersion = "0.2";

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

        await using SqliteCommand command = connection.CreateCommand();
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
        long analysisRunId = await LastInsertRowIdAsync(connection, cancellationToken).ConfigureAwait(false);

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

    private static async Task<string> ComputeSha256Async(
        string filePath,
        CancellationToken cancellationToken)
    {
        if (!File.Exists(filePath))
        {
            return string.Empty;
        }

        await using FileStream stream = File.OpenRead(filePath);
        byte[] hash = await SHA256.HashDataAsync(stream, cancellationToken).ConfigureAwait(false);
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
        """;
}
