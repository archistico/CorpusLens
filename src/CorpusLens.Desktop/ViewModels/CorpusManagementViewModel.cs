using System.Collections.ObjectModel;
using CorpusLens.Application.Storage;
using CorpusLens.Domain.Storage;

namespace CorpusLens.Desktop.ViewModels;

public sealed class CorpusManagementViewModel : ViewModelBase
{
    private readonly Func<ListCorporaRequest, CancellationToken, Task<IReadOnlyList<StoredCorpus>>> _corpusLoader;
    private readonly Func<CreateCorpusRequest, CancellationToken, Task<StoredCorpus>> _corpusCreator;
    private IReadOnlyList<StoredCorpus> _storedCorpora = Array.Empty<StoredCorpus>();
    private CorpusListItemViewModel? _selectedCorpus;
    private string _summary = "Open a database to list its corpora.";
    private string _details = "No corpus selected.";

    public CorpusManagementViewModel(
        Func<ListCorporaRequest, CancellationToken, Task<IReadOnlyList<StoredCorpus>>>? corpusLoader = null,
        Func<CreateCorpusRequest, CancellationToken, Task<StoredCorpus>>? corpusCreator = null)
    {
        _corpusLoader = corpusLoader ?? LoadCorporaFromApplicationAsync;
        _corpusCreator = corpusCreator ?? CreateCorpusFromApplicationAsync;
        SupportedLanguages = CorpusLanguageCatalog.ListSupportedLanguages();
    }

    public ObservableCollection<CorpusListItemViewModel> Corpora { get; } = new();

    public IReadOnlyList<CorpusLanguageOption> SupportedLanguages { get; }

    public CorpusLanguageOption? DefaultLanguage => SupportedLanguages.FirstOrDefault(
        language => string.Equals(language.Code, "en", StringComparison.OrdinalIgnoreCase))
        ?? SupportedLanguages.FirstOrDefault();

    public CorpusListItemViewModel? SelectedCorpus
    {
        get => _selectedCorpus;
        private set => SetProperty(ref _selectedCorpus, value);
    }

    public long? SelectedCorpusId => SelectedCorpus?.Id;

    public string Summary
    {
        get => _summary;
        private set => SetProperty(ref _summary, value);
    }

    public string Details
    {
        get => _details;
        private set => SetProperty(ref _details, value);
    }

    public async Task LoadAsync(
        string databasePath,
        IReadOnlyCollection<RunListItemViewModel> runs,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(databasePath);
        ArgumentNullException.ThrowIfNull(runs);

        long? previousSelectionId = SelectedCorpusId;
        bool allWasSelected = SelectedCorpus?.IsAllCorpora != false;
        _storedCorpora = await _corpusLoader(
            new ListCorporaRequest(databasePath),
            cancellationToken).ConfigureAwait(true);
        cancellationToken.ThrowIfCancellationRequested();

        RebuildItems(runs, previousSelectionId, allWasSelected);
    }

