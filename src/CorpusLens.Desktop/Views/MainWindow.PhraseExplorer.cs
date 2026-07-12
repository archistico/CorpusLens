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
}
