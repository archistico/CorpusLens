using CorpusLens.Domain.Analysis;
using CorpusLens.Domain.Books;

namespace CorpusLens.Application.EpubAnalysis;

public sealed record AnalyzeEpubResult(
    ImportedBook Book,
    CorpusAnalysisResult Analysis,
    string ReportPath,
    string WordsCsvPath,
    string NGramsCsvPath,
    string NextWordsCsvPath,
    string ExtractedTextPath);
