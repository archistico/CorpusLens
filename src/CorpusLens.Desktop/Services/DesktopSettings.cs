namespace CorpusLens.Desktop.Services;

public sealed record DesktopSettings
{
    public const int MaxRecentDatabases = 8;

    public string LastDatabasePath { get; init; } = string.Empty;

    public List<string> RecentDatabasePaths { get; init; } = new();

    public string LastEpubInputFolder { get; init; } = string.Empty;

    public string LastEpubOutputFolder { get; init; } = string.Empty;

    public bool RecursiveEpubSearch { get; init; }

    public double WindowWidth { get; init; } = 1280;

    public double WindowHeight { get; init; } = 820;

    public DesktopSettings Normalize()
    {
        IEnumerable<string> recentPaths = RecentDatabasePaths ?? Enumerable.Empty<string>();
        List<string> recent = recentPaths
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Select(NormalizePathSafely)
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(MaxRecentDatabases)
            .ToList();

        string lastDatabase = NormalizePathSafely(LastDatabasePath);
        if (!string.IsNullOrWhiteSpace(lastDatabase))
        {
            recent.RemoveAll(path => string.Equals(path, lastDatabase, StringComparison.OrdinalIgnoreCase));
            recent.Insert(0, lastDatabase);
        }

        return this with
        {
            LastDatabasePath = lastDatabase,
            RecentDatabasePaths = recent.Take(MaxRecentDatabases).ToList(),
            LastEpubInputFolder = NormalizePathSafely(LastEpubInputFolder),
            LastEpubOutputFolder = NormalizePathSafely(LastEpubOutputFolder),
            WindowWidth = ClampFinite(WindowWidth, 980, 3840, 1280),
            WindowHeight = ClampFinite(WindowHeight, 640, 2160, 820),
        };
    }

    private static string NormalizePathSafely(string? path)
    {
        string value = path?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        try
        {
            return Path.GetFullPath(value);
        }
        catch (Exception exception) when (exception is ArgumentException or NotSupportedException or PathTooLongException)
        {
            return value;
        }
    }

    private static double ClampFinite(double value, double minimum, double maximum, double fallback)
    {
        return double.IsFinite(value) ? Math.Clamp(value, minimum, maximum) : fallback;
    }
}

public interface IDesktopSettingsStore
{
    string SettingsPath { get; }

    DesktopSettings Load();

    void Save(DesktopSettings settings);
}

public sealed class InMemoryDesktopSettingsStore : IDesktopSettingsStore
{
    private DesktopSettings _settings;

    public InMemoryDesktopSettingsStore(DesktopSettings? settings = null)
    {
        _settings = (settings ?? new DesktopSettings()).Normalize();
    }

    public string SettingsPath => string.Empty;

    public DesktopSettings Load()
    {
        return _settings;
    }

    public void Save(DesktopSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        _settings = settings.Normalize();
    }
}
