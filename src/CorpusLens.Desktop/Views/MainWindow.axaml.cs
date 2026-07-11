using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
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
        Content = BuildContent(viewModel);
    }

    private static Control BuildContent(MainWindowViewModel viewModel)
    {
        TextBlock databasePathText = new()
        {
            Text = viewModel.DatabasePath,
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis,
            Margin = new Thickness(12, 0, 0, 0),
        };

        TextBlock selectedRunText = new()
        {
            Text = viewModel.SelectedRun,
        };

        TextBlock statusText = new()
        {
            Text = viewModel.StatusMessage,
        };

        viewModel.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(MainWindowViewModel.DatabasePath))
            {
                databasePathText.Text = viewModel.DatabasePath;
            }
            else if (args.PropertyName == nameof(MainWindowViewModel.SelectedRun))
            {
                selectedRunText.Text = viewModel.SelectedRun;
            }
            else if (args.PropertyName == nameof(MainWindowViewModel.StatusMessage))
            {
                statusText.Text = viewModel.StatusMessage;
            }
        };

        Grid root = new()
        {
            RowDefinitions = new RowDefinitions("Auto,*,Auto"),
        };

        Border topBar = new()
        {
            Padding = new Thickness(16, 12),
            BorderBrush = Brushes.LightGray,
            BorderThickness = new Thickness(0, 0, 0, 1),
            Child = BuildTopBar(viewModel, databasePathText),
        };
        Grid.SetRow(topBar, 0);
        root.Children.Add(topBar);

        Grid body = new()
        {
            ColumnDefinitions = new ColumnDefinitions("280,*"),
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
            Content = BuildMainArea(viewModel, selectedRunText),
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

    private static Control BuildTopBar(MainWindowViewModel viewModel, TextBlock databasePathText)
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
            Command = viewModel.OpenDatabaseCommand,
            Margin = new Thickness(12, 0, 0, 0),
        };
        Grid.SetColumn(openButton, 1);
        topGrid.Children.Add(openButton);

        Grid.SetColumn(databasePathText, 2);
        topGrid.Children.Add(databasePathText);

        Button refreshButton = new()
        {
            Content = "Refresh",
            Command = viewModel.RefreshCommand,
            Margin = new Thickness(12, 0, 0, 0),
        };
        Grid.SetColumn(refreshButton, 3);
        topGrid.Children.Add(refreshButton);

        return topGrid;
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
        runs.SelectionChanged += (_, _) =>
        {
            if (runs.SelectedItem is string selectedRun)
            {
                viewModel.SelectedRun = selectedRun;
            }
        };
        Grid.SetRow(runs, 1);
        panel.Children.Add(runs);

        return panel;
    }

    private static Control BuildMainArea(MainWindowViewModel viewModel, TextBlock selectedRunText)
    {
        StackPanel stack = new()
        {
            Margin = new Thickness(24),
            Spacing = 18,
        };

        stack.Children.Add(new TextBlock
        {
            Text = viewModel.EmptyStateTitle,
            FontSize = 28,
            FontWeight = FontWeight.SemiBold,
        });

        stack.Children.Add(new TextBlock
        {
            Text = viewModel.EmptyStateMessage,
            FontSize = 15,
            TextWrapping = TextWrapping.Wrap,
            MaxWidth = 760,
        });

        Grid cards = new()
        {
            ColumnDefinitions = new ColumnDefinitions("*,*,*"),
        };
        cards.Children.Add(BuildCard("Run dashboard", "Summary, profile and health checks will appear here.", 0));
        cards.Children.Add(BuildCard("Word explorer", "Word detail, KWIC and book distribution will follow.", 1));
        cards.Children.Add(BuildCard("Analysis tools", "Collocations, phrases and comparisons will be added incrementally.", 2));
        stack.Children.Add(cards);

        Border selectedRun = new()
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
                    new TextBlock { Text = "Selected run", FontWeight = FontWeight.SemiBold },
                    selectedRunText,
                },
            },
        };
        stack.Children.Add(selectedRun);

        return stack;
    }

    private static Control BuildCard(string title, string message, int column)
    {
        Border card = new()
        {
            Margin = column == 0 ? new Thickness(0) : new Thickness(16, 0, 0, 0),
            Padding = new Thickness(16),
            BorderBrush = Brushes.LightGray,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Child = new StackPanel
            {
                Spacing = 6,
                Children =
                {
                    new TextBlock { Text = title, FontWeight = FontWeight.SemiBold },
                    new TextBlock { Text = message, TextWrapping = TextWrapping.Wrap },
                },
            },
        };
        Grid.SetColumn(card, column);
        return card;
    }
}
