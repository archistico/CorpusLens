using CorpusLens.Domain.Storage;

namespace CorpusLens.Desktop.ViewModels;

public sealed class RunListItemViewModel
{
    public RunListItemViewModel(StoredAnalysisRunSummary summary)
    {
        Summary = summary;
    }

    public StoredAnalysisRunSummary Summary { get; }

    public long Id => Summary.Id;

    public string DisplayTitle => $"Run {Summary.Id} — {Summary.CorpusName}";

    public string DisplaySubtitle => $"{Summary.BookTitle} · {Summary.WordTokenCount:n0} words · {Summary.Status}";

    public override string ToString()
    {
        return $"{Summary.Id} · {Summary.CorpusName} · {Summary.BookTitle}";
    }
}
