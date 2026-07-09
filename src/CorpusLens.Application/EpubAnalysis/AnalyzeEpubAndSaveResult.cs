using CorpusLens.Domain.Storage;

namespace CorpusLens.Application.EpubAnalysis;

public sealed record AnalyzeEpubAndSaveResult(
    AnalyzeEpubResult AnalysisResult,
    StoredCorpus Corpus,
    StoredBook Book,
    StoredAnalysisRun AnalysisRun);
