namespace CorpusLens.Domain.Storage;

public sealed record StoredChapter(
    long Id,
    long BookId,
    int OrderIndex,
    string Title,
    string SourcePath,
    string CleanText,
    int CharacterCount);
