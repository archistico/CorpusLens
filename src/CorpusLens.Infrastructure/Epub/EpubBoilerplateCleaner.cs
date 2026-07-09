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

    private static readonly Regex ContentsLineRegex = new(
        @"(?im)^\s*(?:contents|table of contents)\s*$",
        RegexOptions.Compiled);

    private static readonly Regex ChapterMarkerLineRegex = new(
        @"(?im)^\s*(?:chapter|book|part)\s+(?:[ivxlcdm]+|\d+)\.?\s*$",
        RegexOptions.Compiled);

    public string Clean(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        string cleaned = RemoveProjectGutenbergHeaderAndFooter(text);
        cleaned = RemoveLeadingDuplicatedChapterList(cleaned);
        return cleaned.Trim();
    }

    public bool IsLikelyFrontMatterOnly(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return true;
        }

        string[] lines = GetNonEmptyLines(text);
        if (lines.Length == 0)
        {
            return true;
        }

        bool hasContentsNearTop = lines
            .Take(3)
            .Any(line => ContentsLineRegex.IsMatch(line));

        if (!hasContentsNearTop)
        {
            return false;
        }

        int chapterMarkerCount = lines.Count(line => ChapterMarkerLineRegex.IsMatch(line));
        int longProseLineCount = lines.Count(line => CountWords(line) >= 12);

        return chapterMarkerCount >= 3 && longProseLineCount == 0;
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

    private static string RemoveLeadingDuplicatedChapterList(string text)
    {
        Match contentsMatch = ContentsLineRegex.Match(text);
        if (!contentsMatch.Success || contentsMatch.Index > 3_000)
        {
            return text;
        }

        MatchCollection chapterMatches = ChapterMarkerLineRegex.Matches(text);
        if (chapterMatches.Count < 2)
        {
            return text;
        }

        Match firstChapterMatch = chapterMatches[0];
        string firstChapterMarker = NormalizeChapterMarker(firstChapterMatch.Value);

        foreach (Match chapterMatch in chapterMatches.Cast<Match>().Skip(1))
        {
            if (!string.Equals(NormalizeChapterMarker(chapterMatch.Value), firstChapterMarker, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (chapterMatch.Index > 15_000)
            {
                return text;
            }

            return text[chapterMatch.Index..];
        }

        return text;
    }

    private static string NormalizeChapterMarker(string value)
    {
        return Regex.Replace(value.Trim().TrimEnd('.'), @"\s+", " ").ToLowerInvariant();
    }

    private static string[] GetNonEmptyLines(string text)
    {
        return text
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n')
            .Split('\n')
            .Select(line => line.Trim())
            .Where(line => line.Length > 0)
            .ToArray();
    }

    private static int CountWords(string line)
    {
        return Regex.Matches(line, @"[\p{L}\p{M}]+", RegexOptions.CultureInvariant).Count;
    }
}
