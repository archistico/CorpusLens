using System.Text.RegularExpressions;

namespace CorpusLens.Infrastructure.Epub;

public sealed class EpubBoilerplateCleaner
{
    private static readonly Regex ProjectGutenbergStartRegex = new(
        @"(?im)^\s*\*{3}\s*START OF (?:THE )?PROJECT GUTENBERG EBOOK.*?\*{3}\s*$",
        RegexOptions.Compiled);

    private static readonly Regex ProjectGutenbergEndRegex = new(
        @"(?im)^\s*\*{3}\s*END OF (?:THE )?PROJECT GUTENBERG EBOOK.*?\*{3}\s*$",
        RegexOptions.Compiled);

    public string Clean(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        string cleaned = RemoveProjectGutenbergHeaderAndFooter(text);
        return cleaned.Trim();
    }

    private static string RemoveProjectGutenbergHeaderAndFooter(string text)
    {
        string cleaned = text;

        Match endMatch = ProjectGutenbergEndRegex.Match(cleaned);
        if (endMatch.Success)
        {
            cleaned = cleaned[..endMatch.Index];
        }

        Match startMatch = ProjectGutenbergStartRegex.Match(cleaned);
        if (startMatch.Success)
        {
            int removeUntil = startMatch.Index + startMatch.Length;
            while (removeUntil < cleaned.Length && (cleaned[removeUntil] == '\r' || cleaned[removeUntil] == '\n'))
            {
                removeUntil++;
            }

            cleaned = cleaned[removeUntil..];
        }

        return cleaned;
    }
}
