namespace CorpusLens.Domain.Storage;

public sealed record StoredCorpus(
    long Id,
    string Name,
    string LanguageCode,
    string Description,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
