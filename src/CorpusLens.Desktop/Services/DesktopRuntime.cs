namespace CorpusLens.Desktop.Services;

public static class DesktopRuntime
{
    public static IDesktopDiagnosticLog DiagnosticLog { get; } = new DesktopDiagnosticLog();

    public static IDesktopSettingsStore SettingsStore { get; } = new JsonDesktopSettingsStore(
        diagnosticLog: DiagnosticLog);
}
