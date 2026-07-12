using System.Diagnostics;

namespace CorpusLens.Desktop.Services;

public static class SystemPathLauncher
{
    public static Task OpenAsync(
        string path,
        bool isDirectory,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        string fullPath = Path.GetFullPath(path);
        bool exists = isDirectory ? Directory.Exists(fullPath) : File.Exists(fullPath);
        if (!exists)
        {
            if (isDirectory)
            {
                throw new DirectoryNotFoundException($"Directory not found: {fullPath}");
            }

            throw new FileNotFoundException($"File not found: {fullPath}", fullPath);
        }

        ProcessStartInfo startInfo = new()
        {
            FileName = fullPath,
            UseShellExecute = true,
        };
        using Process process = Process.Start(startInfo)
            ?? throw new InvalidOperationException($"The operating system could not open: {fullPath}");

        return Task.CompletedTask;
    }
}
