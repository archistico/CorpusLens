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
}
