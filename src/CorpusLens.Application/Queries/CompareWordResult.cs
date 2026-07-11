namespace CorpusLens.Application.Queries;

public sealed record CompareWordResult(
    RunComparisonContext Context,
    WordComparisonItem Comparison);
