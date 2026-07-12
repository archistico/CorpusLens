using CorpusLens.Application.Storage;
using CorpusLens.Domain.Books;
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
        CancellationToken cancellationToken = default,
        IProgress<EpubAnalysisProgress>? progress = null)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.CorpusName);

        progress?.Report(new EpubAnalysisProgress(
            EpubAnalysisStage.Validating,
            0,
            "Validating database, corpus and language..."));

        SqliteCorpusStore store = new(request.DatabasePath);
        StoredCorpus? corpus = await store
            .FindCorpusByNameAsync(request.CorpusName, cancellationToken)
            .ConfigureAwait(false);

        if (corpus is null)
        {
            throw new InvalidOperationException(
                $"Corpus '{request.CorpusName}' does not exist. Create it with: corpuslens corpus create \"{request.CorpusName}\" --language {request.LanguageCode}");
        }

        if (!CorpusLanguageCatalog.TryNormalizeSupportedCode(request.LanguageCode, out string normalizedLanguage)
            || !CorpusLanguageCatalog.TryNormalizeSupportedCode(corpus.LanguageCode, out string normalizedCorpusLanguage)
            || !string.Equals(normalizedCorpusLanguage, normalizedLanguage, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Corpus '{corpus.Name}' uses language '{corpus.LanguageCode}', but the requested analysis language is '{request.LanguageCode}'.");
        }

        IProgress<EpubAnalysisProgress>? analysisProgress = progress is null
            ? null
            : new ScaledProgress(progress, 0, 70);
        AnalyzeEpubFolderResult analysisResult = await _analyzeEpubFolderUseCase
            .ExecuteAsync(new AnalyzeEpubFolderRequest(
                request.FolderPath,
                normalizedLanguage,
                request.OutputDirectory,
                request.Settings,
                request.SearchPattern,
                request.Recursive), cancellationToken, analysisProgress)
            .ConfigureAwait(false);

        List<long> persistedBookIds = new();
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            progress?.Report(new EpubAnalysisProgress(
                EpubAnalysisStage.PersistingBooks,
                72,
                "Persisting aggregate corpus book...",
                analysisResult.SourceBooks.Count + analysisResult.Failures.Count,
                analysisResult.SourceBooks.Count + analysisResult.Failures.Count,
                analysisResult.SourceBooks.Count,
                analysisResult.Failures.Count));

            StoredBookImport aggregateBook = await store
                .SaveImportedBookAsync(corpus.Id, analysisResult.Book, cancellationToken)
                .ConfigureAwait(false);
            persistedBookIds.Add(aggregateBook.Book.Id);

            List<StoredBookImport> sourceBookImports = new();
            for (int index = 0; index < analysisResult.SourceBooks.Count; index++)
            {
                ImportedBook sourceBook = analysisResult.SourceBooks[index];
                int percent = 74 + (int)Math.Round(
                    (index + 1) * 12.0 / analysisResult.SourceBooks.Count,
                    MidpointRounding.AwayFromZero);
                progress?.Report(new EpubAnalysisProgress(
                    EpubAnalysisStage.PersistingBooks,
                    percent,
                    $"Persisting source book {index + 1:n0}/{analysisResult.SourceBooks.Count:n0}: {sourceBook.Title}",
                    index + 1,
                    analysisResult.SourceBooks.Count,
                    index + 1,
                    analysisResult.Failures.Count));

                StoredBookImport sourceBookImport = await store
                    .SaveImportedBookAsync(corpus.Id, sourceBook, cancellationToken)
                    .ConfigureAwait(false);
                sourceBookImports.Add(sourceBookImport);
                persistedBookIds.Add(sourceBookImport.Book.Id);
            }

            progress?.Report(new EpubAnalysisProgress(
                EpubAnalysisStage.PersistingStatistics,
                88,
                "Persisting analysis run and aggregate statistics...",
                analysisResult.SourceBooks.Count,
                analysisResult.SourceBooks.Count,
                analysisResult.SourceBooks.Count,
                analysisResult.Failures.Count));
            StoredAnalysisRun analysisRun = await store
                .SaveAnalysisRunAsync(
                    corpus.Id,
                    aggregateBook.Book.Id,
                    request.Settings,
                    analysisResult.Analysis,
                    analysisResult.ReportPath,
                    analysisResult.WordsCsvPath,
                    analysisResult.NGramsCsvPath,
                    analysisResult.NextWordsCsvPath,
                    analysisResult.ExtractedTextPath,
                    cancellationToken)
                .ConfigureAwait(false);

            IReadOnlyList<StoredAnalysisRunBook> runBooks = await store
                .SaveAnalysisRunBooksAsync(analysisRun.Id, sourceBookImports, cancellationToken)
                .ConfigureAwait(false);

            progress?.Report(new EpubAnalysisProgress(
                EpubAnalysisStage.BuildingTokenIndex,
                95,
                "Building the persisted token index for source books...",
                analysisResult.SourceBooks.Count,
                analysisResult.SourceBooks.Count,
                analysisResult.SourceBooks.Count,
                analysisResult.Failures.Count));
            await store
                .ReplaceTokenOccurrencesForBooksAsync(
                    analysisRun.Id,
                    corpus.Id,
                    sourceBookImports,
                    analysisResult.Analysis.Words,
                    cancellationToken)
                .ConfigureAwait(false);

            progress?.Report(new EpubAnalysisProgress(
                EpubAnalysisStage.Completed,
                100,
                $"Run {analysisRun.Id} completed: {sourceBookImports.Count:n0} imported, {analysisResult.Failures.Count:n0} skipped.",
                analysisResult.SourceBooks.Count + analysisResult.Failures.Count,
                analysisResult.SourceBooks.Count + analysisResult.Failures.Count,
                sourceBookImports.Count,
                analysisResult.Failures.Count));

            return new AnalyzeEpubFolderAndSaveResult(
                analysisResult,
                corpus,
                aggregateBook.Book,
                sourceBookImports.Select(sourceBook => sourceBook.Book).ToArray(),
                runBooks,
                analysisRun);
        }
        catch (Exception persistenceException)
        {
            if (persistedBookIds.Count > 0)
            {
                try
                {
                    await store.DeleteBooksAsync(persistedBookIds, CancellationToken.None).ConfigureAwait(false);
                }
                catch (Exception cleanupException)
                {
                    throw new AggregateException(
                        "EPUB analysis persistence failed and the compensating cleanup could not remove all partial data.",
                        persistenceException,
                        cleanupException);
                }
            }

            throw;
        }
    }

    private sealed class ScaledProgress : IProgress<EpubAnalysisProgress>
    {
        private readonly IProgress<EpubAnalysisProgress> _target;
        private readonly int _minimum;
        private readonly int _maximum;

        public ScaledProgress(IProgress<EpubAnalysisProgress> target, int minimum, int maximum)
        {
            _target = target;
            _minimum = minimum;
            _maximum = maximum;
        }

        public void Report(EpubAnalysisProgress value)
        {
            int mappedPercent = _minimum + (int)Math.Round(
                value.NormalizedPercent * (_maximum - _minimum) / 100.0,
                MidpointRounding.AwayFromZero);
            EpubAnalysisStage mappedStage = value.Stage == EpubAnalysisStage.Completed
                ? EpubAnalysisStage.WritingArtifacts
                : value.Stage;
            string mappedMessage = value.Stage == EpubAnalysisStage.Completed
                ? "EPUB extraction and artifacts complete; preparing database persistence..."
                : value.Message;
            _target.Report(value with
            {
                Stage = mappedStage,
                Percent = mappedPercent,
                Message = mappedMessage,
            });
        }
    }
}
