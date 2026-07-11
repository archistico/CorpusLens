using CorpusLens.Domain.Storage;
using CorpusLens.Infrastructure.Storage;

namespace CorpusLens.Application.Queries;

public sealed class WordExplorerQueryService
{
    public async Task<WordExplorerResult> GetWordExplorerAsync(
        WordExplorerRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(request.DatabasePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Word);

        int relatedWordLimit = Math.Clamp(request.RelatedWordLimit, 1, 100);
        int contextLimit = Math.Clamp(request.ContextLimit, 1, 100);
        int contextWords = Math.Clamp(request.ContextWords, 1, 30);
        int bookLimit = Math.Clamp(request.BookLimit, 1, 100);

        SqliteCorpusStore store = new(request.DatabasePath);
        StoredWordStatistic? word = await store
            .GetWordStatisticAsync(request.AnalysisRunId, request.Word, cancellationToken)
            .ConfigureAwait(false);

        if (word is null)
        {
            return new WordExplorerResult(
                null,
                Array.Empty<StoredNextWordStatistic>(),
                Array.Empty<StoredNextWordStatistic>(),
                Array.Empty<StoredWordContext>(),
                Array.Empty<StoredWordBookStatistic>());
        }

        Task<IReadOnlyList<StoredNextWordStatistic>> nextWordsTask = store
            .ListTopNextWordsAsync(request.AnalysisRunId, word.Word, relatedWordLimit, cancellationToken);
        Task<IReadOnlyList<StoredNextWordStatistic>> previousWordsTask = store
            .ListPreviousWordsAsync(request.AnalysisRunId, word.Word, relatedWordLimit, cancellationToken);
        Task<IReadOnlyList<StoredWordContext>> contextsTask = store
            .ListWordContextsAsync(request.AnalysisRunId, word.Word, contextLimit, contextWords, cancellationToken);
        Task<IReadOnlyList<StoredWordBookStatistic>> booksTask = store
            .ListWordBookDistributionAsync(request.AnalysisRunId, word.Word, bookLimit, cancellationToken);

        await Task.WhenAll(nextWordsTask, previousWordsTask, contextsTask, booksTask)
            .ConfigureAwait(false);

        return new WordExplorerResult(
            word,
            await nextWordsTask.ConfigureAwait(false),
            await previousWordsTask.ConfigureAwait(false),
            await contextsTask.ConfigureAwait(false),
            await booksTask.ConfigureAwait(false));
    }
}
