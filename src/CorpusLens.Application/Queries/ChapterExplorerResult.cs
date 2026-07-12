namespace CorpusLens.Application.Queries;

public sealed record ChapterExplorerResult(
    long BookId,
    IReadOnlyList<ChapterExplorerItem> Chapters);
