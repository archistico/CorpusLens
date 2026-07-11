namespace CorpusLens.Application.Queries;

public sealed record CompareWordsRequest(
    string DatabasePath,
    long LeftRunId,
    long RightRunId,
    int Limit,
    int MinCount,
    ComparisonWordFilter WordFilter,
    ComparisonPresenceFilter PresenceFilter);
