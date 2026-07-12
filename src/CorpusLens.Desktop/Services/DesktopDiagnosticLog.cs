using System.Text;

namespace CorpusLens.Desktop.Services;

public interface IDesktopDiagnosticLog
{
    string LogDirectory { get; }

    string CurrentLogPath { get; }

    void WriteInformation(string message);

    void WriteError(string message, Exception exception);
}

public sealed class DesktopDiagnosticLog : IDesktopDiagnosticLog
{
    private readonly object _syncRoot = new();

    public DesktopDiagnosticLog(string? logDirectory = null)
    {
        LogDirectory = string.IsNullOrWhiteSpace(logDirectory)
            ? DesktopRuntimePaths.LogDirectory
            : Path.GetFullPath(logDirectory);
    }

    public string LogDirectory { get; }

    public string CurrentLogPath => Path.Combine(
        LogDirectory,
        $"corpuslens-{DateTime.Now:yyyyMMdd}.log");

    public void WriteInformation(string message)
    {
        Append("INFO", message, null);
    }

    public void WriteError(string message, Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        Append("ERROR", message, exception);
    }

    private void Append(string level, string message, Exception? exception)
    {
        try
        {
            StringBuilder entry = new();
            entry.Append(DateTimeOffset.Now.ToString("O"));
            entry.Append(" [");
            entry.Append(level);
            entry.Append("] ");
            entry.AppendLine(message);
            if (exception is not null)
            {
                entry.AppendLine(exception.ToString());
            }

            lock (_syncRoot)
            {
                Directory.CreateDirectory(LogDirectory);
                File.AppendAllText(CurrentLogPath, entry.ToString(), Encoding.UTF8);
            }
        }
        catch
        {
            // Diagnostic logging must never crash the application or mask the original error.
        }
    }
}

public sealed class NullDesktopDiagnosticLog : IDesktopDiagnosticLog
{
    public string LogDirectory => string.Empty;

    public string CurrentLogPath => string.Empty;

    public void WriteInformation(string message)
    {
    }

    public void WriteError(string message, Exception exception)
    {
    }
}
