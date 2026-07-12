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
    private static TextBlock CreateBoundText(MainWindowViewModel viewModel, string propertyName, Func<string> valueProvider)
    {
        TextBlock textBlock = new()
        {
            Text = valueProvider(),
        };

        viewModel.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == propertyName)
            {
                textBlock.Text = valueProvider();
            }
        };

        return textBlock;
    }
    private static TextBox CreateBoundReadOnlyTextBox(
        MainWindowViewModel viewModel,
        string propertyName,
        Func<string> valueProvider)
    {
        TextBox textBox = new()
        {
            Text = valueProvider(),
            IsReadOnly = true,
            AcceptsReturn = true,
            TextWrapping = TextWrapping.NoWrap,
            FontFamily = new FontFamily("Consolas, Cascadia Mono, Menlo, monospace"),
            FontSize = 12,
            MinHeight = 220,
            MaxHeight = 420,
        };

        viewModel.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == propertyName)
            {
                textBox.Text = valueProvider();
            }
        };

        return textBox;
    }

    private static ProgressBar CreateBusyProgress(MainWindowViewModel viewModel)
    {
        ProgressBar progressBar = new()
        {
            IsIndeterminate = true,
            IsVisible = viewModel.IsBusy,
            MinWidth = 180,
            Height = 8,
            VerticalAlignment = VerticalAlignment.Center,
        };

        viewModel.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(MainWindowViewModel.IsBusy))
            {
                progressBar.IsVisible = viewModel.IsBusy;
            }
        };

        return progressBar;
    }
    private static void DisableWhileBusy(Button button, MainWindowViewModel viewModel)
    {
        button.IsEnabled = !viewModel.IsBusy;
        viewModel.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(MainWindowViewModel.IsBusy))
            {
                button.IsEnabled = !viewModel.IsBusy;
            }
        };
    }
    private static Control LabeledInput(string label, Control input)
    {
        return new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 2,
            Children =
            {
                new TextBlock { Text = label, FontSize = 11 },
                input,
            },
        };
    }
    private static Control BuildCard(string title, TextBlock body, int column, bool monospace = false)
    {
        body.TextWrapping = TextWrapping.Wrap;
        if (monospace)
        {
            body.FontFamily = new FontFamily("Consolas, Cascadia Mono, Menlo, monospace");
            body.FontSize = 12;
        }

        Border card = new()
        {
            Margin = column == 0 ? new Thickness(0) : new Thickness(16, 0, 0, 0),
            Padding = new Thickness(16),
            BorderBrush = Brushes.LightGray,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Child = new StackPanel
            {
                Spacing = 8,
                Children =
                {
                    new TextBlock { Text = title, FontWeight = FontWeight.SemiBold },
                    body,
                },
            },
        };
        Grid.SetColumn(card, column);
        return card;
    }
}
