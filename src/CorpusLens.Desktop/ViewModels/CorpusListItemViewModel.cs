using CorpusLens.Domain.Storage;

namespace CorpusLens.Desktop.ViewModels;

public sealed class CorpusListItemViewModel
{
    public CorpusListItemViewModel(StoredCorpus? corpus, int runCount)
    {
        Corpus = corpus;
        RunCount = runCount;
    }

    public StoredCorpus? Corpus { get; }

    public long? Id => Corpus?.Id;

    public bool IsAllCorpora => Corpus is null;

    public int RunCount { get; }

    public string Name => Corpus?.Name ?? "All corpora";

    public string LanguageCode => Corpus?.LanguageCode ?? string.Empty;

    public string DisplayTitle => IsAllCorpora
        ? "All corpora"
        : Corpus!.Name;

    public string DisplaySubtitle => IsAllCorpora
        ? $"{RunCount:n0} run(s) in database"
        : $"{Corpus!.LanguageCode} · {RunCount:n0} run(s)";

    public override string ToString()
    {
        return IsAllCorpora
            ? $"All corpora ({RunCount:n0} runs)"
            : $"{Corpus!.Name} [{Corpus.LanguageCode}] ({RunCount:n0} runs)";
    }
}
