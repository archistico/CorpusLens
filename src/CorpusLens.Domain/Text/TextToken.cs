namespace CorpusLens.Domain.Text;

public sealed record TextToken(
    int SentenceIndex,
    int TokenIndex,
    string Text,
    string NormalizedText,
    TokenKind Kind,
    int StartOffset,
    int EndOffset);
