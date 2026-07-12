using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Styling;
using Avalonia.Themes.Fluent;
using AvaloniaApplication = Avalonia.Application;
using CorpusLens.Desktop.Services;
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
            MainWindowViewModel viewModel = new(
                settingsStore: DesktopRuntime.SettingsStore,
                diagnosticLog: DesktopRuntime.DiagnosticLog);
            MainWindow window = new(viewModel);
            window.Opened += async (_, _) => await viewModel.InitializeAsync().ConfigureAwait(true);
            desktop.MainWindow = window;
        }

        base.OnFrameworkInitializationCompleted();
    }
}
