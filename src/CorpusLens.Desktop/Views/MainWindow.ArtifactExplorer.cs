using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using CorpusLens.Desktop.ViewModels;

namespace CorpusLens.Desktop.Views;

public sealed partial class MainWindow
{
    private static Control BuildArtifactExplorer(
        MainWindowViewModel viewModel,
        TextBlock titleText,
        TextBlock summaryText,
        TextBlock detailsText,
        TextBlock outputDirectoryText)
    {
        ListBox artifacts = new()
        {
            ItemsSource = viewModel.RunArtifacts,
            SelectedItem = viewModel.SelectedArtifact,
            MinHeight = 190,
            MaxHeight = 300,
            IsEnabled = !viewModel.IsBusy,
        };
        artifacts.SelectionChanged += (_, _) =>
        {
            ArtifactListItemViewModel? selectedArtifact = artifacts.SelectedItem as ArtifactListItemViewModel;
            if (!ReferenceEquals(selectedArtifact, viewModel.SelectedArtifact))
            {
                viewModel.SetSelectedArtifact(selectedArtifact);
            }
        };

        Button openArtifactButton = new()
        {
            Content = "Open selected",
            IsEnabled = viewModel.CanOpenSelectedArtifact && !viewModel.IsBusy,
        };
        openArtifactButton.Click += async (_, _) =>
            await viewModel.OpenSelectedArtifactAsync().ConfigureAwait(true);

        Button openFolderButton = new()
        {
            Content = "Open output folder",
            IsEnabled = viewModel.CanOpenOutputDirectory && !viewModel.IsBusy,
        };
        openFolderButton.Click += async (_, _) =>
            await viewModel.OpenArtifactOutputDirectoryAsync().ConfigureAwait(true);

        Button refreshButton = new()
        {
            Content = "Refresh availability",
            IsEnabled = !viewModel.IsBusy,
        };
        refreshButton.Click += async (_, _) =>
            await viewModel.RefreshArtifactsAsync().ConfigureAwait(true);

        viewModel.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(MainWindowViewModel.SelectedArtifact))
            {
                if (!ReferenceEquals(artifacts.SelectedItem, viewModel.SelectedArtifact))
                {
                    artifacts.SelectedItem = viewModel.SelectedArtifact;
                }

                openArtifactButton.IsEnabled = viewModel.CanOpenSelectedArtifact && !viewModel.IsBusy;
            }

            if (args.PropertyName == nameof(MainWindowViewModel.CanOpenSelectedArtifact))
            {
                openArtifactButton.IsEnabled = viewModel.CanOpenSelectedArtifact && !viewModel.IsBusy;
            }

            if (args.PropertyName == nameof(MainWindowViewModel.CanOpenOutputDirectory))
            {
                openFolderButton.IsEnabled = viewModel.CanOpenOutputDirectory && !viewModel.IsBusy;
            }

            if (args.PropertyName == nameof(MainWindowViewModel.IsBusy))
            {
                artifacts.IsEnabled = !viewModel.IsBusy;
                refreshButton.IsEnabled = !viewModel.IsBusy;
                openArtifactButton.IsEnabled = viewModel.CanOpenSelectedArtifact && !viewModel.IsBusy;
                openFolderButton.IsEnabled = viewModel.CanOpenOutputDirectory && !viewModel.IsBusy;
            }
        };

        WrapPanel actions = new()
        {
            Orientation = Orientation.Horizontal,
            ItemSpacing = 8,
            Children =
            {
                openArtifactButton,
                openFolderButton,
                refreshButton,
            },
        };

        StackPanel artifactListPanel = new()
        {
            Spacing = 10,
            Children =
            {
                new TextBlock
                {
                    Text = "Generated files",
                    FontWeight = FontWeight.SemiBold,
                },
                artifacts,
                actions,
            },
        };

        Border artifactListCard = new()
        {
            Padding = new Thickness(16),
            BorderBrush = Brushes.LightGray,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Child = artifactListPanel,
        };
        Grid.SetColumn(artifactListCard, 0);

        Border detailsCard = new()
        {
            Padding = new Thickness(16),
            BorderBrush = Brushes.LightGray,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Margin = new Thickness(12, 0, 0, 0),
            Child = new StackPanel
            {
                Spacing = 10,
                Children =
                {
                    new TextBlock
                    {
                        Text = "Artifact details",
                        FontWeight = FontWeight.SemiBold,
                    },
                    detailsText,
                    new TextBlock
                    {
                        Text = "Resolved output folder",
                        FontWeight = FontWeight.SemiBold,
                        Margin = new Thickness(0, 8, 0, 0),
                    },
                    outputDirectoryText,
                },
            },
        };
        Grid.SetColumn(detailsCard, 1);

        Grid content = new()
        {
            ColumnDefinitions = new ColumnDefinitions("2*,3*"),
            Children =
            {
                artifactListCard,
                detailsCard,
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
                    summaryText,
                    content,
                },
            },
        };
    }
}
