using CorpusLens.Domain.Analysis;

namespace CorpusLens.Application.EpubAnalysis;

public sealed record AnalyzeEpubRequest(
    string FilePath,
    string LanguageCode,
    string OutputDirectory,
    AnalysisSettings Settings);
