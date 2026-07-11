using Avalonia.Styling;
using Avalonia.Themes.Fluent;
using AvaloniaApplication = Avalonia.Application;
using Avalonia.Controls.ApplicationLifetimes;
using CorpusLens.Desktop.ViewModels;
using CorpusLens.Desktop.Views;

namespace CorpusLens.Desktop;

public sealed partial class App : AvaloniaApplication
{
    public override void Initialize()
    {
        Styles.Add(new FluentTheme());
        RequestedThemeVariant = ThemeVariant.Light;
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow(new MainWindowViewModel());
        }

        base.OnFrameworkInitializationCompleted();
    }
}
