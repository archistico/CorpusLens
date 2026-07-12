namespace CorpusLens.Desktop.Services;

public static class DesktopRuntimePaths
{
    public static string ApplicationDataDirectory => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "CorpusLens");

    public static string SettingsPath => Path.Combine(ApplicationDataDirectory, "settings.json");

    public static string LogDirectory => Path.Combine(ApplicationDataDirectory, "logs");
}
