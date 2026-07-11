using CorpusLens.Domain.Storage;
using CorpusLens.Infrastructure.Storage;

namespace CorpusLens.Application.Queries;

public sealed class AnalysisRunQueryService
{
    private readonly SqliteCorpusStore _store;

    public AnalysisRunQueryService(string databasePath)
    {
        _store = new SqliteCorpusStore(databasePath);
    }

    public Task<IReadOnlyList<StoredAnalysisRunSummary>> ListRunsAsync(
        long? corpusId = null,
        int limit = 20,
        CancellationToken cancellationToken = default)
    {
        return _store.ListAnalysisRunsAsync(corpusId, limit, cancellationToken);
    }

    public Task<StoredAnalysisRunSummary?> GetRunSummaryAsync(
        long analysisRunId,
        CancellationToken cancellationToken = default)
    {
        return _store.GetAnalysisRunSummaryAsync(analysisRunId, cancellationToken);
    }

    public Task<IReadOnlyList<StoredAnalysisRunBook>> ListRunBooksAsync(
        long analysisRunId,
        CancellationToken cancellationToken = default)
    {
        return _store.ListAnalysisRunBooksAsync(analysisRunId, cancellationToken);
    }
}
