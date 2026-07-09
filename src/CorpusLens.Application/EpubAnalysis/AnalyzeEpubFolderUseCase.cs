using System.Security.Cryptography;
using System.Text;
using CorpusLens.Analysis.Statistics;
using CorpusLens.Domain.Analysis;
using CorpusLens.Domain.Books;
using CorpusLens.Domain.Text;
using CorpusLens.Infrastructure.Epub;
using CorpusLens.Infrastructure.Reports;

namespace CorpusLens.Application.EpubAnalysis;

public sealed class AnalyzeEpubFolderUseCase
{
    private readonly EpubBookReader _bookReader;
    private readonly CorpusAnalyzer _analyzer;
    private readonly MarkdownReportWriter _markdownReportWriter;
    private readonly CsvReportWriter _csvReportWriter;
    private readonly ImportDiagnosticsWriter _importDiagnosticsWriter;

    public AnalyzeEpubFolderUseCase()
        : this(new EpubBookReader(), new CorpusAnalyzer(), new MarkdownReportWriter(), new CsvReportWriter(), new ImportDiagnosticsWriter())
    {
    }

    public AnalyzeEpubFolderUseCase(
        EpubBookReader bookReader,
        CorpusAnalyzer analyzer,
        MarkdownReportWriter markdownReportWriter,
        CsvReportWriter csvReportWriter,
        ImportDiagnosticsWriter importDiagnosticsWriter)
    {
        _bookReader = bookReader;
        _analyzer = analyzer;
        _markdownReportWriter = markdownReportWriter;
        _csvReportWriter = csvReportWriter;
        _importDiagnosticsWriter = importDiagnosticsWriter;
    }

