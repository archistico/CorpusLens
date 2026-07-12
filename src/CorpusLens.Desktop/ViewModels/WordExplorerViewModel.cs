using CorpusLens.Application.Queries;
using CorpusLens.Domain.Storage;

namespace CorpusLens.Desktop.ViewModels;

public sealed class WordExplorerViewModel : ViewModelBase
{
    private const int RelatedWordLimit = 10;
    private const int ContextLimit = 10;
    private const int ContextWords = 8;
    private const int BookLimit = 10;

    private readonly Func<WordExplorerRequest, CancellationToken, Task<WordExplorerResult>> _wordLoader;

    private string _wordExplorerTitle = "Word explorer";
    private string _wordExplorerSummary = "Select a run and search a word.";
    private string _wordNextWords = "Next words will appear here.";
    private string _wordPreviousWords = "Previous words will appear here.";
    private string _wordKwic = "KWIC contexts will appear here.";
    private string _wordBookDistribution = "Book distribution will appear here.";

    public WordExplorerViewModel(
        Func<WordExplorerRequest, CancellationToken, Task<WordExplorerResult>>? wordLoader = null)
    {
        _wordLoader = wordLoader ?? LoadWordFromApplicationAsync;
    }

    public string WordExplorerTitle
    {
        get => _wordExplorerTitle;
        private set => SetProperty(ref _wordExplorerTitle, value);
    }

    public string WordExplorerSummary
    {
        get => _wordExplorerSummary;
        private set => SetProperty(ref _wordExplorerSummary, value);
    }

    public string WordNextWords
    {
        get => _wordNextWords;
        private set => SetProperty(ref _wordNextWords, value);
    }

    public string WordPreviousWords
    {
        get => _wordPreviousWords;
        private set => SetProperty(ref _wordPreviousWords, value);
    }

    public string WordKwic
    {
        get => _wordKwic;
        private set => SetProperty(ref _wordKwic, value);
    }

    public string WordBookDistribution
    {
        get => _wordBookDistribution;
        private set => SetProperty(ref _wordBookDistribution, value);
    }

    public async Task<string> SearchAsync(
        string databasePath,
        long runId,
        string word,
        CancellationToken cancellationToken = default)
    {
        WordExplorerResult result = await _wordLoader(new WordExplorerRequest(
                databasePath,
                runId,
                word,
                RelatedWordLimit,
                ContextLimit,
                ContextWords,
                BookLimit),
            cancellationToken).ConfigureAwait(true);
        cancellationToken.ThrowIfCancellationRequested();

        ApplyResult(word, result);
        return result.Word is null
            ? $"Word '{word}' was not found in run {runId}."
            : $"Word '{result.Word.Word}' loaded.";
    }

    public void Clear(string message)
    {
        WordExplorerTitle = "Word explorer";
        WordExplorerSummary = message;
        WordNextWords = "Next words will appear here.";
        WordPreviousWords = "Previous words will appear here.";
        WordKwic = "KWIC contexts will appear here.";
        WordBookDistribution = "Book distribution will appear here.";
    }

    private void ApplyResult(string requestedWord, WordExplorerResult result)
    {
        if (result.Word is null)
        {
            WordExplorerTitle = $"Word explorer — {requestedWord}";
            WordExplorerSummary = "Word not found in the selected run.";
            WordNextWords = "No next words.";
            WordPreviousWords = "No previous words.";
            WordKwic = "No KWIC contexts.";
            WordBookDistribution = "No book distribution.";
            return;
        }

        StoredWordStatistic word = result.Word;
        WordExplorerTitle = $"Word explorer — {word.Word}";
        WordExplorerSummary = string.Join(Environment.NewLine,
            $"Type: {(word.IsStopWord ? "function" : "content")}",
            $"Count: {word.Count:n0}",
            $"Documents: {word.DocumentCount:n0}",
            $"Per million: {DesktopTextFormatter.FormatDouble(word.FrequencyPerMillion)}");
        WordNextWords = FormatNextWords(result.NextWords, useNextWord: true);
        WordPreviousWords = FormatNextWords(result.PreviousWords, useNextWord: false);
        WordKwic = FormatKwic(result.Contexts);
        WordBookDistribution = FormatBookDistribution(result.BookDistribution);
    }

    private static Task<WordExplorerResult> LoadWordFromApplicationAsync(
        WordExplorerRequest request,
        CancellationToken cancellationToken)
    {
        return Task.Run(async () =>
        {
            WordExplorerQueryService service = new();
            return await service.GetWordExplorerAsync(request, cancellationToken).ConfigureAwait(false);
        }, cancellationToken);
    }

    private static string FormatNextWords(IReadOnlyList<StoredNextWordStatistic> words, bool useNextWord)
    {
        if (words.Count == 0)
        {
            return useNextWord ? "No next-word statistics found." : "No previous-word statistics found.";
        }

        IEnumerable<string> lines = words.Select((item, index) =>
        {
            string value = useNextWord ? item.NextWord : item.Word;
            return $"{index + 1,2}. {DesktopTextFormatter.TrimForColumn(value, 18),-18} {item.Count,6:n0}  {DesktopTextFormatter.FormatProbability(item.Probability),7}";
        });
        return string.Join(Environment.NewLine, lines);
    }

    private static string FormatKwic(IReadOnlyList<StoredWordContext> contexts)
    {
        if (contexts.Count == 0)
        {
            return "No contexts found.";
        }

        IEnumerable<string> lines = contexts.Select((context, index) =>
        {
            string chapter = string.IsNullOrWhiteSpace(context.ChapterTitle)
                ? $"Chapter {context.ChapterOrderIndex}"
                : context.ChapterTitle;
            return $"{index + 1,2}. {DesktopTextFormatter.TrimForColumn(chapter, 24),-24}  {DesktopTextFormatter.TrimForColumn(context.LeftContext, 28),-28}  [{context.MatchText}]  {DesktopTextFormatter.TrimForColumn(context.RightContext, 28)}";
        });
        return string.Join(Environment.NewLine, lines);
    }

    private static string FormatBookDistribution(IReadOnlyList<StoredWordBookStatistic> books)
    {
        if (books.Count == 0)
        {
            return "No matching source books found.";
        }

        IEnumerable<string> lines = books.Select((book, index) =>
            $"{index + 1,2}. {DesktopTextFormatter.TrimForColumn(book.Title, 24),-24} {book.Count,6:n0}  {DesktopTextFormatter.FormatDouble(book.FrequencyPerMillion),8}/M");
        return string.Join(Environment.NewLine, lines);
    }
}
