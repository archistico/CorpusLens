using CorpusLens.Domain.Storage;
using CorpusLens.Infrastructure.Storage;

namespace CorpusLens.Application.Storage;

public sealed class ListCorporaUseCase
{
    public async Task<IReadOnlyList<StoredCorpus>> ExecuteAsync(
        ListCorporaRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.DatabasePath);

        SqliteCorpusStore store = new(request.DatabasePath);
        return await store.ListCorporaAsync(cancellationToken).ConfigureAwait(false);
    }
}
