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
}
