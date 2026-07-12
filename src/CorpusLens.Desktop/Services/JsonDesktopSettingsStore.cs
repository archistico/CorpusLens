using System.Text.Json;

namespace CorpusLens.Desktop.Services;

public sealed class JsonDesktopSettingsStore : IDesktopSettingsStore
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
    };

    private readonly IDesktopDiagnosticLog? _diagnosticLog;

    public JsonDesktopSettingsStore(
        string? settingsPath = null,
        IDesktopDiagnosticLog? diagnosticLog = null)
    {
        SettingsPath = string.IsNullOrWhiteSpace(settingsPath)
            ? DesktopRuntimePaths.SettingsPath
            : Path.GetFullPath(settingsPath);
        _diagnosticLog = diagnosticLog;
    }

    public string SettingsPath { get; }

    public DesktopSettings Load()
    {
        try
        {
            if (!File.Exists(SettingsPath))
            {
                return new DesktopSettings();
            }

            string json = File.ReadAllText(SettingsPath);
            DesktopSettings? settings = JsonSerializer.Deserialize<DesktopSettings>(json, SerializerOptions);
            return (settings ?? new DesktopSettings()).Normalize();
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or JsonException or ArgumentException or NotSupportedException)
        {
            _diagnosticLog?.WriteError("Could not read desktop settings. Defaults will be used.", exception);
            return new DesktopSettings();
        }
    }

    public void Save(DesktopSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        try
        {
            DesktopSettings normalized = settings.Normalize();
            string? directory = Path.GetDirectoryName(SettingsPath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string temporaryPath = SettingsPath + ".tmp";
            string json = JsonSerializer.Serialize(normalized, SerializerOptions);
            File.WriteAllText(temporaryPath, json);
            File.Move(temporaryPath, SettingsPath, overwrite: true);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or JsonException or ArgumentException or NotSupportedException)
        {
            _diagnosticLog?.WriteError("Could not save desktop settings.", exception);
        }
    }
}
