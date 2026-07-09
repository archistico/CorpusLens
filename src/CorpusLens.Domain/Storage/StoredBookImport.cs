namespace CorpusLens.Domain.Storage;

public sealed record StoredBookImport(
    StoredBook Book,
    IReadOnlyList<StoredChapter> Chapters);
