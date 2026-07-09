using CorpusLens.Domain.Storage;
using CorpusLens.Infrastructure.Storage;

namespace CorpusLens.Application.EpubAnalysis;

public sealed class AnalyzeEpubAndSaveUseCase
{
    private readonly AnalyzeEpubUseCase _analyzeEpubUseCase;

    public AnalyzeEpubAndSaveUseCase()
        : this(new AnalyzeEpubUseCase())
    {
    }

    public AnalyzeEpubAndSaveUseCase(AnalyzeEpubUseCase analyzeEpubUseCase)
    {
        _analyzeEpubUseCase = analyzeEpubUseCase;
    }

    public async Task<AnalyzeEpubAndSaveResult> ExecuteAsync(
        AnalyzeEpubAndSaveRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.CorpusName);

        SqliteCorpusStore store = new(request.DatabasePath);
        StoredCorpus? corpus = await store
            .FindCorpusByNameAsync(request.CorpusName, cancellationToken)
            .ConfigureAwait(false);

        if (corpus is null)
        {
            throw new InvalidOperationException(
                $"Corpus '{request.CorpusName}' does not exist. Create it with: corpuslens corpus create \"{request.CorpusName}\" --language {request.LanguageCode}");
        }

        AnalyzeEpubResult analysisResult = await _analyzeEpubUseCase
            .ExecuteAsync(new AnalyzeEpubRequest(
                request.FilePath,
                request.LanguageCode,
                request.OutputDirectory,
                request.Settings), cancellationToken)
            .ConfigureAwait(false);

        StoredBookImport importedBook = await store
            .SaveImportedBookAsync(corpus.Id, analysisResult.Book, cancellationToken)
            .ConfigureAwait(false);

        StoredAnalysisRun analysisRun = await store
            .SaveAnalysisRunAsync(
                corpus.Id,
                importedBook.Book.Id,
                request.Settings,
                analysisResult.Analysis,
                analysisResult.ReportPath,
                analysisResult.WordsCsvPath,
                analysisResult.NGramsCsvPath,
                analysisResult.NextWordsCsvPath,
                analysisResult.ExtractedTextPath,
                cancellationToken)
            .ConfigureAwait(false);

        return new AnalyzeEpubAndSaveResult(analysisResult, corpus, importedBook.Book, analysisRun);
    }
}
