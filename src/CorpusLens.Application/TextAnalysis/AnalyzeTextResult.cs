using CorpusLens.Domain.Analysis;

namespace CorpusLens.Application.TextAnalysis;

public sealed record AnalyzeTextResult(
    CorpusAnalysisResult Analysis,
    string ReportPath,
    string WordsCsvPath,
    string NGramsCsvPath,
    string NextWordsCsvPath);
