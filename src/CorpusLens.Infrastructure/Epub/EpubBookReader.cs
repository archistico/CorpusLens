using System.Security.Cryptography;
using CorpusLens.Domain.Books;
using VersOne.Epub;

namespace CorpusLens.Infrastructure.Epub;

public sealed class EpubBookReader
{
    private readonly HtmlToTextConverter _htmlToTextConverter;
    private readonly EpubBoilerplateCleaner _boilerplateCleaner;

    public EpubBookReader()
        : this(new HtmlToTextConverter(), new EpubBoilerplateCleaner())
    {
    }

    public EpubBookReader(
        HtmlToTextConverter htmlToTextConverter,
        EpubBoilerplateCleaner boilerplateCleaner)
    {
        _htmlToTextConverter = htmlToTextConverter;
        _boilerplateCleaner = boilerplateCleaner;
    }

    public Task<ImportedBook> ReadAsync(
        string filePath,
        string fallbackLanguageCode,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(fallbackLanguageCode);

        cancellationToken.ThrowIfCancellationRequested();

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("EPUB file not found.", filePath);
        }

        if (!string.Equals(Path.GetExtension(filePath), ".epub", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("The selected file is not an EPUB file.");
        }

        EpubBook book = EpubReader.ReadBook(filePath);
        List<ImportedChapter> chapters = new();
        int orderIndex = 1;

        foreach (var textContentFile in book.ReadingOrder)
        {
            cancellationToken.ThrowIfCancellationRequested();

            string rawHtml = textContentFile.Content ?? string.Empty;
            string cleanText = _boilerplateCleaner.Clean(_htmlToTextConverter.Convert(rawHtml));
            if (string.IsNullOrWhiteSpace(cleanText) || _boilerplateCleaner.IsLikelyFrontMatterOnly(cleanText))
            {
                continue;
            }

            chapters.Add(new ImportedChapter(
                orderIndex,
                $"Chapter {orderIndex}",
                string.Empty,
                rawHtml,
                cleanText));
            orderIndex++;
        }

        if (chapters.Count == 0)
        {
            throw new InvalidOperationException("The EPUB file does not contain readable text content.");
        }

        string title = string.IsNullOrWhiteSpace(book.Title)
            ? Path.GetFileNameWithoutExtension(filePath)
            : book.Title.Trim();

        string author = string.IsNullOrWhiteSpace(book.Author)
            ? "Unknown"
            : book.Author.Trim();

        ImportedBook importedBook = new(
            ComputeFileHash(filePath),
            title,
            author,
            fallbackLanguageCode.Trim(),
            Path.GetFullPath(filePath),
            chapters);

        return Task.FromResult(importedBook);
    }

    private static string ComputeFileHash(string filePath)
    {
        using FileStream stream = File.OpenRead(filePath);
        byte[] hash = SHA256.HashData(stream);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
