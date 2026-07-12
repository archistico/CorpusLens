using Avalonia.Controls;
using CorpusLens.Desktop.ViewModels;

namespace CorpusLens.Desktop.Views;

public sealed partial class MainWindow : Window
{
    public MainWindow(MainWindowViewModel viewModel)
    {
        DataContext = viewModel;
        Title = $"CorpusLens {viewModel.ApplicationVersion}";
        Width = viewModel.InitialWindowWidth;
        Height = viewModel.InitialWindowHeight;
        MinWidth = 980;
        MinHeight = 640;
        Content = BuildContent(viewModel, this);
        Closing += (_, _) => viewModel.RememberWindowSize(Bounds.Width, Bounds.Height);
    }
}
