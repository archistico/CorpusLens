using System.Text.RegularExpressions;

namespace CorpusLens.Analysis.Normalization;

public sealed class TextNormalizer
{
    private static readonly Regex HorizontalWhitespaceRegex = new(@"[\t\f\v\u00A0]+", RegexOptions.Compiled);
    private static readonly Regex MultiSpaceRegex = new(@" {2,}", RegexOptions.Compiled);
    private static readonly Regex MultiBlankLineRegex = new(@"\n{3,}", RegexOptions.Compiled);

    public string NormalizeForReading(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        string normalized = text
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace("\r", "\n", StringComparison.Ordinal)
            .Replace('’', '\'')
            .Replace('‘', '\'')
            .Replace('“', '"')
            .Replace('”', '"')
            .Replace('—', '-')
            .Replace('–', '-');

        normalized = HorizontalWhitespaceRegex.Replace(normalized, " ");
        normalized = MultiSpaceRegex.Replace(normalized, " ");
        normalized = MultiBlankLineRegex.Replace(normalized, "\n\n");

        return normalized.Trim();
    }

    public string NormalizeInline(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        string normalized = NormalizeForReading(text).Replace('\n', ' ');
        normalized = MultiSpaceRegex.Replace(normalized, " ");

        return normalized.Trim();
    }

    public string NormalizeKey(string text)
    {
        return NormalizeInline(text).ToLowerInvariant();
    }
}
