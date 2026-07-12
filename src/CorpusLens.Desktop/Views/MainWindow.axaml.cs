using Avalonia.Controls;
using CorpusLens.Desktop.ViewModels;

namespace CorpusLens.Desktop.Views;

public sealed partial class MainWindow : Window
{
    public MainWindow(MainWindowViewModel viewModel)
    {
        DataContext = viewModel;
        Title = "CorpusLens";
        Width = 1280;
        Height = 820;
        MinWidth = 980;
        MinHeight = 640;
        Content = BuildContent(viewModel, this);
    }
}
