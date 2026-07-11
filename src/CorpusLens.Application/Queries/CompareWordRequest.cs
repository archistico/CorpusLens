namespace CorpusLens.Application.Queries;

public sealed record CompareWordRequest(
    string DatabasePath,
    long LeftRunId,
    long RightRunId,
    string Word);
