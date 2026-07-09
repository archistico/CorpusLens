using CorpusLens.Domain.Analysis;
using CorpusLens.Domain.Books;

namespace CorpusLens.Application.EpubAnalysis;

public sealed record AnalyzeEpubFolderResult(
    ImportedBook Book,
    IReadOnlyList<ImportedBook> SourceBooks,
    IReadOnlyList<EpubImportFailure> Failures,
    CorpusAnalysisResult Analysis,
    string ReportPath,
    string WordsCsvPath,
    string NGramsCsvPath,
    string NextWordsCsvPath,
    string ExtractedTextPath,
    string ImportFailuresCsvPath);
