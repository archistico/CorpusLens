using CorpusLens.Domain.Storage;
using CorpusLens.Infrastructure.Storage;

namespace CorpusLens.Application.Storage;

public sealed class CreateCorpusUseCase
{
    public async Task<StoredCorpus> ExecuteAsync(
        CreateCorpusRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        SqliteCorpusStore store = new(request.DatabasePath);
        return await store
            .CreateCorpusAsync(request.Name, request.LanguageCode, request.Description, cancellationToken)
            .ConfigureAwait(false);
    }
}
