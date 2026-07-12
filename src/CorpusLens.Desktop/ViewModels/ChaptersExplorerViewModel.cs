using System.Collections.ObjectModel;
using CorpusLens.Application.Queries;

namespace CorpusLens.Desktop.ViewModels;

public sealed class ChaptersExplorerViewModel : ViewModelBase
{
    private readonly Func<ChapterExplorerRequest, CancellationToken, Task<ChapterExplorerResult>> _query;
    private readonly List<int> _searchMatches = new();

    private string _chapterExplorerTitle = "Chapters explorer";
    private string _chapterExplorerSummary = "Select a source book to inspect its chapters.";
    private string _chapterDetails = "Chapter details will appear here.";
    private string _chapterPreview = string.Empty;
    private string _chapterSearchSummary = "Select a chapter, then enter text to search its preview.";
    private ChapterListItemViewModel? _selectedChapter;
    private string _activeSearchText = string.Empty;
    private int _currentSearchMatchIndex = -1;
    private int _chapterPreviewSelectionStart;
    private int _chapterPreviewSelectionLength;

    public ChaptersExplorerViewModel(
        Func<ChapterExplorerRequest, CancellationToken, Task<ChapterExplorerResult>>? query = null)
    {
        _query = query ?? QueryFromApplicationAsync;
    }

    public ObservableCollection<ChapterListItemViewModel> Chapters { get; } = new();

    public string ChapterExplorerTitle
    {
        get => _chapterExplorerTitle;
        private set => SetProperty(ref _chapterExplorerTitle, value);
    }

    public string ChapterExplorerSummary
    {
        get => _chapterExplorerSummary;
        private set => SetProperty(ref _chapterExplorerSummary, value);
    }

    public string ChapterDetails
    {
        get => _chapterDetails;
        private set => SetProperty(ref _chapterDetails, value);
    }

    public string ChapterPreview
    {
        get => _chapterPreview;
        private set => SetProperty(ref _chapterPreview, value);
    }

    public string ChapterSearchSummary
    {
        get => _chapterSearchSummary;
        private set => SetProperty(ref _chapterSearchSummary, value);
    }

    public ChapterListItemViewModel? SelectedChapter
    {
        get => _selectedChapter;
        private set => SetProperty(ref _selectedChapter, value);
    }

    public int ChapterPreviewSelectionStart
    {
        get => _chapterPreviewSelectionStart;
        private set => SetProperty(ref _chapterPreviewSelectionStart, value);
    }

    public int ChapterPreviewSelectionLength
    {
        get => _chapterPreviewSelectionLength;
        private set => SetProperty(ref _chapterPreviewSelectionLength, value);
    }

    public async Task<string> LoadAsync(
        string databasePath,
        RunBookListItemViewModel book,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(book);

        ChapterExplorerResult result = await _query(
                new ChapterExplorerRequest(databasePath, book.Id),
                cancellationToken)
            .ConfigureAwait(true);
        cancellationToken.ThrowIfCancellationRequested();

        Chapters.Clear();
        foreach (ChapterExplorerItem chapter in result.Chapters.OrderBy(chapter => chapter.OrderIndex))
        {
            Chapters.Add(new ChapterListItemViewModel(chapter));
        }

        ChapterExplorerTitle = $"Chapters — {book.Book.Title}";
        if (Chapters.Count == 0)
        {
            ChapterExplorerSummary = "No persisted chapters were found for this book.";
            SetSelectedChapter(null);
            return $"Book '{book.Book.Title}' has no persisted chapters.";
        }

        int warningCount = result.Chapters.Count(chapter => chapter.HasQualityWarning);
        long characterCount = result.Chapters.Sum(chapter => (long)chapter.CharacterCount);
        long wordCount = result.Chapters.Sum(chapter => (long)chapter.WordCount);
        long sentenceCount = result.Chapters.Sum(chapter => (long)chapter.SentenceCount);

        ChapterExplorerSummary = string.Join(Environment.NewLine,
            $"Chapters: {Chapters.Count:n0}",
            $"Characters: {characterCount:n0}",
            $"Words: {wordCount:n0}",
            $"Sentences: {sentenceCount:n0}",
            $"Quality warnings: {warningCount:n0}");
        SetSelectedChapter(Chapters[0]);

        return $"Loaded {Chapters.Count:n0} chapter(s) for '{book.Book.Title}'.";
    }

