namespace CorpusLens.Application.Storage;

public sealed record CreateCorpusRequest(
    string DatabasePath,
    string Name,
    string LanguageCode,
    string? Description);