    public async Task<StoredCorpus> CreateAsync(
        string databasePath,
        string? name,
        string? languageCode,
        string? description,
        IReadOnlyCollection<RunListItemViewModel> runs,
        CancellationToken cancellationToken = default)
    {
        if (!TryValidateCreateInput(name, languageCode, out string normalizedName, out string normalizedLanguage, out string error))
        {
            throw new ArgumentException(error);
        }

        StoredCorpus created = await _corpusCreator(
            new CreateCorpusRequest(
                databasePath,
                normalizedName,
                normalizedLanguage,
                description?.Trim()),
            cancellationToken).ConfigureAwait(true);
        cancellationToken.ThrowIfCancellationRequested();

        _storedCorpora = _storedCorpora
            .Append(created)
            .OrderBy(corpus => corpus.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        RebuildItems(runs, created.Id, allWasSelected: false);
        return created;
    }

    public bool TryValidateCreateInput(
        string? name,
        string? languageCode,
        out string normalizedName,
        out string normalizedLanguage,
        out string error)
    {
        normalizedName = name?.Trim() ?? string.Empty;
        normalizedLanguage = string.Empty;
        error = string.Empty;

        if (string.IsNullOrWhiteSpace(normalizedName))
        {
            error = "Enter a corpus name.";
            return false;
        }

        string candidateName = normalizedName;
        if (_storedCorpora.Any(corpus => string.Equals(
            corpus.Name,
            candidateName,
            StringComparison.OrdinalIgnoreCase)))
        {
            error = $"A corpus named '{candidateName}' already exists.";
            return false;
        }

        if (!CorpusLanguageCatalog.TryNormalizeSupportedCode(languageCode, out normalizedLanguage))
        {
            string supported = string.Join(", ", SupportedLanguages.Select(language => language.Code));
            error = $"Choose a supported language: {supported}.";
            return false;
        }

        return true;
    }

    public void SetSelectedCorpus(CorpusListItemViewModel? corpus)
    {
        SelectedCorpus = corpus ?? Corpora.FirstOrDefault();
        OnPropertyChanged(nameof(SelectedCorpusId));
        UpdateDetails();
    }

    public bool IsSelectedCorpusLanguageCompatible(string? languageCode)
    {
        CorpusListItemViewModel? selected = SelectedCorpus;
        if (selected?.Corpus is null)
        {
            return true;
        }

        return CorpusLanguageCatalog.TryNormalizeSupportedCode(languageCode, out string normalizedLanguage)
            && string.Equals(
                selected.Corpus.LanguageCode,
                normalizedLanguage,
                StringComparison.OrdinalIgnoreCase);
    }

    public void Clear(string message)
    {
        _storedCorpora = Array.Empty<StoredCorpus>();
        Corpora.Clear();
        SelectedCorpus = null;
        OnPropertyChanged(nameof(SelectedCorpusId));
        Summary = message;
        Details = "No corpus selected.";
    }

    private void RebuildItems(
        IReadOnlyCollection<RunListItemViewModel> runs,
        long? selectedCorpusId,
        bool allWasSelected)
    {
        Corpora.Clear();
        CorpusListItemViewModel all = new(null, runs.Count);
        Corpora.Add(all);

        foreach (StoredCorpus corpus in _storedCorpora)
        {
            int runCount = runs.Count(run => run.Summary.CorpusId == corpus.Id);
            Corpora.Add(new CorpusListItemViewModel(corpus, runCount));
        }

        CorpusListItemViewModel selected = all;
        if (!allWasSelected && selectedCorpusId is not null)
        {
            selected = Corpora.FirstOrDefault(item => item.Id == selectedCorpusId) ?? all;
        }

        SetSelectedCorpus(selected);
        int corpusCount = _storedCorpora.Count;
        int languageCount = _storedCorpora
            .Select(corpus => corpus.LanguageCode)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count();
        Summary = corpusCount == 0
            ? "No corpora are stored in this database. Create the first corpus below."
            : $"Corpora: {corpusCount:n0} · Languages: {languageCount:n0} · Runs: {runs.Count:n0}";
    }

    private void UpdateDetails()
    {
        CorpusListItemViewModel? selected = SelectedCorpus;
        if (selected is null)
        {
            Details = "No corpus selected.";
            return;
        }

        if (selected.Corpus is null)
        {
            Details = $"Showing all {selected.RunCount:n0} run(s) in the database.";
            return;
        }

        StoredCorpus corpus = selected.Corpus;
        string description = string.IsNullOrWhiteSpace(corpus.Description)
            ? "(no description)"
            : corpus.Description;
        Details = string.Join(
            Environment.NewLine,
            $"ID: {corpus.Id}",
            $"Name: {corpus.Name}",
            $"Language: {corpus.LanguageCode}",
            $"Runs: {selected.RunCount:n0}",
            $"Description: {description}",
            $"Created: {corpus.CreatedAt.LocalDateTime:g}",
            $"Updated: {corpus.UpdatedAt.LocalDateTime:g}");
    }

    private static Task<IReadOnlyList<StoredCorpus>> LoadCorporaFromApplicationAsync(
        ListCorporaRequest request,
        CancellationToken cancellationToken)
    {
        ListCorporaUseCase useCase = new();
        return useCase.ExecuteAsync(request, cancellationToken);
    }

    private static Task<StoredCorpus> CreateCorpusFromApplicationAsync(
        CreateCorpusRequest request,
        CancellationToken cancellationToken)
    {
        CreateCorpusUseCase useCase = new();
        return useCase.ExecuteAsync(request, cancellationToken);
    }
}
