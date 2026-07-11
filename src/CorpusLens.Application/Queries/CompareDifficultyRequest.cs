namespace CorpusLens.Application.Queries;

public sealed record CompareDifficultyRequest(
    string DatabasePath,
    long LeftRunId,
    long RightRunId,
    int? LongWordLength = null,
    int? VeryLongWordLength = null);
