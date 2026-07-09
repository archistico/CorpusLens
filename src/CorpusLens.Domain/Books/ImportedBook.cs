namespace CorpusLens.Domain.Books;

public sealed record ImportedBook(
    string Id,
    string Title,
    string Author,
    string LanguageCode,
    string SourceFilePath,
    IReadOnlyList<ImportedChapter> Chapters)
{
    public string Content => string.Join(Environment.NewLine + Environment.NewLine, Chapters.Select(chapter => chapter.CleanText));
}
