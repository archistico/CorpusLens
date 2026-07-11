using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using CorpusLens.Application.Queries;
using CorpusLens.Desktop.ViewModels;

namespace CorpusLens.Desktop.Views;

public sealed partial class MainWindow : Window
{
    public MainWindow(MainWindowViewModel viewModel)
    {
        DataContext = viewModel;
        Title = "CorpusLens";
        Width = 1280;
        Height = 820;
        MinWidth = 980;
        MinHeight = 640;
        Content = BuildContent(viewModel, this);
    }

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
        TextBlock reportPathText = CreateBoundText(viewModel, nameof(MainWindowViewModel.ReportPath), () => viewModel.ReportPath);
        reportPathText.TextWrapping = TextWrapping.Wrap;

        TextBlock wordExplorerTitleText = CreateBoundText(viewModel, nameof(MainWindowViewModel.WordExplorerTitle), () => viewModel.WordExplorerTitle);
        wordExplorerTitleText.FontSize = 18;
        wordExplorerTitleText.FontWeight = FontWeight.SemiBold;
        TextBlock wordExplorerSummaryText = CreateBoundText(viewModel, nameof(MainWindowViewModel.WordExplorerSummary), () => viewModel.WordExplorerSummary);
        TextBlock wordNextWordsText = CreateBoundText(viewModel, nameof(MainWindowViewModel.WordNextWords), () => viewModel.WordNextWords);
        TextBlock wordPreviousWordsText = CreateBoundText(viewModel, nameof(MainWindowViewModel.WordPreviousWords), () => viewModel.WordPreviousWords);
        TextBlock wordKwicText = CreateBoundText(viewModel, nameof(MainWindowViewModel.WordKwic), () => viewModel.WordKwic);
        TextBlock wordBookDistributionText = CreateBoundText(viewModel, nameof(MainWindowViewModel.WordBookDistribution), () => viewModel.WordBookDistribution);

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
                reportPathText,
                wordExplorerTitleText,
                wordExplorerSummaryText,
                wordNextWordsText,
                wordPreviousWordsText,
                wordKwicText,
                wordBookDistributionText,
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

