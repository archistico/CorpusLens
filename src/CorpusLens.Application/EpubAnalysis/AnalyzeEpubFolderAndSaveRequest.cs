using CorpusLens.Domain.Analysis;

namespace CorpusLens.Application.EpubAnalysis;

public sealed record AnalyzeEpubFolderAndSaveRequest(
    string FolderPath,
    string LanguageCode,
    string OutputDirectory,
    AnalysisSettings Settings,
    string SearchPattern,
    bool Recursive,
    string DatabasePath,
    string CorpusName);
