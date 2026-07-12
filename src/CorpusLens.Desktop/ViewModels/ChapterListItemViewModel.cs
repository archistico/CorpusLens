using CorpusLens.Application.Queries;

namespace CorpusLens.Desktop.ViewModels;

public sealed class ChapterListItemViewModel
{
    public ChapterListItemViewModel(ChapterExplorerItem chapter)
    {
        Chapter = chapter;
    }

    public ChapterExplorerItem Chapter { get; }

    public long Id => Chapter.Id;

    public string DisplayTitle
    {
        get
        {
            string title = string.IsNullOrWhiteSpace(Chapter.Title) ? "Untitled chapter" : Chapter.Title;
            return $"{Chapter.OrderIndex}. {title}";
        }
    }

    public string DisplaySubtitle => string.Join(" · ",
        $"{Chapter.CharacterCount:n0} chars",
        $"{Chapter.WordCount:n0} words",
        $"{Chapter.SentenceCount:n0} sentences",
        Chapter.HasQualityWarning ? $"WARNING: {Chapter.QualityLabel}" : Chapter.QualityLabel);

    public override string ToString()
    {
        string warning = Chapter.HasQualityWarning ? "⚠ " : string.Empty;
        string quality = Chapter.HasQualityWarning ? $" · {Chapter.QualityLabel}" : string.Empty;
        return $"{warning}{DisplayTitle} — {Chapter.CharacterCount:n0} chars{quality}";
    }
}