    public async Task<AnalyzeEpubFolderResult> ExecuteAsync(
        AnalyzeEpubFolderRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.FolderPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.OutputDirectory);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.SearchPattern);

        if (!Directory.Exists(request.FolderPath))
        {
            throw new DirectoryNotFoundException($"EPUB folder not found: {request.FolderPath}");
        }

        SearchOption searchOption = request.Recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
        IReadOnlyList<string> epubFiles = Directory
            .EnumerateFiles(request.FolderPath, request.SearchPattern, searchOption)
            .Where(file => string.Equals(Path.GetExtension(file), ".epub", StringComparison.OrdinalIgnoreCase))
            .OrderBy(file => file, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (epubFiles.Count == 0)
        {
            throw new InvalidOperationException($"No EPUB files found in folder: {request.FolderPath}");
        }

        List<ImportedBook> books = new();
        List<EpubImportFailure> failures = new();
        foreach (string epubFile in epubFiles)
        {
            try
            {
                ImportedBook book = await _bookReader
                    .ReadAsync(epubFile, request.LanguageCode, cancellationToken)
                    .ConfigureAwait(false);
                books.Add(book);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                failures.Add(new EpubImportFailure(
                    Path.GetFullPath(epubFile),
                    Path.GetFileName(epubFile),
                    exception.Message,
                    exception.GetType().Name));
            }
        }

        if (books.Count == 0)
        {
            string failureSummary = failures.Count == 0
                ? "No EPUB files could be imported."
                : string.Join(Environment.NewLine, failures.Select(failure => $"- {failure.FileName}: {failure.ErrorMessage}"));

            throw new InvalidOperationException($"No valid EPUB files could be imported from folder: {request.FolderPath}{Environment.NewLine}{failureSummary}");
        }

        IReadOnlyList<TextDocument> documents = books
            .Select(book => new TextDocument(book.Id, book.Title, book.LanguageCode, book.Content))
            .ToArray();

        CorpusAnalysisResult analysis = _analyzer.Analyze(documents, request.Settings);
        ImportedBook aggregateBook = BuildAggregateBook(request.FolderPath, request.LanguageCode, books);

        Directory.CreateDirectory(request.OutputDirectory);

        string extractedTextPath = Path.Combine(request.OutputDirectory, "extracted_text.txt");
        await File.WriteAllTextAsync(extractedTextPath, BuildExtractedText(books, failures), cancellationToken).ConfigureAwait(false);

        string title = $"EPUB folder: {Path.GetFileName(Path.GetFullPath(request.FolderPath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar))} ({books.Count} books)";
        string reportPath = Path.Combine(request.OutputDirectory, "report.md");
        await _markdownReportWriter
            .WriteAsync(analysis, reportPath, title, cancellationToken)
            .ConfigureAwait(false);

        await _csvReportWriter
            .WriteAsync(analysis, request.OutputDirectory, cancellationToken)
            .ConfigureAwait(false);

        string importFailuresCsvPath = Path.Combine(request.OutputDirectory, "import_failures.csv");
        await WriteImportFailuresAsync(failures, importFailuresCsvPath, cancellationToken).ConfigureAwait(false);

        string importDiagnosticsPath = Path.Combine(request.OutputDirectory, "import_diagnostics.md");
        await _importDiagnosticsWriter
            .WriteAsync(books, failures, importDiagnosticsPath, cancellationToken)
            .ConfigureAwait(false);

        return new AnalyzeEpubFolderResult(
            aggregateBook,
            books,
            failures,
            analysis,
            reportPath,
            Path.Combine(request.OutputDirectory, "words.csv"),
            Path.Combine(request.OutputDirectory, "ngrams.csv"),
            Path.Combine(request.OutputDirectory, "next_words.csv"),
            extractedTextPath,
            importFailuresCsvPath,
            importDiagnosticsPath);
    }

    private static ImportedBook BuildAggregateBook(string folderPath, string languageCode, IReadOnlyList<ImportedBook> books)
    {
        List<ImportedChapter> chapters = new();
        int orderIndex = 1;

        foreach (ImportedBook book in books)
        {
            foreach (ImportedChapter chapter in book.Chapters)
            {
                string chapterTitle = string.IsNullOrWhiteSpace(chapter.Title)
                    ? book.Title
                    : $"{book.Title} — {chapter.Title}";

                chapters.Add(new ImportedChapter(
                    orderIndex++,
                    chapterTitle,
                    $"{book.SourceFilePath}::{chapter.SourcePath}",
                    chapter.RawHtml,
                    chapter.CleanText));
            }
        }

        string normalizedFolder = Path.GetFullPath(folderPath);
        string title = $"EPUB folder: {Path.GetFileName(normalizedFolder.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar))}";
        string id = $"epub-folder-{HashText(normalizedFolder)}";

        return new ImportedBook(
            id,
            title,
            string.Empty,
            languageCode,
            normalizedFolder,
            chapters);
    }

    private static string BuildExtractedText(IReadOnlyList<ImportedBook> books, IReadOnlyList<EpubImportFailure> failures)
    {
        StringBuilder builder = new();

        if (failures.Count > 0)
        {
            builder.AppendLine("===== Import warnings =====");
            foreach (EpubImportFailure failure in failures)
            {
                builder.AppendLine($"Skipped: {failure.FileName} — {failure.ErrorMessage}");
            }

            builder.AppendLine();
            builder.AppendLine();
        }

        foreach (ImportedBook book in books)
        {
            if (builder.Length > 0)
            {
                builder.AppendLine();
                builder.AppendLine();
            }

            builder.AppendLine($"===== {book.Title} =====");
            if (!string.IsNullOrWhiteSpace(book.Author))
            {
                builder.AppendLine($"Author: {book.Author}");
            }

            builder.AppendLine($"Source: {book.SourceFilePath}");
            builder.AppendLine();
            builder.AppendLine(book.Content);
        }

        return builder.ToString();
    }

    private static async Task WriteImportFailuresAsync(
        IReadOnlyList<EpubImportFailure> failures,
        string outputPath,
        CancellationToken cancellationToken)
    {
        StringBuilder builder = new();
        builder.AppendLine("file_name,file_path,exception_type,error_message");

        foreach (EpubImportFailure failure in failures)
        {
            builder.Append(EscapeCsv(failure.FileName));
            builder.Append(',');
            builder.Append(EscapeCsv(failure.FilePath));
            builder.Append(',');
            builder.Append(EscapeCsv(failure.ExceptionType));
            builder.Append(',');
            builder.AppendLine(EscapeCsv(failure.ErrorMessage));
        }

        await File.WriteAllTextAsync(outputPath, builder.ToString(), cancellationToken).ConfigureAwait(false);
    }

    private static string EscapeCsv(string value)
    {
        if (!value.Contains('"') && !value.Contains(',') && !value.Contains('\n') && !value.Contains('\r'))
        {
            return value;
        }

        return $"\"{value.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
    }

    private static string HashText(string text)
    {
        byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(text));
        StringBuilder builder = new(bytes.Length * 2);
        foreach (byte value in bytes)
        {
            builder.Append(value.ToString("x2", System.Globalization.CultureInfo.InvariantCulture));
        }

        return builder.ToString()[..16];
    }
}
