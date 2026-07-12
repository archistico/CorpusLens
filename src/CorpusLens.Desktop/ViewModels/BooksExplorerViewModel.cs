using System.Collections.ObjectModel;
using CorpusLens.Application.Queries;
using CorpusLens.Domain.Storage;

namespace CorpusLens.Desktop.ViewModels;

public sealed class BooksExplorerViewModel : ViewModelBase
{
    private readonly Func<string, long, CancellationToken, Task<IReadOnlyList<StoredAnalysisRunBook>>> _booksLoader;

    private string _booksExplorerTitle = "Books explorer";
    private string _booksExplorerSummary = "Select a run to inspect its source books.";
    private string _runBookDetails = "Book details will appear here.";
    private RunBookListItemViewModel? _selectedRunBook;

    public BooksExplorerViewModel(
        Func<string, long, CancellationToken, Task<IReadOnlyList<StoredAnalysisRunBook>>>? booksLoader = null)
    {
        _booksLoader = booksLoader ?? LoadBooksFromApplicationAsync;
    }

    public ObservableCollection<RunBookListItemViewModel> RunBooks { get; } = new();

    public string BooksExplorerTitle
    {
        get => _booksExplorerTitle;
        private set => SetProperty(ref _booksExplorerTitle, value);
    }

    public string BooksExplorerSummary
    {
        get => _booksExplorerSummary;
        private set => SetProperty(ref _booksExplorerSummary, value);
    }

    public string RunBookDetails
    {
        get => _runBookDetails;
        private set => SetProperty(ref _runBookDetails, value);
    }

    public RunBookListItemViewModel? SelectedRunBook
    {
        get => _selectedRunBook;
        private set => SetProperty(ref _selectedRunBook, value);
    }

    public async Task LoadAsync(string databasePath, long runId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<StoredAnalysisRunBook> books = await _booksLoader(databasePath, runId, cancellationToken)
            .ConfigureAwait(true);
        cancellationToken.ThrowIfCancellationRequested();

        RunBooks.Clear();
        foreach (StoredAnalysisRunBook book in books)
        {
            RunBooks.Add(new RunBookListItemViewModel(book));
        }

        BooksExplorerTitle = $"Books — run {runId}";
        if (RunBooks.Count == 0)
        {
            BooksExplorerSummary = "No source books are linked to this run.";
            SetSelectedRunBook(null);
            return;
        }

        long chapterCount = books.Sum(book => (long)book.ChapterCount);
        long characterCount = books.Sum(book => (long)book.CharacterCount);
        IReadOnlyList<string> languageCodes = books
            .Select(book => book.LanguageCode)
            .Where(code => !string.IsNullOrWhiteSpace(code))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(code => code, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        BooksExplorerSummary = string.Join(Environment.NewLine,
            $"Source books: {books.Count:n0}",
            $"Chapters: {chapterCount:n0}",
            $"Characters: {characterCount:n0}",
            $"Languages: {DesktopTextFormatter.FormatLanguageCodes(languageCodes)}");
        SetSelectedRunBook(RunBooks[0]);
    }

    public void SetSelectedRunBook(RunBookListItemViewModel? book)
    {
        SelectedRunBook = book;
        RunBookDetails = book is null
            ? "Select a source book to inspect its details."
            : FormatRunBookDetails(book.Book);
    }

    public void Clear(string message)
    {
        RunBooks.Clear();
        SelectedRunBook = null;
        BooksExplorerTitle = "Books explorer";
        BooksExplorerSummary = message;
        RunBookDetails = "Book details will appear here.";
    }

    private static Task<IReadOnlyList<StoredAnalysisRunBook>> LoadBooksFromApplicationAsync(
        string databasePath,
        long runId,
        CancellationToken cancellationToken)
    {
        return Task.Run(async () =>
        {
            AnalysisRunQueryService service = new(databasePath);
            return await service.ListRunBooksAsync(runId, cancellationToken).ConfigureAwait(false);
        }, cancellationToken);
    }

    private static string FormatRunBookDetails(StoredAnalysisRunBook book)
    {
        string author = string.IsNullOrWhiteSpace(book.Author) ? "Unknown" : book.Author;
        string language = string.IsNullOrWhiteSpace(book.LanguageCode) ? "unknown" : book.LanguageCode;
        string sourcePath = string.IsNullOrWhiteSpace(book.OriginalFilePath)
            ? "not available"
            : book.OriginalFilePath;
        string fileHash = string.IsNullOrWhiteSpace(book.FileHash) ? "not available" : book.FileHash;

        return string.Join(Environment.NewLine,
            $"Title: {book.Title}",
            $"Author: {author}",
            $"Language: {language}",
            $"Run order: {book.OrderIndex:n0}",
            $"Book ID: {book.BookId:n0}",
            $"Chapters: {book.ChapterCount:n0}",
            $"Characters: {book.CharacterCount:n0}",
            $"Source file: {sourcePath}",
            $"File hash: {fileHash}");
    }
}
