using CorpusLens.Domain.Analysis;

namespace CorpusLens.Application.EpubAnalysis;

public sealed record AnalyzeEpubAndSaveRequest(
    string FilePath,
    string LanguageCode,
    string OutputDirectory,
    AnalysisSettings Settings,
    string DatabasePath,
    string CorpusName);
