namespace CorpusLens.Domain.Storage;

public sealed record StoredWordContext(
    long AnalysisRunId,
    long BookId,
    long ChapterId,
    int ChapterOrderIndex,
    string ChapterTitle,
    string MatchText,
    string LeftContext,
    string RightContext,
    int OccurrenceIndexInChapter,
    int CharacterOffset);
