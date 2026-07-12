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
}
