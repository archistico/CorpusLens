using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform.Storage;
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
