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
        ArgumentException.ThrowIfNullOrWhiteSpace(request.DatabasePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Name);

        string languageCode = CorpusLanguageCatalog.NormalizeSupportedCode(request.LanguageCode);
        SqliteCorpusStore store = new(request.DatabasePath);
        return await store
            .CreateCorpusAsync(request.Name, languageCode, request.Description, cancellationToken)
            .ConfigureAwait(false);
    }
}
