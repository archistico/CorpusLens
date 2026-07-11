using System.Globalization;
using System.Text;

namespace CorpusLens.Infrastructure.Reports;

internal static class CsvWriter
{
    public static async Task WriteAsync(
        string filePath,
        IReadOnlyList<string> headers,
        IEnumerable<IReadOnlyList<string>> rows,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        ArgumentNullException.ThrowIfNull(headers);
        ArgumentNullException.ThrowIfNull(rows);

        StringBuilder builder = new();
        builder.AppendLine(string.Join(',', headers.Select(Escape)));

        foreach (IReadOnlyList<string> row in rows)
        {
            builder.AppendLine(string.Join(',', row.Select(Escape)));
        }

        string? directory = Path.GetDirectoryName(Path.GetFullPath(filePath));
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await File.WriteAllTextAsync(filePath, builder.ToString(), Encoding.UTF8, cancellationToken).ConfigureAwait(false);
    }

    public static string FormatDouble(double value)
    {
        return value.ToString("0.####", CultureInfo.InvariantCulture);
    }

    private static string Escape(string value)
    {
        if (value.Contains('"') ||
            value.Contains(',') ||
            value.Contains('\n') ||
            value.Contains('\r'))
        {
            return $"\"{value.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
        }

        return value;
    }
}