    private static TextBlock CreateBoundText(MainWindowViewModel viewModel, string propertyName, Func<string> valueProvider)
    {
        TextBlock textBlock = new()
        {
            Text = valueProvider(),
        };

        viewModel.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == propertyName)
            {
                textBlock.Text = valueProvider();
            }
        };

        return textBlock;
    }

    private static ProgressBar CreateBusyProgress(MainWindowViewModel viewModel)
    {
        ProgressBar progressBar = new()
        {
            IsIndeterminate = true,
            IsVisible = viewModel.IsBusy,
            MinWidth = 180,
            Height = 8,
            VerticalAlignment = VerticalAlignment.Center,
        };

        viewModel.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(MainWindowViewModel.IsBusy))
            {
                progressBar.IsVisible = viewModel.IsBusy;
            }
        };

        return progressBar;
    }

    private static void DisableWhileBusy(Button button, MainWindowViewModel viewModel)
    {
        button.IsEnabled = !viewModel.IsBusy;
        viewModel.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(MainWindowViewModel.IsBusy))
            {
                button.IsEnabled = !viewModel.IsBusy;
            }
        };
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
        TextBlock reportPathText,
        TextBlock wordExplorerTitleText,
        TextBlock wordExplorerSummaryText,
        TextBlock wordNextWordsText,
        TextBlock wordPreviousWordsText,
        TextBlock wordKwicText,
        TextBlock wordBookDistributionText,
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

        Border reportCard = new()
        {
            Padding = new Thickness(16),
            BorderBrush = Brushes.LightGray,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Child = new StackPanel
            {
                Spacing = 8,
                Children =
                {
                    new TextBlock { Text = "Report", FontWeight = FontWeight.SemiBold },
                    reportPathText,
                },
            },
        };
        stack.Children.Add(reportCard);
        stack.Children.Add(BuildWordExplorer(
            viewModel,
            wordExplorerTitleText,
            wordExplorerSummaryText,
            wordNextWordsText,
            wordPreviousWordsText,
            wordKwicText,
            wordBookDistributionText));

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

    private static Control BuildWordExplorer(
        MainWindowViewModel viewModel,
        TextBlock titleText,
        TextBlock summaryText,
        TextBlock nextWordsText,
        TextBlock previousWordsText,
        TextBlock kwicText,
        TextBlock bookDistributionText)
    {
        TextBox wordBox = new()
        {
            PlaceholderText = "word, e.g. piazza",
            MinWidth = 240,
        };

        Button searchButton = new()
        {
            Content = "Search word",
            Margin = new Thickness(12, 0, 0, 0),
        };
        searchButton.Click += async (_, _) => await viewModel.SearchWordAsync(wordBox.Text).ConfigureAwait(true);
        wordBox.KeyDown += async (_, args) =>
        {
            if (args.Key == Avalonia.Input.Key.Enter)
            {
                await viewModel.SearchWordAsync(wordBox.Text).ConfigureAwait(true);
            }
        };
        DisableWhileBusy(searchButton, viewModel);

        StackPanel searchRow = new()
        {
            Orientation = Orientation.Horizontal,
            Spacing = 0,
            Children =
            {
                wordBox,
                searchButton,
            },
        };

        Grid relatedCards = new()
        {
            ColumnDefinitions = new ColumnDefinitions("*,*"),
        };
        relatedCards.Children.Add(BuildCard("Next words", nextWordsText, 0, monospace: true));
        relatedCards.Children.Add(BuildCard("Previous words", previousWordsText, 1, monospace: true));

        Grid detailCards = new()
        {
            ColumnDefinitions = new ColumnDefinitions("2*,*"),
        };
        detailCards.Children.Add(BuildCard("KWIC", kwicText, 0, monospace: true));
        detailCards.Children.Add(BuildCard("Books", bookDistributionText, 1, monospace: true));

        Border panel = new()
        {
            Padding = new Thickness(16),
            BorderBrush = Brushes.LightGray,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Child = new StackPanel
            {
                Spacing = 12,
                Children =
                {
                    titleText,
                    searchRow,
                    summaryText,
                    relatedCards,
                    detailCards,
                },
            },
        };

        return panel;
    }

    private static Control BuildCollocationExplorer(
        MainWindowViewModel viewModel,
        TextBlock titleText,
        TextBlock summaryText,
        TextBlock resultsText,
        TextBlock filterText)
    {
        TextBox wordBox = new()
        {
            PlaceholderText = "target word, e.g. piazza",
            MinWidth = 220,
        };
        TextBox windowBox = new()
        {
            Text = "4",
            MinWidth = 52,
        };
        TextBox minCountBox = new()
        {
            Text = "1",
            MinWidth = 52,
        };
        TextBox minDiceBox = new()
        {
            Text = "0",
            MinWidth = 60,
        };
        TextBox limitBox = new()
        {
            Text = "30",
            MinWidth = 60,
        };

        Button allButton = new() { Content = "All" };
        Button contentButton = new() { Content = "Content" };
        Button functionButton = new() { Content = "Function" };
        allButton.Click += (_, _) => viewModel.SetCollocationFilter(CollocationExplorerFilter.All);
        contentButton.Click += (_, _) => viewModel.SetCollocationFilter(CollocationExplorerFilter.ContentOnly);
        functionButton.Click += (_, _) => viewModel.SetCollocationFilter(CollocationExplorerFilter.FunctionOnly);
        DisableWhileBusy(allButton, viewModel);
        DisableWhileBusy(contentButton, viewModel);
        DisableWhileBusy(functionButton, viewModel);

        Button searchButton = new()
        {
            Content = "Search collocations",
            Margin = new Thickness(12, 0, 0, 0),
        };
        searchButton.Click += async (_, _) => await viewModel.SearchCollocationsAsync(
                wordBox.Text,
                windowBox.Text,
                minCountBox.Text,
                minDiceBox.Text,
                limitBox.Text)
            .ConfigureAwait(true);
        wordBox.KeyDown += async (_, args) =>
        {
            if (args.Key == Avalonia.Input.Key.Enter)
            {
                await viewModel.SearchCollocationsAsync(
                        wordBox.Text,
                        windowBox.Text,
                        minCountBox.Text,
                        minDiceBox.Text,
                        limitBox.Text)
                    .ConfigureAwait(true);
            }
        };
        DisableWhileBusy(searchButton, viewModel);

        StackPanel searchRow = new()
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Children =
            {
                wordBox,
                LabeledInput("Window", windowBox),
                LabeledInput("Min count", minCountBox),
                LabeledInput("Min Dice", minDiceBox),
                LabeledInput("Limit", limitBox),
                searchButton,
            },
        };

        StackPanel filterRow = new()
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Children =
            {
                allButton,
                contentButton,
                functionButton,
                filterText,
            },
        };

        Border panel = new()
        {
            Padding = new Thickness(16),
            BorderBrush = Brushes.LightGray,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Child = new StackPanel
            {
                Spacing = 12,
                Children =
                {
                    titleText,
                    searchRow,
                    filterRow,
                    summaryText,
                    BuildCard("Collocations", resultsText, 0, monospace: true),
                },
            },
        };

        return panel;
    }

    private static Control BuildPhraseExplorer(
        MainWindowViewModel viewModel,
        TextBlock titleText,
        TextBlock summaryText,
        TextBlock resultsText,
        TextBlock boundaryText)
    {
        TextBox minNBox = new()
        {
            Text = "2",
            MinWidth = 52,
        };
        TextBox maxNBox = new()
        {
            Text = "5",
            MinWidth = 52,
        };
        TextBox minCountBox = new()
        {
            Text = "3",
            MinWidth = 52,
        };
        TextBox minChaptersBox = new()
        {
            Text = "2",
            MinWidth = 52,
        };
        TextBox limitBox = new()
        {
            Text = "30",
            MinWidth = 60,
        };

        CheckBox contentBoundaryCheck = new()
        {
            Content = "Content boundary",
            IsChecked = viewModel.PhraseContentBoundaryOnly,
            VerticalAlignment = VerticalAlignment.Center,
        };
        contentBoundaryCheck.Click += (_, _) => viewModel.SetPhraseContentBoundary(contentBoundaryCheck.IsChecked == true);

        CheckBox longestOnlyCheck = new()
        {
            Content = "Longest only",
            IsChecked = viewModel.PhraseLongestOnly,
            VerticalAlignment = VerticalAlignment.Center,
        };
        longestOnlyCheck.Click += (_, _) => viewModel.SetPhraseLongestOnly(longestOnlyCheck.IsChecked == true);

        Button searchButton = new()
        {
            Content = "Search phrases",
            Margin = new Thickness(12, 0, 0, 0),
        };
        searchButton.Click += async (_, _) => await viewModel.SearchPhrasesAsync(
                minNBox.Text,
                maxNBox.Text,
                minCountBox.Text,
                minChaptersBox.Text,
                limitBox.Text)
            .ConfigureAwait(true);
        DisableWhileBusy(searchButton, viewModel);

        StackPanel inputRow = new()
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Children =
            {
                LabeledInput("Min N", minNBox),
                LabeledInput("Max N", maxNBox),
                LabeledInput("Min count", minCountBox),
                LabeledInput("Min chapters", minChaptersBox),
                LabeledInput("Limit", limitBox),
                searchButton,
            },
        };

        StackPanel filterRow = new()
        {
            Orientation = Orientation.Horizontal,
            Spacing = 14,
            Children =
            {
                contentBoundaryCheck,
                longestOnlyCheck,
                boundaryText,
            },
        };

        Border panel = new()
        {
            Padding = new Thickness(16),
            BorderBrush = Brushes.LightGray,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Child = new StackPanel
            {
                Spacing = 12,
                Children =
                {
                    titleText,
                    inputRow,
                    filterRow,
                    summaryText,
                    BuildCard("Phrases", resultsText, 0, monospace: true),
                },
            },
        };

        return panel;
    }


    private static Control BuildCompareExplorer(
        MainWindowViewModel viewModel,
        TextBlock titleText,
        TextBlock summaryText,
        TextBlock wordSummaryText,
        TextBlock wordsText,
        TextBlock difficultyText,
        TextBlock wordFilterText,
        TextBlock presenceText)
    {
        ComboBox rightRunBox = new()
        {
            ItemsSource = viewModel.Runs,
            SelectedItem = viewModel.ComparisonRightRun,
            MinWidth = 260,
        };
        rightRunBox.SelectionChanged += (_, _) =>
        {
            viewModel.SetComparisonRightRun(rightRunBox.SelectedItem as RunListItemViewModel);
        };
        viewModel.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(MainWindowViewModel.ComparisonRightRun)
                && !ReferenceEquals(rightRunBox.SelectedItem, viewModel.ComparisonRightRun))
            {
                rightRunBox.SelectedItem = viewModel.ComparisonRightRun;
            }

            if (args.PropertyName == nameof(MainWindowViewModel.IsBusy))
            {
                rightRunBox.IsEnabled = !viewModel.IsBusy;
            }
        };

        TextBox wordBox = new()
        {
            PlaceholderText = "optional word, e.g. amore",
            MinWidth = 220,
        };
        TextBox minCountBox = new()
        {
            Text = "5",
            MinWidth = 52,
        };
        TextBox limitBox = new()
        {
            Text = "30",
            MinWidth = 60,
        };

        Button allWordsButton = new() { Content = "All" };
        Button contentWordsButton = new() { Content = "Content" };
        Button functionWordsButton = new() { Content = "Function" };
        allWordsButton.Click += (_, _) => viewModel.SetComparisonWordFilter(ComparisonWordFilter.All);
        contentWordsButton.Click += (_, _) => viewModel.SetComparisonWordFilter(ComparisonWordFilter.ContentOnly);
        functionWordsButton.Click += (_, _) => viewModel.SetComparisonWordFilter(ComparisonWordFilter.FunctionOnly);
        DisableWhileBusy(allWordsButton, viewModel);
        DisableWhileBusy(contentWordsButton, viewModel);
        DisableWhileBusy(functionWordsButton, viewModel);

        Button presenceAllButton = new() { Content = "All presence" };
        Button sharedOnlyButton = new() { Content = "Shared" };
        Button exclusiveOnlyButton = new() { Content = "Exclusive" };
        presenceAllButton.Click += (_, _) => viewModel.SetComparisonPresenceFilter(ComparisonPresenceFilter.All);
        sharedOnlyButton.Click += (_, _) => viewModel.SetComparisonPresenceFilter(ComparisonPresenceFilter.SharedOnly);
        exclusiveOnlyButton.Click += (_, _) => viewModel.SetComparisonPresenceFilter(ComparisonPresenceFilter.ExclusiveOnly);
        DisableWhileBusy(presenceAllButton, viewModel);
        DisableWhileBusy(sharedOnlyButton, viewModel);
        DisableWhileBusy(exclusiveOnlyButton, viewModel);

        Button compareButton = new()
        {
            Content = "Compare runs",
            Margin = new Thickness(12, 0, 0, 0),
        };
        compareButton.Click += async (_, _) => await viewModel.CompareRunsAsync(
                wordBox.Text,
                minCountBox.Text,
                limitBox.Text)
            .ConfigureAwait(true);
        wordBox.KeyDown += async (_, args) =>
        {
            if (args.Key == Avalonia.Input.Key.Enter)
            {
                await viewModel.CompareRunsAsync(
                        wordBox.Text,
                        minCountBox.Text,
                        limitBox.Text)
                    .ConfigureAwait(true);
            }
        };
        DisableWhileBusy(compareButton, viewModel);

        StackPanel inputRow = new()
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Children =
            {
                LabeledInput("Right run", rightRunBox),
                wordBox,
                LabeledInput("Min count", minCountBox),
                LabeledInput("Limit", limitBox),
                compareButton,
            },
        };

        StackPanel wordFilterRow = new()
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Children =
            {
                allWordsButton,
                contentWordsButton,
                functionWordsButton,
                wordFilterText,
            },
        };

        StackPanel presenceRow = new()
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Children =
            {
                presenceAllButton,
                sharedOnlyButton,
                exclusiveOnlyButton,
                presenceText,
            },
        };

        Grid resultCards = new()
        {
            ColumnDefinitions = new ColumnDefinitions("*,*"),
        };
        resultCards.Children.Add(BuildCard("Word comparison", wordSummaryText, 0, monospace: true));
        resultCards.Children.Add(BuildCard("Difficulty comparison", difficultyText, 1, monospace: true));

        Border panel = new()
        {
            Padding = new Thickness(16),
            BorderBrush = Brushes.LightGray,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Child = new StackPanel
            {
                Spacing = 12,
                Children =
                {
                    titleText,
                    inputRow,
                    wordFilterRow,
                    presenceRow,
                    summaryText,
                    resultCards,
                    BuildCard("Word differences", wordsText, 0, monospace: true),
                },
            },
        };

        return panel;
    }

    private static Control LabeledInput(string label, Control input)
    {
        return new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 2,
            Children =
            {
                new TextBlock { Text = label, FontSize = 11 },
                input,
            },
        };
    }

    private static Control BuildCard(string title, TextBlock body, int column, bool monospace = false)
    {
        body.TextWrapping = TextWrapping.Wrap;
        if (monospace)
        {
            body.FontFamily = new FontFamily("Consolas, Cascadia Mono, Menlo, monospace");
            body.FontSize = 12;
        }

        Border card = new()
        {
            Margin = column == 0 ? new Thickness(0) : new Thickness(16, 0, 0, 0),
            Padding = new Thickness(16),
            BorderBrush = Brushes.LightGray,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Child = new StackPanel
            {
                Spacing = 8,
                Children =
                {
                    new TextBlock { Text = title, FontWeight = FontWeight.SemiBold },
                    body,
                },
            },
        };
        Grid.SetColumn(card, column);
        return card;
    }
}
