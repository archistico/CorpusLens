namespace CorpusLens.Domain.Text;

public sealed record TextSentence(
    int Index,
    string Text,
    string NormalizedText,
    int StartOffset,
    int EndOffset);
