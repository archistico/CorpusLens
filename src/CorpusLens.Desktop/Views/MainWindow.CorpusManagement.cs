using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using CorpusLens.Application.Storage;
using CorpusLens.Desktop.ViewModels;

namespace CorpusLens.Desktop.Views;

public sealed partial class MainWindow
{
    private static Control BuildCorpusManagement(
        MainWindowViewModel viewModel,
        TextBlock summaryText,
        TextBlock detailsText)
    {
        TextBox nameInput = new()
        {
            PlaceholderText = "e.g. Italian literature",
            MinWidth = 240,
        };
        ComboBox languageInput = new()
        {
            ItemsSource = viewModel.SupportedCorpusLanguages,
            SelectedItem = viewModel.DefaultCorpusLanguage,
            MinWidth = 180,
        };
        TextBox descriptionInput = new()
        {
            PlaceholderText = "Optional description",
            AcceptsReturn = true,
            TextWrapping = TextWrapping.Wrap,
            MinHeight = 70,
        };
        CheckBox confirmation = new()
        {
            Content = "I confirm creation of this corpus in the open database.",
        };
        Button createButton = new()
        {
            Content = "Create corpus",
            HorizontalAlignment = HorizontalAlignment.Left,
            MinWidth = 130,
        };
        DisableWhileBusy(createButton, viewModel);
        createButton.Click += async (_, _) =>
        {
            int previousCount = viewModel.CorpusItems.Count;
            string? languageCode = (languageInput.SelectedItem as CorpusLanguageOption)?.Code;
            await viewModel.CreateCorpusAsync(
                nameInput.Text,
                languageCode,
                descriptionInput.Text,
                confirmation.IsChecked == true).ConfigureAwait(true);

            if (viewModel.CorpusItems.Count > previousCount)
            {
                nameInput.Text = string.Empty;
                descriptionInput.Text = string.Empty;
                confirmation.IsChecked = false;
            }
        };

        Grid formRow = new()
        {
            ColumnDefinitions = new ColumnDefinitions("2*,*,Auto"),
        };
        Control nameField = LabeledInput("Corpus name", nameInput);
        nameField.Margin = new Thickness(0, 0, 12, 0);
        Grid.SetColumn(nameField, 0);
        formRow.Children.Add(nameField);
        Control languageField = LabeledInput("Language", languageInput);
        languageField.Margin = new Thickness(0, 0, 12, 0);
        Grid.SetColumn(languageField, 1);
        formRow.Children.Add(languageField);
        Grid.SetColumn(createButton, 2);
        createButton.VerticalAlignment = VerticalAlignment.Bottom;
        formRow.Children.Add(createButton);

        detailsText.FontFamily = new FontFamily("Consolas, Cascadia Mono, Menlo, monospace");
        detailsText.FontSize = 12;
        detailsText.TextWrapping = TextWrapping.Wrap;
        summaryText.TextWrapping = TextWrapping.Wrap;

        Border panel = new()
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
                        Text = "Corpus management",
                        FontSize = 18,
                        FontWeight = FontWeight.SemiBold,
                    },
                    summaryText,
                    detailsText,
                    new Separator(),
                    new TextBlock
                    {
                        Text = "Create corpus",
                        FontWeight = FontWeight.SemiBold,
                    },
                    new TextBlock
                    {
                        Text = "The language is persisted with the corpus and will constrain EPUB analysis in the next milestone.",
                        TextWrapping = TextWrapping.Wrap,
                        FontSize = 12,
                    },
                    formRow,
                    LabeledInput("Description", descriptionInput),
                    confirmation,
                },
            },
        };

        return panel;
    }
}