    public void SetSelectedChapter(ChapterListItemViewModel? chapter)
    {
        SelectedChapter = chapter;
        ResetSearch();

        if (chapter is null)
        {
            ChapterDetails = "Select a chapter to inspect its persisted text.";
            ChapterPreview = string.Empty;
            ChapterSearchSummary = "Select a chapter, then enter text to search its preview.";
            return;
        }

        ChapterExplorerItem item = chapter.Chapter;
        string title = string.IsNullOrWhiteSpace(item.Title) ? "Untitled chapter" : item.Title;
        string sourcePath = string.IsNullOrWhiteSpace(item.SourcePath) ? "not available" : item.SourcePath;

        ChapterDetails = string.Join(Environment.NewLine,
            $"Title: {title}",
            $"Order: {item.OrderIndex:n0}",
            $"Chapter ID: {item.Id:n0}",
            $"Characters: {item.CharacterCount:n0}",
            $"Words: {item.WordCount:n0}",
            $"Sentences: {item.SentenceCount:n0}",
            $"Quality: {item.QualityLabel}",
            $"Source path: {sourcePath}");
        ChapterPreview = item.CleanText;
        ChapterSearchSummary = "Enter text to search this chapter preview.";
    }

    public void Search(string? searchText)
    {
        _activeSearchText = searchText?.Trim() ?? string.Empty;
        _searchMatches.Clear();
        _currentSearchMatchIndex = -1;
        SetPreviewSelection(0, 0);

        if (SelectedChapter is null)
        {
            ChapterSearchSummary = "Select a chapter before searching.";
            return;
        }

        if (string.IsNullOrWhiteSpace(_activeSearchText))
        {
            ChapterSearchSummary = "Enter text to search this chapter preview.";
            return;
        }

        int searchStart = 0;
        while (searchStart <= ChapterPreview.Length - _activeSearchText.Length)
        {
            int matchIndex = ChapterPreview.IndexOf(
                _activeSearchText,
                searchStart,
                StringComparison.OrdinalIgnoreCase);
            if (matchIndex < 0)
            {
                break;
            }

            _searchMatches.Add(matchIndex);
            searchStart = matchIndex + Math.Max(1, _activeSearchText.Length);
        }

        if (_searchMatches.Count == 0)
        {
            ChapterSearchSummary = $"No matches for '{_activeSearchText}'.";
            return;
        }

        _currentSearchMatchIndex = 0;
        ApplyCurrentSearchMatch();
    }

    public void MoveToNextSearchMatch()
    {
        if (_searchMatches.Count == 0)
        {
            ChapterSearchSummary = string.IsNullOrWhiteSpace(_activeSearchText)
                ? "Enter text to search this chapter preview."
                : $"No matches for '{_activeSearchText}'.";
            return;
        }

        _currentSearchMatchIndex = (_currentSearchMatchIndex + 1) % _searchMatches.Count;
        ApplyCurrentSearchMatch();
    }

    public void MoveToPreviousSearchMatch()
    {
        if (_searchMatches.Count == 0)
        {
            ChapterSearchSummary = string.IsNullOrWhiteSpace(_activeSearchText)
                ? "Enter text to search this chapter preview."
                : $"No matches for '{_activeSearchText}'.";
            return;
        }

        _currentSearchMatchIndex = (_currentSearchMatchIndex - 1 + _searchMatches.Count) % _searchMatches.Count;
        ApplyCurrentSearchMatch();
    }

    public void Clear(string message)
    {
        Chapters.Clear();
        SelectedChapter = null;
        ChapterExplorerTitle = "Chapters explorer";
        ChapterExplorerSummary = message;
        ChapterDetails = "Chapter details will appear here.";
        ChapterPreview = string.Empty;
        ChapterSearchSummary = "Select a chapter, then enter text to search its preview.";
        ResetSearch();
    }

    private void ApplyCurrentSearchMatch()
    {
        int selectionStart = _searchMatches[_currentSearchMatchIndex];
        SetPreviewSelection(selectionStart, _activeSearchText.Length);
        ChapterSearchSummary = string.Join(" ",
            $"Matches: {_searchMatches.Count:n0}.",
            $"Showing {_currentSearchMatchIndex + 1:n0} of {_searchMatches.Count:n0}",
            $"for '{_activeSearchText}'.");
    }

    private void ResetSearch()
    {
        _activeSearchText = string.Empty;
        _searchMatches.Clear();
        _currentSearchMatchIndex = -1;
        SetPreviewSelection(0, 0);
    }

    private void SetPreviewSelection(int start, int length)
    {
        ChapterPreviewSelectionStart = start;
        ChapterPreviewSelectionLength = length;
    }

    private static Task<ChapterExplorerResult> QueryFromApplicationAsync(
        ChapterExplorerRequest request,
        CancellationToken cancellationToken)
    {
        return Task.Run(async () =>
        {
            ChapterExplorerQueryService service = new();
            return await service.GetChaptersAsync(request, cancellationToken).ConfigureAwait(false);
        }, cancellationToken);
    }
}
