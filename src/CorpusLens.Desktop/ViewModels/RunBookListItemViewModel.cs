using CorpusLens.Domain.Storage;

namespace CorpusLens.Desktop.ViewModels;

public sealed class RunBookListItemViewModel
{
    public RunBookListItemViewModel(StoredAnalysisRunBook book)
    {
        Book = book;
    }

    public StoredAnalysisRunBook Book { get; }

    public long Id => Book.BookId;

    public string DisplayTitle => $"{Book.OrderIndex}. {Book.Title}";

    public string DisplaySubtitle => string.IsNullOrWhiteSpace(Book.Author)
        ? $"{Book.ChapterCount:n0} chapters · {Book.CharacterCount:n0} characters"
        : $"{Book.Author} · {Book.ChapterCount:n0} chapters · {Book.CharacterCount:n0} characters";

    public override string ToString()
    {
        return string.IsNullOrWhiteSpace(Book.Author)
            ? $"{Book.OrderIndex}. {Book.Title}"
            : $"{Book.OrderIndex}. {Book.Title} — {Book.Author}";
    }
}
