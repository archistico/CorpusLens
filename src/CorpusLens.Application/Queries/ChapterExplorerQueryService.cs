using CorpusLens.Analysis.Normalization;
using CorpusLens.Analysis.Sentences;
using CorpusLens.Analysis.Tokens;
using CorpusLens.Domain.Storage;
using CorpusLens.Domain.Text;
using CorpusLens.Infrastructure.Storage;

namespace CorpusLens.Application.Queries;

public sealed class ChapterExplorerQueryService
{
    public const int VeryShortCharacterThreshold = 200;
    public const int VeryLongCharacterThreshold = 100_000;

    private static readonly string[] SuspiciousTitleMarkers =
    {
        "table of contents",
        "contents",
        "indice",
        "index",
        "copyright",
        "colophon",
    };

    private static readonly string[] SuspiciousTextMarkers =
    {
        "project gutenberg",
        "www.gutenberg.org",
        "all rights reserved",
        "copyright ©",
        "copyright (c)",
        "isbn ",
    };

    public async Task<ChapterExplorerResult> GetChaptersAsync(
        ChapterExplorerRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(request.DatabasePath);
        if (request.BookId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(request), "Book ID must be greater than zero.");
        }

        SqliteCorpusStore store = new(request.DatabasePath);
        IReadOnlyList<StoredChapter> storedChapters = await store
            .ListChaptersAsync(request.BookId, cancellationToken)
            .ConfigureAwait(false);

        SentenceSplitter sentenceSplitter = new(new TextNormalizer());
        Tokenizer tokenizer = new();
        List<ChapterExplorerItem> chapters = new(storedChapters.Count);

        foreach (StoredChapter chapter in storedChapters)
        {
            cancellationToken.ThrowIfCancellationRequested();

            IReadOnlyList<TextSentence> sentences = sentenceSplitter.Split(chapter.CleanText);
            int wordCount = 0;
            foreach (TextSentence sentence in sentences)
            {
                cancellationToken.ThrowIfCancellationRequested();
                wordCount += tokenizer.Tokenize(sentence).Count(token => token.Kind == TokenKind.Word);
            }

            int characterCount = chapter.CleanText.Length;
            bool isEmpty = string.IsNullOrWhiteSpace(chapter.CleanText);
            bool isVeryShort = !isEmpty && characterCount < VeryShortCharacterThreshold;
            bool isVeryLong = characterCount >= VeryLongCharacterThreshold;
            bool isPotentiallySuspicious = ContainsAny(chapter.Title, SuspiciousTitleMarkers)
                || ContainsAny(chapter.CleanText, SuspiciousTextMarkers);

            chapters.Add(new ChapterExplorerItem(
                chapter.Id,
                chapter.BookId,
                chapter.OrderIndex,
                chapter.Title,
                chapter.SourcePath,
                chapter.CleanText,
                characterCount,
                wordCount,
                sentences.Count,
                isEmpty,
                isVeryShort,
                isVeryLong,
                isPotentiallySuspicious));
        }

        return new ChapterExplorerResult(request.BookId, chapters);
    }

    private static bool ContainsAny(string value, IEnumerable<string> markers)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return markers.Any(marker => value.Contains(marker, StringComparison.OrdinalIgnoreCase));
    }
}
