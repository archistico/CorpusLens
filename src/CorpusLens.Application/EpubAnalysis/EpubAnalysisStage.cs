namespace CorpusLens.Application.EpubAnalysis;

public enum EpubAnalysisStage
{
    Validating = 0,
    DiscoveringFiles = 1,
    ImportingBooks = 2,
    AnalyzingCorpus = 3,
    WritingArtifacts = 4,
    PersistingBooks = 5,
    PersistingStatistics = 6,
    BuildingTokenIndex = 7,
    Completed = 8,
}
