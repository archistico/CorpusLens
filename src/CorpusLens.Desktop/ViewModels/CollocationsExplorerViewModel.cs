using CorpusLens.Application.Queries;

namespace CorpusLens.Desktop.ViewModels;

public sealed class CollocationsExplorerViewModel : ViewModelBase
{
    private const int DefaultLimit = 30;

    private readonly Func<CollocationExplorerRequest, CancellationToken, Task<CollocationExplorerResult>> _collocationLoader;

    private string _collocationExplorerTitle = "Collocations explorer";
    private string _collocationExplorerSummary = "Select a run and search collocations.";
    private string _collocationResults = "Collocations will appear here.";
    private string _collocationFilterLabel = "Filter: all words";
    private CollocationExplorerFilter _collocationFilter = CollocationExplorerFilter.All;

    public CollocationsExplorerViewModel(
        Func<CollocationExplorerRequest, CancellationToken, Task<CollocationExplorerResult>>? collocationLoader = null)
    {
        _collocationLoader = collocationLoader ?? LoadCollocationsFromApplicationAsync;
    }

    public string CollocationExplorerTitle
    {
        get => _collocationExplorerTitle;
        private set => SetProperty(ref _collocationExplorerTitle, value);
    }

    public string CollocationExplorerSummary
    {
        get => _collocationExplorerSummary;
        private set => SetProperty(ref _collocationExplorerSummary, value);
    }

    public string CollocationResults
    {
        get => _collocationResults;
        private set => SetProperty(ref _collocationResults, value);
    }

    public string CollocationFilterLabel
    {
        get => _collocationFilterLabel;
        private set => SetProperty(ref _collocationFilterLabel, value);
    }

    public CollocationExplorerFilter CollocationFilter
    {
        get => _collocationFilter;
        private set
        {
            if (SetProperty(ref _collocationFilter, value))
            {
                CollocationFilterLabel = $"Filter: {FilterText(value)}";
            }
        }
    }

    public void SetFilter(CollocationExplorerFilter filter)
    {
        CollocationFilter = filter;
    }

    public async Task<string> SearchAsync(
        string databasePath,
        long runId,
        string word,
        string? windowText,
        string? minCountText,
        string? minDiceText,
        string? limitText,
        CancellationToken cancellationToken = default)
    {
        int window = DesktopTextFormatter.ParseIntOrDefault(windowText, 4);
        int minCount = DesktopTextFormatter.ParseIntOrDefault(minCountText, 1);
        int limit = DesktopTextFormatter.ParseIntOrDefault(limitText, DefaultLimit);
        double minDice = DesktopTextFormatter.ParseDoubleOrDefault(minDiceText, 0.0);
        CollocationExplorerFilter filter = CollocationFilter;

        CollocationExplorerResult result = await _collocationLoader(new CollocationExplorerRequest(
                databasePath,
                runId,
                word,
                window,
                limit,
                minCount,
                minDice,
                filter),
            cancellationToken).ConfigureAwait(true);
        cancellationToken.ThrowIfCancellationRequested();

        ApplyResult(result);
        return $"Loaded {result.Collocations.Count:n0} collocation(s) for '{result.Word}'.";
    }

    public void Clear(string message)
    {
        CollocationExplorerTitle = "Collocations explorer";
        CollocationExplorerSummary = message;
        CollocationResults = "Collocations will appear here.";
    }

    private void ApplyResult(CollocationExplorerResult result)
    {
        CollocationExplorerTitle = $"Collocations — {result.Word}";
        CollocationExplorerSummary = string.Join(Environment.NewLine,
            $"Window: {result.Window} words per side",
            $"Filter: {FilterText(result.Filter)}",
            $"Minimum count: {result.MinCount:n0}",
            $"Minimum Dice: {DesktopTextFormatter.FormatDouble(result.MinDice)}",
            $"Matched collocates: {result.MatchedCount:n0}",
            $"Shown collocates: {result.Collocations.Count:n0} of {result.MatchedCount:n0}");
        CollocationResults = FormatCollocations(result.Collocations);
    }

    private static Task<CollocationExplorerResult> LoadCollocationsFromApplicationAsync(
        CollocationExplorerRequest request,
        CancellationToken cancellationToken)
    {
        return Task.Run(async () =>
        {
            CollocationExplorerQueryService service = new();
            return await service.GetCollocationsAsync(request, cancellationToken).ConfigureAwait(false);
        }, cancellationToken);
    }

    private static string FormatCollocations(IReadOnlyList<CollocationExplorerItem> collocations)
    {
        if (collocations.Count == 0)
        {
            return "No collocations found for the selected filter and thresholds.";
        }

        IEnumerable<string> lines = collocations.Select((item, index) =>
            $"{index + 1,2}. {DesktopTextFormatter.TrimForColumn(item.Collocate, 18),-18} {item.WordType,-8} {item.Count,5:n0}  L {item.LeftCount,4:n0}  R {item.RightCount,4:n0}  {DesktopTextFormatter.FormatDouble(item.RatePerTarget),6}/t  d {DesktopTextFormatter.FormatDouble(item.AverageDistance),5}  dice {DesktopTextFormatter.FormatDouble(item.DiceCoefficient),5}");
        return string.Join(Environment.NewLine, lines);
    }

    private static string FilterText(CollocationExplorerFilter filter)
    {
        return filter switch
        {
            CollocationExplorerFilter.ContentOnly => "content words only",
            CollocationExplorerFilter.FunctionOnly => "function words only",
            _ => "all words"
        };
    }
}
