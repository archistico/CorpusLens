using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using CorpusLens.Application.Queries;
using CorpusLens.Desktop.ViewModels;

namespace CorpusLens.Desktop.Views;

public sealed partial class MainWindow
{
    private static Control BuildContent(MainWindowViewModel viewModel, Window window)
    {
        TextBlock databasePathText = CreateBoundText(viewModel, nameof(MainWindowViewModel.DatabasePath), () => viewModel.DatabasePath);
        databasePathText.VerticalAlignment = VerticalAlignment.Center;
        databasePathText.TextTrimming = TextTrimming.CharacterEllipsis;
        databasePathText.Margin = new Thickness(12, 0, 0, 0);

        TextBlock statusText = CreateBoundText(viewModel, nameof(MainWindowViewModel.StatusMessage), () => viewModel.StatusMessage);
        TextBlock runTitleText = CreateBoundText(viewModel, nameof(MainWindowViewModel.RunTitle), () => viewModel.RunTitle);
        runTitleText.FontSize = 26;
        runTitleText.FontWeight = FontWeight.SemiBold;

        TextBlock runSubtitleText = CreateBoundText(viewModel, nameof(MainWindowViewModel.RunSubtitle), () => viewModel.RunSubtitle);
        runSubtitleText.FontSize = 14;
        runSubtitleText.TextWrapping = TextWrapping.Wrap;

        TextBlock coreMetricsText = CreateBoundText(viewModel, nameof(MainWindowViewModel.CoreMetrics), () => viewModel.CoreMetrics);
        TextBlock profileSummaryText = CreateBoundText(viewModel, nameof(MainWindowViewModel.ProfileSummary), () => viewModel.ProfileSummary);
        TextBlock difficultyText = CreateBoundText(viewModel, nameof(MainWindowViewModel.DifficultySummary), () => viewModel.DifficultySummary);
        TextBlock contentWordsText = CreateBoundText(viewModel, nameof(MainWindowViewModel.TopContentWords), () => viewModel.TopContentWords);
        TextBlock functionWordsText = CreateBoundText(viewModel, nameof(MainWindowViewModel.TopFunctionWords), () => viewModel.TopFunctionWords);
        TextBlock phrasesText = CreateBoundText(viewModel, nameof(MainWindowViewModel.RecurringPhrases), () => viewModel.RecurringPhrases);
        TextBlock tokenIndexText = CreateBoundText(viewModel, nameof(MainWindowViewModel.TokenIndexSummary), () => viewModel.TokenIndexSummary);
        TextBlock queryPathText = CreateBoundText(viewModel, nameof(MainWindowViewModel.QueryPathSummary), () => viewModel.QueryPathSummary);
        TextBlock artifactTitleText = CreateBoundText(viewModel, nameof(MainWindowViewModel.ArtifactExplorerTitle), () => viewModel.ArtifactExplorerTitle);
        artifactTitleText.FontSize = 18;
        artifactTitleText.FontWeight = FontWeight.SemiBold;
        TextBlock artifactSummaryText = CreateBoundText(viewModel, nameof(MainWindowViewModel.ArtifactExplorerSummary), () => viewModel.ArtifactExplorerSummary);
        TextBlock artifactDetailsText = CreateBoundText(viewModel, nameof(MainWindowViewModel.ArtifactDetails), () => viewModel.ArtifactDetails);
        artifactDetailsText.FontFamily = new FontFamily("Consolas, Cascadia Mono, Menlo, monospace");
        artifactDetailsText.FontSize = 12;
        artifactDetailsText.TextWrapping = TextWrapping.Wrap;
        TextBlock artifactOutputDirectoryText = CreateBoundText(viewModel, nameof(MainWindowViewModel.OutputDirectorySummary), () => viewModel.OutputDirectorySummary);
        artifactOutputDirectoryText.TextWrapping = TextWrapping.Wrap;

        TextBlock booksExplorerTitleText = CreateBoundText(viewModel, nameof(MainWindowViewModel.BooksExplorerTitle), () => viewModel.BooksExplorerTitle);
        booksExplorerTitleText.FontSize = 18;
        booksExplorerTitleText.FontWeight = FontWeight.SemiBold;
        TextBlock booksExplorerSummaryText = CreateBoundText(viewModel, nameof(MainWindowViewModel.BooksExplorerSummary), () => viewModel.BooksExplorerSummary);
        TextBlock runBookDetailsText = CreateBoundText(viewModel, nameof(MainWindowViewModel.RunBookDetails), () => viewModel.RunBookDetails);

        TextBlock wordExplorerTitleText = CreateBoundText(viewModel, nameof(MainWindowViewModel.WordExplorerTitle), () => viewModel.WordExplorerTitle);
        wordExplorerTitleText.FontSize = 18;
        wordExplorerTitleText.FontWeight = FontWeight.SemiBold;
        TextBlock wordExplorerSummaryText = CreateBoundText(viewModel, nameof(MainWindowViewModel.WordExplorerSummary), () => viewModel.WordExplorerSummary);
        TextBlock wordNextWordsText = CreateBoundText(viewModel, nameof(MainWindowViewModel.WordNextWords), () => viewModel.WordNextWords);
        TextBlock wordPreviousWordsText = CreateBoundText(viewModel, nameof(MainWindowViewModel.WordPreviousWords), () => viewModel.WordPreviousWords);
        TextBlock wordKwicText = CreateBoundText(viewModel, nameof(MainWindowViewModel.WordKwic), () => viewModel.WordKwic);
        TextBlock wordBookDistributionText = CreateBoundText(viewModel, nameof(MainWindowViewModel.WordBookDistribution), () => viewModel.WordBookDistribution);

        TextBlock ngramTitleText = CreateBoundText(viewModel, nameof(MainWindowViewModel.NGramExplorerTitle), () => viewModel.NGramExplorerTitle);
        ngramTitleText.FontSize = 18;
        ngramTitleText.FontWeight = FontWeight.SemiBold;
        TextBlock ngramSummaryText = CreateBoundText(viewModel, nameof(MainWindowViewModel.NGramExplorerSummary), () => viewModel.NGramExplorerSummary);
        TextBox ngramResultsText = CreateBoundReadOnlyTextBox(viewModel, nameof(MainWindowViewModel.NGramResults), () => viewModel.NGramResults);
        TextBlock ngramSizeText = CreateBoundText(viewModel, nameof(MainWindowViewModel.NGramSizeLabel), () => viewModel.NGramSizeLabel);
        TextBlock ngramFilterText = CreateBoundText(viewModel, nameof(MainWindowViewModel.NGramFilterLabel), () => viewModel.NGramFilterLabel);
        TextBlock ngramSortText = CreateBoundText(viewModel, nameof(MainWindowViewModel.NGramSortLabel), () => viewModel.NGramSortLabel);

        TextBlock collocationTitleText = CreateBoundText(viewModel, nameof(MainWindowViewModel.CollocationExplorerTitle), () => viewModel.CollocationExplorerTitle);
        collocationTitleText.FontSize = 18;
        collocationTitleText.FontWeight = FontWeight.SemiBold;
        TextBlock collocationSummaryText = CreateBoundText(viewModel, nameof(MainWindowViewModel.CollocationExplorerSummary), () => viewModel.CollocationExplorerSummary);
        TextBlock collocationResultsText = CreateBoundText(viewModel, nameof(MainWindowViewModel.CollocationResults), () => viewModel.CollocationResults);
        TextBlock collocationFilterText = CreateBoundText(viewModel, nameof(MainWindowViewModel.CollocationFilterLabel), () => viewModel.CollocationFilterLabel);

        TextBlock phraseTitleText = CreateBoundText(viewModel, nameof(MainWindowViewModel.PhraseExplorerTitle), () => viewModel.PhraseExplorerTitle);
        phraseTitleText.FontSize = 18;
        phraseTitleText.FontWeight = FontWeight.SemiBold;
        TextBlock phraseSummaryText = CreateBoundText(viewModel, nameof(MainWindowViewModel.PhraseExplorerSummary), () => viewModel.PhraseExplorerSummary);
        TextBlock phraseResultsText = CreateBoundText(viewModel, nameof(MainWindowViewModel.PhraseResults), () => viewModel.PhraseResults);
        TextBlock phraseBoundaryText = CreateBoundText(viewModel, nameof(MainWindowViewModel.PhraseBoundaryLabel), () => viewModel.PhraseBoundaryLabel);

        TextBlock comparisonTitleText = CreateBoundText(viewModel, nameof(MainWindowViewModel.ComparisonExplorerTitle), () => viewModel.ComparisonExplorerTitle);
        comparisonTitleText.FontSize = 18;
        comparisonTitleText.FontWeight = FontWeight.SemiBold;
        TextBlock comparisonSummaryText = CreateBoundText(viewModel, nameof(MainWindowViewModel.ComparisonExplorerSummary), () => viewModel.ComparisonExplorerSummary);
        TextBlock comparisonWordSummaryText = CreateBoundText(viewModel, nameof(MainWindowViewModel.ComparisonWordSummary), () => viewModel.ComparisonWordSummary);
        TextBlock comparisonWordsText = CreateBoundText(viewModel, nameof(MainWindowViewModel.ComparisonWords), () => viewModel.ComparisonWords);
        TextBlock comparisonDifficultyText = CreateBoundText(viewModel, nameof(MainWindowViewModel.ComparisonDifficulty), () => viewModel.ComparisonDifficulty);
        TextBlock comparisonWordFilterText = CreateBoundText(viewModel, nameof(MainWindowViewModel.ComparisonWordFilterLabel), () => viewModel.ComparisonWordFilterLabel);
        TextBlock comparisonPresenceText = CreateBoundText(viewModel, nameof(MainWindowViewModel.ComparisonPresenceLabel), () => viewModel.ComparisonPresenceLabel);

        Grid root = new()
        {
            RowDefinitions = new RowDefinitions("Auto,*,Auto"),
        };

        Border topBar = new()
        {
            Padding = new Thickness(16, 12),
            BorderBrush = Brushes.LightGray,
            BorderThickness = new Thickness(0, 0, 0, 1),
            Child = BuildTopBar(viewModel, window, databasePathText),
        };
        Grid.SetRow(topBar, 0);
        root.Children.Add(topBar);

        Grid body = new()
        {
            ColumnDefinitions = new ColumnDefinitions("320,*"),
        };
        Grid.SetRow(body, 1);
        root.Children.Add(body);

        Border runsPanel = new()
        {
            Padding = new Thickness(16),
            BorderBrush = Brushes.LightGray,
            BorderThickness = new Thickness(0, 0, 1, 0),
            Child = BuildRunsPanel(viewModel),
        };
        Grid.SetColumn(runsPanel, 0);
        body.Children.Add(runsPanel);

        ScrollViewer mainArea = new()
        {
            Content = BuildMainArea(
                runTitleText,
                runSubtitleText,
                coreMetricsText,
                profileSummaryText,
                difficultyText,
                contentWordsText,
                functionWordsText,
                phrasesText,
                tokenIndexText,
                queryPathText,
                artifactTitleText,
                artifactSummaryText,
                artifactDetailsText,
                artifactOutputDirectoryText,
                booksExplorerTitleText,
                booksExplorerSummaryText,
                runBookDetailsText,
                wordExplorerTitleText,
                wordExplorerSummaryText,
                wordNextWordsText,
                wordPreviousWordsText,
                wordKwicText,
                wordBookDistributionText,
                ngramTitleText,
                ngramSummaryText,
                ngramResultsText,
                ngramSizeText,
                ngramFilterText,
                ngramSortText,
                collocationTitleText,
                collocationSummaryText,
                collocationResultsText,
                collocationFilterText,
                phraseTitleText,
                phraseSummaryText,
                phraseResultsText,
                phraseBoundaryText,
                comparisonTitleText,
                comparisonSummaryText,
                comparisonWordSummaryText,
                comparisonWordsText,
                comparisonDifficultyText,
                comparisonWordFilterText,
                comparisonPresenceText,
                viewModel),
        };
        Grid.SetColumn(mainArea, 1);
        body.Children.Add(mainArea);

        ProgressBar progressBar = CreateBusyProgress(viewModel);
        Grid statusGrid = new()
        {
            ColumnDefinitions = new ColumnDefinitions("*,220"),
        };
        Grid.SetColumn(statusText, 0);
        statusGrid.Children.Add(statusText);
        Grid.SetColumn(progressBar, 1);
        statusGrid.Children.Add(progressBar);

        Border statusBar = new()
        {
            Padding = new Thickness(12, 8),
            BorderBrush = Brushes.LightGray,
            BorderThickness = new Thickness(0, 1, 0, 0),
            Child = statusGrid,
        };
        Grid.SetRow(statusBar, 2);
        root.Children.Add(statusBar);

        return root;
    }
    private static Control BuildTopBar(MainWindowViewModel viewModel, Window window, TextBlock databasePathText)
    {
        Grid topGrid = new()
        {
            ColumnDefinitions = new ColumnDefinitions("Auto,Auto,*,Auto"),
        };

        TextBlock title = new()
        {
            Text = "CorpusLens",
            FontSize = 22,
            FontWeight = FontWeight.SemiBold,
            VerticalAlignment = VerticalAlignment.Center,
        };
        Grid.SetColumn(title, 0);
        topGrid.Children.Add(title);

        Button openButton = new()
        {
            Content = "Open database",
            Margin = new Thickness(12, 0, 0, 0),
        };
        openButton.Click += async (_, _) => await OpenDatabaseAsync(viewModel, window).ConfigureAwait(true);
        DisableWhileBusy(openButton, viewModel);
        Grid.SetColumn(openButton, 1);
        topGrid.Children.Add(openButton);

        Grid.SetColumn(databasePathText, 2);
        topGrid.Children.Add(databasePathText);

        Button refreshButton = new()
        {
            Content = "Refresh",
            Margin = new Thickness(12, 0, 0, 0),
        };
        refreshButton.Click += async (_, _) => await viewModel.RefreshRunsAsync().ConfigureAwait(true);
        DisableWhileBusy(refreshButton, viewModel);
        Grid.SetColumn(refreshButton, 3);
        topGrid.Children.Add(refreshButton);

        return topGrid;
    }
    private static async Task OpenDatabaseAsync(MainWindowViewModel viewModel, Window window)
    {
        IReadOnlyList<IStorageFile> files = await window.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open CorpusLens database",
            AllowMultiple = false,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("SQLite database")
                {
                    Patterns = new[] { "*.db", "*.sqlite", "*.sqlite3" },
                },
                FilePickerFileTypes.All,
            },
        }).ConfigureAwait(true);

        if (files.Count == 0)
        {
            return;
        }

        string? localPath = files[0].Path.LocalPath;
        if (!string.IsNullOrWhiteSpace(localPath))
        {
            await viewModel.OpenDatabaseAsync(localPath).ConfigureAwait(true);
        }
    }
    private static Control BuildRunsPanel(MainWindowViewModel viewModel)
    {
        Grid panel = new()
        {
            RowDefinitions = new RowDefinitions("Auto,*"),
        };

        TextBlock heading = new()
        {
            Text = "Runs",
            FontSize = 16,
            FontWeight = FontWeight.SemiBold,
            Margin = new Thickness(0, 0, 0, 12),
        };
        Grid.SetRow(heading, 0);
        panel.Children.Add(heading);

        ListBox runs = new()
        {
            ItemsSource = viewModel.Runs,
            IsEnabled = !viewModel.IsBusy,
        };
        runs.SelectionChanged += async (_, _) =>
        {
            if (runs.SelectedItem is RunListItemViewModel selectedRun
                && !ReferenceEquals(selectedRun, viewModel.SelectedRun))
            {
                await viewModel.SelectRunAsync(selectedRun).ConfigureAwait(true);
            }
        };
        viewModel.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(MainWindowViewModel.SelectedRun)
                && !ReferenceEquals(runs.SelectedItem, viewModel.SelectedRun))
            {
                runs.SelectedItem = viewModel.SelectedRun;
            }

            if (args.PropertyName == nameof(MainWindowViewModel.IsBusy))
            {
                runs.IsEnabled = !viewModel.IsBusy;
            }
        };
        Grid.SetRow(runs, 1);
        panel.Children.Add(runs);

        return panel;
    }
    private static Control BuildMainArea(
        TextBlock runTitleText,
        TextBlock runSubtitleText,
        TextBlock coreMetricsText,
        TextBlock profileSummaryText,
        TextBlock difficultyText,
        TextBlock contentWordsText,
        TextBlock functionWordsText,
        TextBlock phrasesText,
        TextBlock tokenIndexText,
        TextBlock queryPathText,
        TextBlock artifactTitleText,
        TextBlock artifactSummaryText,
        TextBlock artifactDetailsText,
        TextBlock artifactOutputDirectoryText,
        TextBlock booksExplorerTitleText,
        TextBlock booksExplorerSummaryText,
        TextBlock runBookDetailsText,
        TextBlock wordExplorerTitleText,
        TextBlock wordExplorerSummaryText,
        TextBlock wordNextWordsText,
        TextBlock wordPreviousWordsText,
        TextBlock wordKwicText,
        TextBlock wordBookDistributionText,
        TextBlock ngramTitleText,
        TextBlock ngramSummaryText,
        TextBox ngramResultsText,
        TextBlock ngramSizeText,
        TextBlock ngramFilterText,
        TextBlock ngramSortText,
        TextBlock collocationTitleText,
        TextBlock collocationSummaryText,
        TextBlock collocationResultsText,
        TextBlock collocationFilterText,
        TextBlock phraseTitleText,
        TextBlock phraseSummaryText,
        TextBlock phraseResultsText,
        TextBlock phraseBoundaryText,
        TextBlock comparisonTitleText,
        TextBlock comparisonSummaryText,
        TextBlock comparisonWordSummaryText,
        TextBlock comparisonWordsText,
        TextBlock comparisonDifficultyText,
        TextBlock comparisonWordFilterText,
        TextBlock comparisonPresenceText,
        MainWindowViewModel viewModel)
    {
        StackPanel stack = new()
        {
            Margin = new Thickness(24),
            Spacing = 18,
        };

        stack.Children.Add(runTitleText);
        stack.Children.Add(runSubtitleText);

        Grid topCards = new()
        {
            ColumnDefinitions = new ColumnDefinitions("*,*,*"),
        };
        topCards.Children.Add(BuildCard("Core metrics", coreMetricsText, 0));
        topCards.Children.Add(BuildCard("Corpus profile", profileSummaryText, 1));
        topCards.Children.Add(BuildCard("Difficulty", difficultyText, 2));
        stack.Children.Add(topCards);

        Grid wordCards = new()
        {
            ColumnDefinitions = new ColumnDefinitions("*,*"),
        };
        wordCards.Children.Add(BuildCard("Top content words", contentWordsText, 0, monospace: true));
        wordCards.Children.Add(BuildCard("Top function words", functionWordsText, 1, monospace: true));
        stack.Children.Add(wordCards);

        Grid lowerCards = new()
        {
            ColumnDefinitions = new ColumnDefinitions("2*,*,*"),
        };
        lowerCards.Children.Add(BuildCard("Recurring phrases", phrasesText, 0, monospace: true));
        lowerCards.Children.Add(BuildCard("Token index", tokenIndexText, 1));
        lowerCards.Children.Add(BuildCard("Query path", queryPathText, 2));
        stack.Children.Add(lowerCards);

        stack.Children.Add(BuildArtifactExplorer(
            viewModel,
            artifactTitleText,
            artifactSummaryText,
            artifactDetailsText,
            artifactOutputDirectoryText));
        stack.Children.Add(BuildBooksExplorer(
            viewModel,
            booksExplorerTitleText,
            booksExplorerSummaryText,
            runBookDetailsText));
        stack.Children.Add(BuildWordExplorer(
            viewModel,
            wordExplorerTitleText,
            wordExplorerSummaryText,
            wordNextWordsText,
            wordPreviousWordsText,
            wordKwicText,
            wordBookDistributionText));

        stack.Children.Add(BuildNGramExplorer(
            viewModel,
            ngramTitleText,
            ngramSummaryText,
            ngramResultsText,
            ngramSizeText,
            ngramFilterText,
            ngramSortText));

        stack.Children.Add(BuildCollocationExplorer(
            viewModel,
            collocationTitleText,
            collocationSummaryText,
            collocationResultsText,
            collocationFilterText));

        stack.Children.Add(BuildPhraseExplorer(
            viewModel,
            phraseTitleText,
            phraseSummaryText,
            phraseResultsText,
            phraseBoundaryText));

        stack.Children.Add(BuildCompareExplorer(
            viewModel,
            comparisonTitleText,
            comparisonSummaryText,
            comparisonWordSummaryText,
            comparisonWordsText,
            comparisonDifficultyText,
            comparisonWordFilterText,
            comparisonPresenceText));

        return stack;
    }
}
