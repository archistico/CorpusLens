namespace CorpusLens.Application.Queries;

public sealed record PhraseExplorerItem(
    string Phrase,
    int N,
    int Count,
    int ChapterCount,
    double FrequencyPerMillion,
    string Boundary);
