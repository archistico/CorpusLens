using CorpusLens.Domain.Storage;
using CorpusLens.Infrastructure.Storage;

namespace CorpusLens.Application.Queries;

public sealed class TokenIndexHealthService
{
    public async Task<TokenIndexHealthResult?> GetHealthAsync(
        string databasePath,
        long analysisRunId,
        CancellationToken cancellationToken = default)
    {
        SqliteCorpusStore store = new(databasePath);
        StoredAnalysisRunSummary? run = await store
            .GetAnalysisRunSummaryAsync(analysisRunId, cancellationToken)
            .ConfigureAwait(false);
        if (run is null)
        {
            return null;
        }

        StoredTokenIndexDiagnostics? diagnostics = await store
            .GetTokenIndexDiagnosticsAsync(analysisRunId, cancellationToken)
            .ConfigureAwait(false);
        IReadOnlyList<StoredAnalysisRunBook> sourceBooks = await store
            .ListAnalysisRunBooksAsync(analysisRunId, cancellationToken)
            .ConfigureAwait(false);
        IReadOnlyList<string> languageCodes = CorpusProfileQueryService.ReadLanguageCodes(sourceBooks);
        long databaseSizeBytes = File.Exists(databasePath) ? new FileInfo(databasePath).Length : 0;

        List<string> warnings = new();
        if (!string.Equals(run.Status, "Completed", StringComparison.OrdinalIgnoreCase))
        {
            warnings.Add($"run status is {run.Status}");
        }

        if (diagnostics is null || !diagnostics.IsIndexed)
        {
            warnings.Add("token index is missing; query fallback will be used");
        }
        else
        {
            if (diagnostics.WordTokenDelta != 0)
            {
                warnings.Add($"token index delta is {diagnostics.WordTokenDelta:+0;-0;0}");
            }

            if (diagnostics.IndexedChapterCount != diagnostics.ExpectedChapterCount)
            {
                warnings.Add($"indexed chapters are {diagnostics.IndexedChapterCount} of {diagnostics.ExpectedChapterCount}");
            }

            bool sourceBooksOk = diagnostics.SourceBookCount == 0
                ? diagnostics.IndexedBookCount > 0
                : diagnostics.IndexedSourceBookCount == diagnostics.SourceBookCount;
            if (!sourceBooksOk)
            {
                warnings.Add($"indexed source books are {diagnostics.IndexedSourceBookCount} of {diagnostics.SourceBookCount}");
            }

            if (diagnostics.RunPositionGapCount != 0)
            {
                warnings.Add($"run position gaps: {diagnostics.RunPositionGapCount}");
            }
        }

        return new TokenIndexHealthResult(
            run,
            diagnostics,
            languageCodes,
            databasePath,
            databaseSizeBytes,
            warnings);
    }
}
