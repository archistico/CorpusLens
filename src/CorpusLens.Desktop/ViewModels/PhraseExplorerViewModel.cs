using CorpusLens.Application.Queries;

namespace CorpusLens.Desktop.ViewModels;

public sealed class PhraseExplorerViewModel : ViewModelBase
{
    private const int DefaultLimit = 30;

    private readonly Func<PhraseExplorerRequest, CancellationToken, Task<PhraseExplorerResult>> _phraseLoader;

    private string _phraseExplorerTitle = "Phrase explorer";
    private string _phraseExplorerSummary = "Select a run and search phrases.";
    private string _phraseResults = "Phrases will appear here.";
    private string _phraseBoundaryLabel = "Filter: all phrases";
    private bool _phraseContentBoundaryOnly;
    private bool _phraseLongestOnly;

    public PhraseExplorerViewModel(
        Func<PhraseExplorerRequest, CancellationToken, Task<PhraseExplorerResult>>? phraseLoader = null)
    {
        _phraseLoader = phraseLoader ?? LoadPhrasesFromApplicationAsync;
    }

    public string PhraseExplorerTitle
    {
        get => _phraseExplorerTitle;
        private set => SetProperty(ref _phraseExplorerTitle, value);
    }

    public string PhraseExplorerSummary
    {
        get => _phraseExplorerSummary;
        private set => SetProperty(ref _phraseExplorerSummary, value);
    }

    public string PhraseResults
    {
        get => _phraseResults;
        private set => SetProperty(ref _phraseResults, value);
    }

    public string PhraseBoundaryLabel
    {
        get => _phraseBoundaryLabel;
        private set => SetProperty(ref _phraseBoundaryLabel, value);
    }

    public bool PhraseContentBoundaryOnly
    {
        get => _phraseContentBoundaryOnly;
        private set
        {
            if (SetProperty(ref _phraseContentBoundaryOnly, value))
            {
                PhraseBoundaryLabel = value ? "Filter: content-word boundary" : "Filter: all phrases";
            }
        }
    }

    public bool PhraseLongestOnly
    {
        get => _phraseLongestOnly;
        private set => SetProperty(ref _phraseLongestOnly, value);
    }

    public void SetContentBoundary(bool enabled)
    {
        PhraseContentBoundaryOnly = enabled;
    }

    public void SetLongestOnly(bool enabled)
    {
        PhraseLongestOnly = enabled;
    }

    public async Task<string> SearchAsync(
        string databasePath,
        long runId,
        string? minNText,
        string? maxNText,
        string? minCountText,
        string? minChaptersText,
        string? limitText,
        CancellationToken cancellationToken = default)
    {
        int minN = DesktopTextFormatter.ParseIntOrDefault(minNText, 2);
        int maxN = DesktopTextFormatter.ParseIntOrDefault(maxNText, 5);
        int minCount = DesktopTextFormatter.ParseIntOrDefault(minCountText, 3);
        int minChapters = DesktopTextFormatter.ParseIntOrDefault(minChaptersText, 2);
        int limit = DesktopTextFormatter.ParseIntOrDefault(limitText, DefaultLimit);
        bool contentBoundaryOnly = PhraseContentBoundaryOnly;
        bool longestOnly = PhraseLongestOnly;

        PhraseExplorerResult result = await _phraseLoader(new PhraseExplorerRequest(
                databasePath,
                runId,
                minN,
                maxN,
                minCount,
                minChapters,
                limit,
                contentBoundaryOnly,
                longestOnly),
            cancellationToken).ConfigureAwait(true);
        cancellationToken.ThrowIfCancellationRequested();

        ApplyResult(result);
        return $"Loaded {result.Phrases.Count:n0} phrase(s).";
    }

    public void Clear(string message)
    {
        PhraseExplorerTitle = "Phrase explorer";
        PhraseExplorerSummary = message;
        PhraseResults = "Phrases will appear here.";
    }

    private void ApplyResult(PhraseExplorerResult result)
    {
        PhraseExplorerTitle = "Phrase explorer";
        PhraseExplorerSummary = string.Join(Environment.NewLine,
            result.MinN == result.MaxN ? $"N: {result.MinN}" : $"N range: {result.MinN}-{result.MaxN}",
            $"Minimum count: {result.MinCount:n0}",
            $"Minimum chapters: {result.MinChapters:n0}",
            result.ContentBoundaryOnly ? "Filter: content-word boundary" : "Filter: all phrases",
            result.LongestOnly ? "Nested phrases: longest only" : "Nested phrases: shown",
            $"Fetched candidates: {result.FetchedCount:n0}",
            $"Matched phrases: {result.MatchedCount:n0}",
            $"Shown phrases: {result.Phrases.Count:n0} of {result.MatchedCount:n0}");
        PhraseResults = FormatPhrases(result.Phrases);
    }

    private static Task<PhraseExplorerResult> LoadPhrasesFromApplicationAsync(
        PhraseExplorerRequest request,
        CancellationToken cancellationToken)
    {
        return Task.Run(async () =>
        {
            PhraseExplorerQueryService service = new();
            return await service.GetPhrasesAsync(request, cancellationToken).ConfigureAwait(false);
        }, cancellationToken);
    }

    private static string FormatPhrases(IReadOnlyList<PhraseExplorerItem> phrases)
    {
        if (phrases.Count == 0)
        {
            return "No phrases found for the selected thresholds.";
        }

        IEnumerable<string> lines = phrases.Select((item, index) =>
            $"{index + 1,2}. {DesktopTextFormatter.TrimForColumn(item.Phrase, 30),-30} n {item.N}  {item.Count,5:n0}  ch {item.ChapterCount,4:n0}  {DesktopTextFormatter.FormatDouble(item.FrequencyPerMillion),8}/M  {item.Boundary}");
        return string.Join(Environment.NewLine, lines);
    }
}
