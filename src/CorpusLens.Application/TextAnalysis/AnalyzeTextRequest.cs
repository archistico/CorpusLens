using CorpusLens.Domain.Analysis;

namespace CorpusLens.Application.TextAnalysis;

public sealed record AnalyzeTextRequest(
    string Text,
    string DocumentId,
    string Title,
    string LanguageCode,
    string OutputDirectory,
    AnalysisSettings Settings);
