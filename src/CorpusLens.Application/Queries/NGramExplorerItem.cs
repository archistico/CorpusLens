namespace CorpusLens.Application.Queries;

public sealed record NGramExplorerItem(
    string Text,
    int N,
    int Count,
    int DocumentCount,
    double FrequencyPerMillion,
    string WordPattern);
