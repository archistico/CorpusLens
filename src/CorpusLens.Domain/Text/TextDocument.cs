namespace CorpusLens.Domain.Text;

public sealed record TextDocument(
    string Id,
    string Title,
    string LanguageCode,
    string Content);
