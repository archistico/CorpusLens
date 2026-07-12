namespace CorpusLens.Desktop.ViewModels;

internal static class DesktopTextFormatter
{
    public static int ParseIntOrDefault(string? value, int fallback)
    {
        return int.TryParse(value, out int parsed) ? parsed : fallback;
    }

    public static double ParseDoubleOrDefault(string? value, double fallback)
    {
        return double.TryParse(value, out double parsed) ? parsed : fallback;
    }

    public static string FormatDouble(double value)
    {
        return value.ToString("0.##");
    }

    public static string FormatPercent(double value)
    {
        return value.ToString("P2");
    }

    public static string FormatProbability(double value)
    {
        return value.ToString("P2");
    }

    public static string FormatSignedDouble(double value)
    {
        if (Math.Abs(value) < 0.005)
        {
            return "0";
        }

        return value > 0 ? $"+{FormatDouble(value)}" : FormatDouble(value);
    }

    public static string FormatRatio(double value)
    {
        if (double.IsPositiveInfinity(value))
        {
            return "inf";
        }

        if (double.IsNaN(value))
        {
            return "n/a";
        }

        if (value > 0 && value < 0.01)
        {
            return "<0.01";
        }

        return FormatDouble(value);
    }

    public static string TrimForColumn(string value, int maxLength)
    {
        if (value.Length <= maxLength)
        {
            return value;
        }

        return maxLength <= 1
            ? value[..maxLength]
            : value[..(maxLength - 1)] + "…";
    }

    public static string FormatLanguageCodes(IReadOnlyList<string> languageCodes)
    {
        return languageCodes.Count == 0
            ? "unknown"
            : string.Join(", ", languageCodes);
    }
}
