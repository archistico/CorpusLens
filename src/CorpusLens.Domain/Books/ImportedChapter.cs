namespace CorpusLens.Domain.Books;

public sealed record ImportedChapter(
    int OrderIndex,
    string Title,
    string SourcePath,
    string RawHtml,
    string CleanText);
