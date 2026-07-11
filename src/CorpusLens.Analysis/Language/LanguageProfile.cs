namespace CorpusLens.Analysis.Language;

public sealed record LanguageProfile(
    string Code,
    string Name,
    string Family,
    int DefaultLongWordLength,
    int DefaultVeryLongWordLength,
    string ApostropheHandling,
    string TokenizationNotes,
    int StopWordCount,
    bool IsKnown);
