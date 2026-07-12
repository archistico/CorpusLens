using CorpusLens.Desktop.Services;
using CorpusLens.Desktop.ViewModels;
using CorpusLens.Infrastructure.Storage;
using Xunit;

namespace CorpusLens.Desktop.Tests;

public sealed class DesktopStabilizationTests
{
    [Fact]
    public void JsonSettingsStore_RoundTripsNonSensitiveDesktopPreferences()
    {
        string root = CreateTemporaryDirectory();
        try
        {
            string settingsPath = Path.Combine(root, "settings.json");
            string firstDatabase = Path.Combine(root, "first.db");
            string secondDatabase = Path.Combine(root, "second.db");
            JsonDesktopSettingsStore store = new(settingsPath);
            DesktopSettings expected = new()
            {
                LastDatabasePath = firstDatabase,
                RecentDatabasePaths = new List<string> { firstDatabase, secondDatabase },
                LastEpubInputFolder = Path.Combine(root, "books"),
                LastEpubOutputFolder = Path.Combine(root, "artifacts"),
                RecursiveEpubSearch = true,
                WindowWidth = 1440,
                WindowHeight = 900,
            };

            store.Save(expected);
            DesktopSettings actual = store.Load();

            Assert.Equal(Path.GetFullPath(firstDatabase), actual.LastDatabasePath);
            Assert.Equal(2, actual.RecentDatabasePaths.Count);
            Assert.Equal(Path.GetFullPath(firstDatabase), actual.RecentDatabasePaths[0]);
            Assert.Equal(Path.GetFullPath(secondDatabase), actual.RecentDatabasePaths[1]);
            Assert.Equal(Path.GetFullPath(expected.LastEpubInputFolder), actual.LastEpubInputFolder);
            Assert.Equal(Path.GetFullPath(expected.LastEpubOutputFolder), actual.LastEpubOutputFolder);
            Assert.True(actual.RecursiveEpubSearch);
            Assert.Equal(1440d, actual.WindowWidth);
            Assert.Equal(900d, actual.WindowHeight);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void JsonSettingsStore_UsesDefaultsWhenJsonIsInvalid()
    {
        string root = CreateTemporaryDirectory();
        try
        {
            string settingsPath = Path.Combine(root, "settings.json");
            File.WriteAllText(settingsPath, "{ definitely not valid json");
            JsonDesktopSettingsStore store = new(settingsPath);

            DesktopSettings settings = store.Load();

            Assert.Empty(settings.RecentDatabasePaths);
            Assert.Equal(string.Empty, settings.LastDatabasePath);
            Assert.Equal(1280d, settings.WindowWidth);
            Assert.Equal(820d, settings.WindowHeight);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void DiagnosticLog_WritesMessageAndExceptionDetails()
    {
        string root = CreateTemporaryDirectory();
        try
        {
            DesktopDiagnosticLog log = new(root);
            InvalidOperationException exception = new("test failure");

            log.WriteInformation("startup smoke test");
            log.WriteError("operation failed", exception);

            Assert.True(File.Exists(log.CurrentLogPath));
            string contents = File.ReadAllText(log.CurrentLogPath);
            Assert.Contains("startup smoke test", contents);
            Assert.Contains("operation failed", contents);
            Assert.Contains("test failure", contents);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task MainWindow_StartupRestoresLastDatabaseAndCorpora()
    {
        string root = CreateTemporaryDirectory();
        try
        {
            string databasePath = Path.Combine(root, "corpuslens.db");
            SqliteCorpusStore corpusStore = new(databasePath);
            await corpusStore.CreateCorpusAsync("English corpus", "en");
            InMemoryDesktopSettingsStore settingsStore = new(new DesktopSettings
            {
                LastDatabasePath = databasePath,
                RecentDatabasePaths = new List<string> { databasePath },
            });
            MainWindowViewModel viewModel = new(settingsStore: settingsStore);

            await viewModel.InitializeAsync();

            Assert.Equal(Path.GetFullPath(databasePath), viewModel.DatabasePath);
            Assert.Equal(Path.GetFullPath(databasePath), viewModel.RecentDatabasePaths[0]);
            Assert.Equal(2, viewModel.CorpusItems.Count);
            Assert.Contains("Loaded 1 corpus/corpora", viewModel.StatusMessage);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task MainWindow_OpeningDatabaseMovesItToFrontOfRecentList()
    {
        string root = CreateTemporaryDirectory();
        try
        {
            string firstDatabase = Path.Combine(root, "first.db");
            string secondDatabase = Path.Combine(root, "second.db");
            await new SqliteCorpusStore(firstDatabase).CreateCorpusAsync("First", "en");
            await new SqliteCorpusStore(secondDatabase).CreateCorpusAsync("Second", "it");
            InMemoryDesktopSettingsStore settingsStore = new();
            MainWindowViewModel viewModel = new(settingsStore: settingsStore);

            await viewModel.OpenDatabaseAsync(firstDatabase);
            await viewModel.OpenDatabaseAsync(secondDatabase);
            await viewModel.OpenDatabaseAsync(firstDatabase);

            Assert.Equal(2, viewModel.RecentDatabasePaths.Count);
            Assert.Equal(Path.GetFullPath(firstDatabase), viewModel.RecentDatabasePaths[0]);
            Assert.Equal(Path.GetFullPath(secondDatabase), viewModel.RecentDatabasePaths[1]);
            Assert.Equal(Path.GetFullPath(firstDatabase), settingsStore.Load().LastDatabasePath);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task MainWindow_RemovesUnavailableLastDatabaseDuringStartup()
    {
        string root = CreateTemporaryDirectory();
        try
        {
            string missingDatabase = Path.Combine(root, "missing.db");
            InMemoryDesktopSettingsStore settingsStore = new(new DesktopSettings
            {
                LastDatabasePath = missingDatabase,
                RecentDatabasePaths = new List<string> { missingDatabase },
            });
            MainWindowViewModel viewModel = new(settingsStore: settingsStore);

            await viewModel.InitializeAsync();

            Assert.Empty(viewModel.RecentDatabasePaths);
            Assert.Equal(string.Empty, settingsStore.Load().LastDatabasePath);
            Assert.Contains("no longer available", viewModel.StatusMessage);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void MainWindow_PersistsAnalysisFoldersAndWindowSize()
    {
        string root = CreateTemporaryDirectory();
        try
        {
            string input = Path.Combine(root, "books");
            string output = Path.Combine(root, "artifacts");
            InMemoryDesktopSettingsStore settingsStore = new();
            MainWindowViewModel viewModel = new(settingsStore: settingsStore);

            viewModel.RememberEpubPreferences(input, output, recursive: true);
            viewModel.RememberWindowSize(1600, 960);

            DesktopSettings settings = settingsStore.Load();
            Assert.Equal(Path.GetFullPath(input), settings.LastEpubInputFolder);
            Assert.Equal(Path.GetFullPath(output), settings.LastEpubOutputFolder);
            Assert.True(settings.RecursiveEpubSearch);
            Assert.Equal(1600d, settings.WindowWidth);
            Assert.Equal(960d, settings.WindowHeight);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private static string CreateTemporaryDirectory()
    {
        string path = Path.Combine(
            Path.GetTempPath(),
            $"corpuslens-desktop-stabilization-{Guid.NewGuid():N}");
        Directory.CreateDirectory(path);
        return path;
    }
}
