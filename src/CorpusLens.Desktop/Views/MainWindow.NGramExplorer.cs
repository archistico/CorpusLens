using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using CorpusLens.Application.Queries;
using CorpusLens.Desktop.ViewModels;

namespace CorpusLens.Desktop.Views;

public sealed partial class MainWindow
{
    private static Control BuildNGramExplorer(
        MainWindowViewModel viewModel,
        TextBlock titleText,
        TextBlock summaryText,
        TextBox resultsText,
        TextBlock sizeText,
        TextBlock filterText,
        TextBlock sortText)
    {
        TextBox termBox = new()
        {
            PlaceholderText = "contained word or phrase (optional)",
            MinWidth = 260,
        };
        TextBox minCountBox = new()
        {
            Text = "2",
            MinWidth = 60,
        };
        TextBox limitBox = new()
        {
            Text = "30",
            MinWidth = 60,
        };

        Button allSizesButton = new() { Content = "All sizes" };
        Button bigramsButton = new() { Content = "Bigrams" };
        Button trigramsButton = new() { Content = "Trigrams" };
        Button fourGramsButton = new() { Content = "4-grams" };
        Button fiveGramsButton = new() { Content = "5-grams" };
        allSizesButton.Click += (_, _) => viewModel.SetNGramSize(null);
        bigramsButton.Click += (_, _) => viewModel.SetNGramSize(2);
        trigramsButton.Click += (_, _) => viewModel.SetNGramSize(3);
        fourGramsButton.Click += (_, _) => viewModel.SetNGramSize(4);
        fiveGramsButton.Click += (_, _) => viewModel.SetNGramSize(5);

        Button allFilterButton = new() { Content = "All" };
        Button contentOnlyButton = new() { Content = "Content only" };
        Button functionOnlyButton = new() { Content = "Function only" };
        Button contentBoundaryButton = new() { Content = "Content boundary" };
        allFilterButton.Click += (_, _) => viewModel.SetNGramFilter(NGramExplorerFilter.All);
        contentOnlyButton.Click += (_, _) => viewModel.SetNGramFilter(NGramExplorerFilter.ContentOnly);
        functionOnlyButton.Click += (_, _) => viewModel.SetNGramFilter(NGramExplorerFilter.FunctionOnly);
        contentBoundaryButton.Click += (_, _) => viewModel.SetNGramFilter(NGramExplorerFilter.ContentBoundary);

        Button countSortButton = new() { Content = "Count" };
        Button frequencySortButton = new() { Content = "Frequency /M" };
        Button documentsSortButton = new() { Content = "Documents" };
        Button textSortButton = new() { Content = "Text" };
        countSortButton.Click += (_, _) => viewModel.SetNGramSort(NGramExplorerSort.Count);
        frequencySortButton.Click += (_, _) => viewModel.SetNGramSort(NGramExplorerSort.FrequencyPerMillion);
        documentsSortButton.Click += (_, _) => viewModel.SetNGramSort(NGramExplorerSort.DocumentCount);
        textSortButton.Click += (_, _) => viewModel.SetNGramSort(NGramExplorerSort.Text);

        Button[] optionButtons =
        {
            allSizesButton,
            bigramsButton,
            trigramsButton,
            fourGramsButton,
            fiveGramsButton,
            allFilterButton,
            contentOnlyButton,
            functionOnlyButton,
            contentBoundaryButton,
            countSortButton,
            frequencySortButton,
            documentsSortButton,
            textSortButton,
        };
        foreach (Button button in optionButtons)
        {
            DisableWhileBusy(button, viewModel);
        }

        Button searchButton = new()
        {
            Content = "Search n-grams",
            Margin = new Thickness(12, 0, 0, 0),
        };
        searchButton.Click += async (_, _) => await viewModel.SearchNGramsAsync(
                termBox.Text,
                minCountBox.Text,
                limitBox.Text)
            .ConfigureAwait(true);
        termBox.KeyDown += async (_, args) =>
        {
            if (args.Key == Key.Enter)
            {
                await viewModel.SearchNGramsAsync(
                        termBox.Text,
                        minCountBox.Text,
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
                termBox,
                LabeledInput("Min count", minCountBox),
                LabeledInput("Limit", limitBox),
                searchButton,
            },
        };

        StackPanel sizeRow = new()
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Children =
            {
                allSizesButton,
                bigramsButton,
                trigramsButton,
                fourGramsButton,
                fiveGramsButton,
                sizeText,
            },
        };

        StackPanel filterRow = new()
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Children =
            {
                allFilterButton,
                contentOnlyButton,
                functionOnlyButton,
                contentBoundaryButton,
                filterText,
            },
        };

        StackPanel sortRow = new()
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Children =
            {
                countSortButton,
                frequencySortButton,
                documentsSortButton,
                textSortButton,
                sortText,
            },
        };

        Border resultsCard = new()
        {
            Padding = new Thickness(12),
            BorderBrush = Brushes.LightGray,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Child = new StackPanel
            {
                Spacing = 8,
                Children =
                {
                    new TextBlock
                    {
                        Text = "N-grams — select text and press Ctrl+C to copy",
                        FontWeight = FontWeight.SemiBold,
                    },
                    resultsText,
                },
            },
        };

        return new Border
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
                    sizeRow,
                    filterRow,
                    sortRow,
                    summaryText,
                    resultsCard,
                },
            },
        };
    }
}
