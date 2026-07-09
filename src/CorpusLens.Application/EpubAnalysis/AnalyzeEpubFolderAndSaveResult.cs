using CorpusLens.Domain.Storage;

namespace CorpusLens.Application.EpubAnalysis;

public sealed record AnalyzeEpubFolderAndSaveResult(
    AnalyzeEpubFolderResult AnalysisResult,
    StoredCorpus Corpus,
    StoredBook Book,
    StoredAnalysisRun AnalysisRun);
