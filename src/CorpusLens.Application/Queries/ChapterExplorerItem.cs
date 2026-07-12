namespace CorpusLens.Application.Queries;

public sealed record ChapterExplorerItem(
    long Id,
    long BookId,
    int OrderIndex,
    string Title,
    string SourcePath,
    string CleanText,
    int CharacterCount,
    int WordCount,
    int SentenceCount,
    bool IsEmpty,
    bool IsVeryShort,
    bool IsVeryLong,
    bool IsPotentiallySuspicious)
{
    public bool HasQualityWarning => IsEmpty || IsVeryShort || IsVeryLong || IsPotentiallySuspicious;

    public string QualityLabel
    {
        get
        {
            List<string> labels = new();

            if (IsEmpty)
            {
                labels.Add("empty");
            }
            else if (IsVeryShort)
            {
                labels.Add("very short");
            }

            if (IsVeryLong)
            {
                labels.Add("very long");
            }

            if (IsPotentiallySuspicious)
            {
                labels.Add("potentially suspicious");
            }

            return labels.Count == 0 ? "normal" : string.Join(" · ", labels);
        }
    }
}
