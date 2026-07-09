namespace CorpusLens.Domain.Storage;

public sealed record StoredBook(
    long Id,
    long CorpusId,
    string Title,
    string Author,
    string LanguageCode,
    string OriginalFilePath,
    string FileHash,
    DateTimeOffset ImportedAt,
    string Status,
    string ErrorMessage);
