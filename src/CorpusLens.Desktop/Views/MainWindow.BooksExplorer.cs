using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using CorpusLens.Desktop.ViewModels;

namespace CorpusLens.Desktop.Views;

public sealed partial class MainWindow
{
    private static Control BuildBooksExplorer(
        MainWindowViewModel viewModel,
        TextBlock titleText,
        TextBlock summaryText,
        TextBlock detailsText)
    {
        ListBox books = new()
        {
            ItemsSource = viewModel.RunBooks,
            SelectedItem = viewModel.SelectedRunBook,
            MinHeight = 180,
            MaxHeight = 280,
            IsEnabled = !viewModel.IsBusy,
        };
        books.SelectionChanged += async (_, _) =>
        {
            RunBookListItemViewModel? selectedBook = books.SelectedItem as RunBookListItemViewModel;
            if (!ReferenceEquals(selectedBook, viewModel.SelectedRunBook))
            {
                await viewModel.SelectRunBookAsync(selectedBook).ConfigureAwait(true);
            }
        };
        viewModel.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(MainWindowViewModel.SelectedRunBook)
                && !ReferenceEquals(books.SelectedItem, viewModel.SelectedRunBook))
            {
                books.SelectedItem = viewModel.SelectedRunBook;
            }

            if (args.PropertyName == nameof(MainWindowViewModel.IsBusy))
            {
                books.IsEnabled = !viewModel.IsBusy;
            }
        };

        Border booksCard = new()
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
                    new TextBlock { Text = "Source books", FontWeight = FontWeight.SemiBold },
                    books,
                },
            },
        };
        Grid.SetColumn(booksCard, 0);

        Grid bookContent = new()
        {
            ColumnDefinitions = new ColumnDefinitions("2*,3*"),
        };
        bookContent.Children.Add(booksCard);
        bookContent.Children.Add(BuildCard("Book details", detailsText, 1, monospace: true));

        TextBlock chapterTitleText = CreateBoundText(
            viewModel,
            nameof(MainWindowViewModel.ChapterExplorerTitle),
            () => viewModel.ChapterExplorerTitle);
        chapterTitleText.FontSize = 18;
        chapterTitleText.FontWeight = FontWeight.SemiBold;

        TextBlock chapterSummaryText = CreateBoundText(
            viewModel,
            nameof(MainWindowViewModel.ChapterExplorerSummary),
            () => viewModel.ChapterExplorerSummary);
        TextBlock chapterDetailsText = CreateBoundText(
            viewModel,
            nameof(MainWindowViewModel.ChapterDetails),
            () => viewModel.ChapterDetails);
        chapterDetailsText.FontFamily = new FontFamily("Consolas, Cascadia Mono, Menlo, monospace");
        chapterDetailsText.FontSize = 12;
        chapterDetailsText.TextWrapping = TextWrapping.Wrap;

        TextBlock chapterSearchSummaryText = CreateBoundText(
            viewModel,
            nameof(MainWindowViewModel.ChapterSearchSummary),
            () => viewModel.ChapterSearchSummary);
        chapterSearchSummaryText.TextWrapping = TextWrapping.Wrap;

        ListBox chapters = new()
        {
            ItemsSource = viewModel.BookChapters,
            SelectedItem = viewModel.SelectedChapter,
            MinHeight = 280,
            MaxHeight = 480,
            IsEnabled = !viewModel.IsBusy,
        };
        chapters.SelectionChanged += (_, _) =>
        {
            ChapterListItemViewModel? selectedChapter = chapters.SelectedItem as ChapterListItemViewModel;
            if (!ReferenceEquals(selectedChapter, viewModel.SelectedChapter))
            {
                viewModel.SetSelectedChapter(selectedChapter);
            }
        };

        TextBox preview = new()
        {
            Text = viewModel.ChapterPreview,
            IsReadOnly = true,
            AcceptsReturn = true,
            TextWrapping = TextWrapping.Wrap,
            MinHeight = 320,
            MaxHeight = 520,
            FontFamily = new FontFamily("Georgia, Cambria, serif"),
            FontSize = 14,
        };

        void ApplyPreviewSelection()
        {
            int textLength = preview.Text?.Length ?? 0;
            int start = Math.Clamp(viewModel.ChapterPreviewSelectionStart, 0, textLength);
            int length = Math.Clamp(viewModel.ChapterPreviewSelectionLength, 0, textLength - start);
            preview.SelectionStart = start;
            preview.SelectionEnd = start + length;
            preview.CaretIndex = start + length;
            if (length > 0)
            {
                preview.Focus();
            }
        }

        TextBox searchBox = new()
        {
            Width = 260,
            PlaceholderText = "Search in selected chapter",
            IsEnabled = !viewModel.IsBusy,
        };
        Button searchButton = new() { Content = "Find" };
        Button previousButton = new() { Content = "Previous" };
        Button nextButton = new() { Content = "Next" };

        void SearchPreview()
        {
            viewModel.SearchChapterText(searchBox.Text);
            ApplyPreviewSelection();
        }

        searchButton.Click += (_, _) => SearchPreview();
        previousButton.Click += (_, _) =>
        {
            viewModel.MoveToPreviousChapterMatch();
            ApplyPreviewSelection();
        };
        nextButton.Click += (_, _) =>
        {
            viewModel.MoveToNextChapterMatch();
            ApplyPreviewSelection();
        };
        searchBox.KeyDown += (_, args) =>
        {
            if (args.Key == Key.Enter)
            {
                SearchPreview();
                args.Handled = true;
            }
        };

        DisableWhileBusy(searchButton, viewModel);
        DisableWhileBusy(previousButton, viewModel);
        DisableWhileBusy(nextButton, viewModel);

        viewModel.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(MainWindowViewModel.SelectedChapter))
            {
                if (!ReferenceEquals(chapters.SelectedItem, viewModel.SelectedChapter))
                {
                    chapters.SelectedItem = viewModel.SelectedChapter;
                }

                searchBox.Text = string.Empty;
            }

            if (args.PropertyName == nameof(MainWindowViewModel.ChapterPreview))
            {
                preview.Text = viewModel.ChapterPreview;
                ApplyPreviewSelection();
            }

            if (args.PropertyName == nameof(MainWindowViewModel.ChapterPreviewSelectionStart)
                || args.PropertyName == nameof(MainWindowViewModel.ChapterPreviewSelectionLength))
            {
                ApplyPreviewSelection();
            }

            if (args.PropertyName == nameof(MainWindowViewModel.IsBusy))
            {
                chapters.IsEnabled = !viewModel.IsBusy;
                searchBox.IsEnabled = !viewModel.IsBusy;
            }
        };

        WrapPanel searchControls = new()
        {
            Orientation = Orientation.Horizontal,
            ItemSpacing = 8,
            Children =
            {
                searchBox,
                searchButton,
                previousButton,
                nextButton,
            },
        };

        Border chapterListCard = new()
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
                    new TextBlock { Text = "Ordered chapters", FontWeight = FontWeight.SemiBold },
                    new TextBlock
                    {
                        Text = "Items marked with ⚠ are empty, very short, very long, or potentially suspicious.",
                        FontSize = 11,
                        TextWrapping = TextWrapping.Wrap,
                    },
                    chapters,
                },
            },
        };
        Grid.SetColumn(chapterListCard, 0);

        Border previewCard = new()
        {
            Margin = new Thickness(16, 0, 0, 0),
            Padding = new Thickness(16),
            BorderBrush = Brushes.LightGray,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Child = new StackPanel
            {
                Spacing = 10,
                Children =
                {
                    new TextBlock { Text = "Persisted clean-text preview", FontWeight = FontWeight.SemiBold },
                    searchControls,
                    chapterSearchSummaryText,
                    chapterDetailsText,
                    preview,
                },
            },
        };
        Grid.SetColumn(previewCard, 1);

        Grid chapterContent = new()
        {
            ColumnDefinitions = new ColumnDefinitions("2*,5*"),
        };
        chapterContent.Children.Add(chapterListCard);
        chapterContent.Children.Add(previewCard);

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
                    bookContent,
                    chapterTitleText,
                    chapterSummaryText,
                    chapterContent,
                },
            },
        };
    }
}
