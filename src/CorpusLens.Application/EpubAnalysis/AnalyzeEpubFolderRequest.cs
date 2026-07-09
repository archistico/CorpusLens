using CorpusLens.Domain.Analysis;

namespace CorpusLens.Application.EpubAnalysis;

public sealed record AnalyzeEpubFolderRequest(
    string FolderPath,
    string LanguageCode,
    string OutputDirectory,
    AnalysisSettings Settings,
    string SearchPattern,
    bool Recursive);
