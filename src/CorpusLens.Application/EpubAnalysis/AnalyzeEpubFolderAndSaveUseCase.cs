using CorpusLens.Domain.Storage;
using CorpusLens.Infrastructure.Storage;

namespace CorpusLens.Application.EpubAnalysis;

public sealed class AnalyzeEpubFolderAndSaveUseCase
{
    private readonly AnalyzeEpubFolderUseCase _analyzeEpubFolderUseCase;

    public AnalyzeEpubFolderAndSaveUseCase()
        : this(new AnalyzeEpubFolderUseCase())
    {
    }

    public AnalyzeEpubFolderAndSaveUseCase(AnalyzeEpubFolderUseCase analyzeEpubFolderUseCase)
    {
        _analyzeEpubFolderUseCase = analyzeEpubFolderUseCase;
    }

    public async Task<AnalyzeEpubFolderAndSaveResult> ExecuteAsync(
        AnalyzeEpubFolderAndSaveRequest request,
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

        AnalyzeEpubFolderResult analysisResult = await _analyzeEpubFolderUseCase
            .ExecuteAsync(new AnalyzeEpubFolderRequest(
                request.FolderPath,
                request.LanguageCode,
                request.OutputDirectory,
                request.Settings,
                request.SearchPattern,
                request.Recursive), cancellationToken)
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

        return new AnalyzeEpubFolderAndSaveResult(analysisResult, corpus, importedBook.Book, analysisRun);
    }
}
