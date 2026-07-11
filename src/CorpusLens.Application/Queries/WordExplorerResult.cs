using CorpusLens.Domain.Storage;

namespace CorpusLens.Application.Queries;

public sealed record WordExplorerResult(
    StoredWordStatistic? Word,
    IReadOnlyList<StoredNextWordStatistic> NextWords,
    IReadOnlyList<StoredNextWordStatistic> PreviousWords,
    IReadOnlyList<StoredWordContext> Contexts,
    IReadOnlyList<StoredWordBookStatistic> BookDistribution);
