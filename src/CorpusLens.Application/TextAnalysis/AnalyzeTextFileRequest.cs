using CorpusLens.Domain.Analysis;

namespace CorpusLens.Application.TextAnalysis;

public sealed record AnalyzeTextFileRequest(
    string FilePath,
    string LanguageCode,
    string? Title,
    string OutputDirectory,
    AnalysisSettings Settings);
