namespace CorpusLens.Domain.Storage;

public sealed record StoredTokenOccurrence(
    long AnalysisRunId,
    long CorpusId,
    long BookId,
    long ChapterId,
    int ChapterOrderIndex,
    int RunPosition,
    int ChapterPosition,
    string TokenText,
    string NormalizedToken,
    bool IsWord,
    bool IsStopWord,
    int StartOffset,
    int EndOffset);
