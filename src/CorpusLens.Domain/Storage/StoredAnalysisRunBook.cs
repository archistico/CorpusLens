namespace CorpusLens.Domain.Storage;

public sealed record StoredAnalysisRunBook(
    long AnalysisRunId,
    long BookId,
    int OrderIndex,
    string Title,
    string Author,
    string LanguageCode,
    string OriginalFilePath,
    string FileHash,
    int ChapterCount,
    int CharacterCount);
