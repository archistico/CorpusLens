using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using CorpusLens.Desktop.ViewModels;

namespace CorpusLens.Desktop.Views;

public sealed partial class MainWindow
{
    private static Control BuildEpubAnalysis(MainWindowViewModel viewModel, Window window)
    {
        TextBlock corpusSummary = CreateBoundText(
            viewModel,
            nameof(MainWindowViewModel.EpubAnalysisCorpusSummary),
            () => viewModel.EpubAnalysisCorpusSummary);
        corpusSummary.TextWrapping = TextWrapping.Wrap;
        corpusSummary.FontWeight = FontWeight.SemiBold;

        TextBox inputFolder = new()
        {
            PlaceholderText = "Folder containing EPUB files",
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };
        Button browseInput = new()
        {
            Content = "Browse...",
            MinWidth = 90,
        };
        DisableWhileBusy(browseInput, viewModel);
        browseInput.Click += async (_, _) =>
        {
            string? selected = await SelectFolderAsync(window, "Choose EPUB input folder").ConfigureAwait(true);
            if (!string.IsNullOrWhiteSpace(selected))
            {
                inputFolder.Text = selected;
            }
        };

        TextBox outputFolder = new()
        {
            PlaceholderText = "Folder for report.md, CSV files and extracted_text.txt",
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };
        Button browseOutput = new()
        {
            Content = "Browse...",
            MinWidth = 90,
        };
        DisableWhileBusy(browseOutput, viewModel);
        browseOutput.Click += async (_, _) =>
        {
            string? selected = await SelectFolderAsync(window, "Choose analysis output folder").ConfigureAwait(true);
            if (!string.IsNullOrWhiteSpace(selected))
            {
                outputFolder.Text = selected;
            }
        };

        CheckBox recursive = new()
        {
            Content = "Include EPUB files in subfolders",
        };
        CheckBox confirmation = new()
        {
            Content = "I confirm creation of artifacts and a persistent analysis run in the selected corpus.",
        };

        Button startButton = new()
        {
            Content = "Analyze EPUB folder",
            MinWidth = 170,
            HorizontalAlignment = HorizontalAlignment.Left,
        };
        DisableWhileBusy(startButton, viewModel);

        Button cancelButton = new()
        {
            Content = "Cancel analysis",
            MinWidth = 120,
            IsEnabled = viewModel.IsBusy,
        };
        cancelButton.Click += (_, _) => viewModel.CancelCurrentOperation();
        viewModel.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(MainWindowViewModel.IsBusy))
            {
                cancelButton.IsEnabled = viewModel.IsBusy;
                inputFolder.IsEnabled = !viewModel.IsBusy;
                outputFolder.IsEnabled = !viewModel.IsBusy;
                recursive.IsEnabled = !viewModel.IsBusy;
                confirmation.IsEnabled = !viewModel.IsBusy;
            }
        };

        startButton.Click += async (_, _) =>
        {
            await viewModel.AnalyzeEpubFolderAsync(
                inputFolder.Text,
                outputFolder.Text,
                recursive.IsChecked == true,
                confirmation.IsChecked == true).ConfigureAwait(true);
            if (viewModel.EpubAnalysisProgressPercent == 100)
            {
                confirmation.IsChecked = false;
            }
        };

        ProgressBar analysisProgress = new()
        {
            Minimum = 0,
            Maximum = 100,
            Value = viewModel.EpubAnalysisProgressPercent,
            Height = 10,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };
        TextBlock progressSummary = CreateBoundText(
            viewModel,
            nameof(MainWindowViewModel.EpubAnalysisProgressSummary),
            () => viewModel.EpubAnalysisProgressSummary);
        progressSummary.TextWrapping = TextWrapping.Wrap;
        TextBlock resultSummary = CreateBoundText(
            viewModel,
            nameof(MainWindowViewModel.EpubAnalysisResultSummary),
            () => viewModel.EpubAnalysisResultSummary);
        resultSummary.TextWrapping = TextWrapping.Wrap;
        resultSummary.FontFamily = new FontFamily("Consolas, Cascadia Mono, Menlo, monospace");
        resultSummary.FontSize = 12;

        viewModel.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(MainWindowViewModel.EpubAnalysisProgressPercent))
            {
                analysisProgress.Value = viewModel.EpubAnalysisProgressPercent;
            }
        };

        Button openOutput = new()
        {
            Content = "Open output folder",
            IsEnabled = viewModel.CanOpenLatestAnalysisOutput && !viewModel.IsBusy,
        };
        openOutput.Click += async (_, _) => await viewModel.OpenLatestAnalysisOutputAsync().ConfigureAwait(true);
        Button openDiagnostics = new()
        {
            Content = "Open diagnostics",
            IsEnabled = viewModel.CanOpenLatestAnalysisDiagnostics && !viewModel.IsBusy,
        };
        openDiagnostics.Click += async (_, _) => await viewModel.OpenLatestAnalysisDiagnosticsAsync().ConfigureAwait(true);
        Button openFailures = new()
        {
            Content = "Open failures CSV",
            IsEnabled = viewModel.CanOpenLatestAnalysisFailures && !viewModel.IsBusy,
        };
        openFailures.Click += async (_, _) => await viewModel.OpenLatestAnalysisFailuresAsync().ConfigureAwait(true);

        viewModel.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName is nameof(MainWindowViewModel.CanOpenLatestAnalysisOutput)
                or nameof(MainWindowViewModel.CanOpenLatestAnalysisDiagnostics)
                or nameof(MainWindowViewModel.CanOpenLatestAnalysisFailures)
                or nameof(MainWindowViewModel.IsBusy))
            {
                openOutput.IsEnabled = viewModel.CanOpenLatestAnalysisOutput && !viewModel.IsBusy;
                openDiagnostics.IsEnabled = viewModel.CanOpenLatestAnalysisDiagnostics && !viewModel.IsBusy;
                openFailures.IsEnabled = viewModel.CanOpenLatestAnalysisFailures && !viewModel.IsBusy;
            }
        };

        Grid inputRow = BuildFolderRow(inputFolder, browseInput);
        Grid outputRow = BuildFolderRow(outputFolder, browseOutput);
        StackPanel actionButtons = new()
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Children =
            {
                startButton,
                cancelButton,
            },
        };
        StackPanel artifactButtons = new()
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Children =
            {
                openOutput,
                openDiagnostics,
                openFailures,
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
                Spacing = 10,
                Children =
                {
                    new TextBlock
                    {
                        Text = "Analyze EPUB folder",
                        FontSize = 18,
                        FontWeight = FontWeight.SemiBold,
                    },
                    new TextBlock
                    {
                        Text = "Runs the same folder-analysis pipeline used by the CLI, writes the standard artifacts and saves the completed run to SQLite.",
                        TextWrapping = TextWrapping.Wrap,
                    },
                    corpusSummary,
                    LabeledInput("EPUB input folder", inputRow),
                    LabeledInput("Output folder", outputRow),
                    recursive,
                    confirmation,
                    actionButtons,
                    analysisProgress,
                    progressSummary,
                    new Separator(),
                    new TextBlock
                    {
                        Text = "Latest desktop analysis",
                        FontWeight = FontWeight.SemiBold,
                    },
                    resultSummary,
                    artifactButtons,
                },
            },
        };
    }

    private static Grid BuildFolderRow(TextBox pathInput, Button browseButton)
    {
        Grid row = new()
        {
            ColumnDefinitions = new ColumnDefinitions("*,Auto"),
        };
        Grid.SetColumn(pathInput, 0);
        pathInput.Margin = new Thickness(0, 0, 8, 0);
        row.Children.Add(pathInput);
        Grid.SetColumn(browseButton, 1);
        row.Children.Add(browseButton);
        return row;
    }

    private static async Task<string?> SelectFolderAsync(Window window, string title)
    {
        IReadOnlyList<IStorageFolder> folders = await window.StorageProvider.OpenFolderPickerAsync(
            new FolderPickerOpenOptions
            {
                Title = title,
                AllowMultiple = false,
            }).ConfigureAwait(true);

        return folders.Count == 0 ? null : folders[0].Path.LocalPath;
    }
}
