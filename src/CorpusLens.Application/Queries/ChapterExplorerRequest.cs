namespace CorpusLens.Application.Queries;

public sealed record ChapterExplorerRequest(
    string DatabasePath,
    long BookId);
