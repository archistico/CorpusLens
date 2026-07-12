using CorpusLens.Application.Queries;

namespace CorpusLens.Desktop.ViewModels;

public sealed class NGramExplorerViewModel : ViewModelBase
{
    private const int DefaultLimit = 30;

    private readonly Func<NGramExplorerRequest, CancellationToken, Task<NGramExplorerResult>> _ngramLoader;

    private string _ngramExplorerTitle = "N-gram explorer";
    private string _ngramExplorerSummary = "Select a run and search n-grams.";
    private string _ngramResults = "N-grams will appear here.";
    private string _ngramSizeLabel = "Size: all";
    private string _ngramFilterLabel = "Filter: all n-grams";
    private string _ngramSortLabel = "Sort: count";
    private int? _ngramSize;
    private NGramExplorerFilter _ngramFilter = NGramExplorerFilter.All;
    private NGramExplorerSort _ngramSort = NGramExplorerSort.Count;

    public NGramExplorerViewModel(
        Func<NGramExplorerRequest, CancellationToken, Task<NGramExplorerResult>>? ngramLoader = null)
    {
        _ngramLoader = ngramLoader ?? LoadNGramsFromApplicationAsync;
    }

    public string NGramExplorerTitle
    {
        get => _ngramExplorerTitle;
        private set => SetProperty(ref _ngramExplorerTitle, value);
    }

    public string NGramExplorerSummary
    {
        get => _ngramExplorerSummary;
        private set => SetProperty(ref _ngramExplorerSummary, value);
    }

    public string NGramResults
    {
        get => _ngramResults;
        private set => SetProperty(ref _ngramResults, value);
    }

    public string NGramSizeLabel
    {
        get => _ngramSizeLabel;
        private set => SetProperty(ref _ngramSizeLabel, value);
    }

    public string NGramFilterLabel
    {
        get => _ngramFilterLabel;
        private set => SetProperty(ref _ngramFilterLabel, value);
    }

    public string NGramSortLabel
    {
        get => _ngramSortLabel;
        private set => SetProperty(ref _ngramSortLabel, value);
    }

    public int? NGramSize
    {
        get => _ngramSize;
        private set
        {
            if (SetProperty(ref _ngramSize, value))
            {
                NGramSizeLabel = value is null ? "Size: all" : $"Size: {value}-gram";
            }
        }
    }

    public NGramExplorerFilter NGramFilter
    {
        get => _ngramFilter;
        private set
        {
            if (SetProperty(ref _ngramFilter, value))
            {
                NGramFilterLabel = $"Filter: {FilterText(value)}";
            }
        }
    }

    public NGramExplorerSort NGramSort
    {
        get => _ngramSort;
        private set
        {
            if (SetProperty(ref _ngramSort, value))
            {
                NGramSortLabel = $"Sort: {SortText(value)}";
            }
        }
    }

    public void SetSize(int? n)
    {
        NGramSize = n is null ? null : Math.Clamp(n.Value, 2, 8);
    }

    public void SetFilter(NGramExplorerFilter filter)
    {
        NGramFilter = filter;
    }

    public void SetSort(NGramExplorerSort sort)
    {
        NGramSort = sort;
    }

    public async Task<string> SearchAsync(
        string databasePath,
        long runId,
        string? searchTerm,
        string? minCountText,
        string? limitText,
        CancellationToken cancellationToken = default)
    {
        int minCount = DesktopTextFormatter.ParseIntOrDefault(minCountText, 2);
        int limit = DesktopTextFormatter.ParseIntOrDefault(limitText, DefaultLimit);
        int? n = NGramSize;
        NGramExplorerFilter filter = NGramFilter;
        NGramExplorerSort sort = NGramSort;

        NGramExplorerResult result = await _ngramLoader(new NGramExplorerRequest(
                databasePath,
                runId,
                n,
                minCount,
                limit,
                searchTerm,
                filter,
                sort),
            cancellationToken).ConfigureAwait(true);
        cancellationToken.ThrowIfCancellationRequested();

        ApplyResult(result);
        return $"Loaded {result.NGrams.Count:n0} n-gram(s).";
    }

    public void Clear(string message)
    {
        NGramExplorerTitle = "N-gram explorer";
        NGramExplorerSummary = message;
        NGramResults = "N-grams will appear here.";
    }

    private void ApplyResult(NGramExplorerResult result)
    {
        string size = result.N is null ? "all stored sizes" : $"{result.N}-grams";
        string search = string.IsNullOrWhiteSpace(result.SearchTerm)
            ? "Search term: none"
            : $"Contains term: {result.SearchTerm}";

        NGramExplorerTitle = $"N-gram explorer — {size}";
        NGramExplorerSummary = string.Join(Environment.NewLine,
            $"Size: {size}",
            search,
            $"Minimum count: {result.MinCount:n0}",
            $"Filter: {FilterText(result.Filter)}",
            $"Sort: {SortText(result.Sort)}",
            $"Fetched candidates: {result.FetchedCount:n0}",
            $"Matched candidates: {result.MatchedCount:n0}",
            $"Shown n-grams: {result.NGrams.Count:n0} of {result.MatchedCount:n0}",
            "Pattern legend: C = content word, F = function word");
        NGramResults = FormatNGrams(result.NGrams);
    }

    private static Task<NGramExplorerResult> LoadNGramsFromApplicationAsync(
        NGramExplorerRequest request,
        CancellationToken cancellationToken)
    {
        return Task.Run(async () =>
        {
            NGramExplorerQueryService service = new();
            return await service.GetNGramsAsync(request, cancellationToken).ConfigureAwait(false);
        }, cancellationToken);
    }

    private static string FormatNGrams(IReadOnlyList<NGramExplorerItem> ngrams)
    {
        if (ngrams.Count == 0)
        {
            return "No n-grams found for the selected size, filter and thresholds.";
        }

        IEnumerable<string> lines = ngrams.Select((item, index) =>
            $"{index + 1,3}. {DesktopTextFormatter.TrimForColumn(item.Text, 42),-42} n {item.N}  {item.Count,7:n0}  docs {item.DocumentCount,5:n0}  {DesktopTextFormatter.FormatDouble(item.FrequencyPerMillion),10}/M  {item.WordPattern}");
        return string.Join(Environment.NewLine, lines);
    }

    private static string FilterText(NGramExplorerFilter filter)
    {
        return filter switch
        {
            NGramExplorerFilter.ContentOnly => "content words only",
            NGramExplorerFilter.FunctionOnly => "function words only",
            NGramExplorerFilter.ContentBoundary => "content-word boundary",
            _ => "all n-grams",
        };
    }

    private static string SortText(NGramExplorerSort sort)
    {
        return sort switch
        {
            NGramExplorerSort.FrequencyPerMillion => "frequency per million",
            NGramExplorerSort.DocumentCount => "document count",
            NGramExplorerSort.Text => "text",
            _ => "count",
        };
    }
}
