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
        Width = 1180;
        Height = 760;
        MinWidth = 900;
        MinHeight = 600;
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
        TextBlock tokenIndexText = CreateBoundText(viewModel, nameof(MainWindowViewModel.TokenIndexSummary), () => viewModel.TokenIndexSummary);
        TextBlock queryPathText = CreateBoundText(viewModel, nameof(MainWindowViewModel.QueryPathSummary), () => viewModel.QueryPathSummary);
        TextBlock reportPathText = CreateBoundText(viewModel, nameof(MainWindowViewModel.ReportPath), () => viewModel.ReportPath);
        reportPathText.TextWrapping = TextWrapping.Wrap;

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
            Content = BuildMainArea(runTitleText, runSubtitleText, coreMetricsText, tokenIndexText, queryPathText, reportPathText),
        };
        Grid.SetColumn(mainArea, 1);
        body.Children.Add(mainArea);

        Border statusBar = new()
        {
            Padding = new Thickness(12, 8),
            BorderBrush = Brushes.LightGray,
            BorderThickness = new Thickness(0, 1, 0, 0),
            Child = statusText,
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
        };
        Grid.SetRow(runs, 1);
        panel.Children.Add(runs);

        return panel;
    }

    private static Control BuildMainArea(
        TextBlock runTitleText,
        TextBlock runSubtitleText,
        TextBlock coreMetricsText,
        TextBlock tokenIndexText,
        TextBlock queryPathText,
        TextBlock reportPathText)
    {
        StackPanel stack = new()
        {
            Margin = new Thickness(24),
            Spacing = 18,
        };

        stack.Children.Add(runTitleText);
        stack.Children.Add(runSubtitleText);

        Grid cards = new()
        {
            ColumnDefinitions = new ColumnDefinitions("*,*,*"),
        };
        cards.Children.Add(BuildCard("Core metrics", coreMetricsText, 0));
        cards.Children.Add(BuildCard("Token index", tokenIndexText, 1));
        cards.Children.Add(BuildCard("Query path", queryPathText, 2));
        stack.Children.Add(cards);

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

        return stack;
    }

    private static Control BuildCard(string title, TextBlock body, int column)
    {
        body.TextWrapping = TextWrapping.Wrap;

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
