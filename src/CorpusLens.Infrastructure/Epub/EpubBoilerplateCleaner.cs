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
        @"(?im)^\s*(?:contents|table of contents|indice|sommario)\s*$",
        RegexOptions.Compiled);

    private static readonly Regex ChapterMarkerLineRegex = new(
        @"(?im)^\s*(?:(?:chapter|book|part|capitolo|parte)\s+(?:[ivxlcdm]+|\d+)\.?|(?:[ivxlcdm]+|\d+)\s*[–—-]\s+.+)\s*$",
        RegexOptions.Compiled);

    private static readonly Regex StandaloneRomanNumeralRegex = new(
        @"(?i)^\s*[ivxlcdm]+\.?\s*$",
        RegexOptions.Compiled);

    private static readonly Regex ItalianChapterMarkerRegex = new(
        @"(?i)^\s*(?:capitolo|parte)\s+(?:[ivxlcdm]+|\d+)\.?\s*$",
        RegexOptions.Compiled);

    public string Clean(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        string cleaned = RemoveProjectGutenbergHeaderAndFooter(text);
        cleaned = RemoveLeadingDuplicatedChapterList(cleaned);
        cleaned = RemoveLiberLiberLeadingMatter(cleaned);
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
            .Take(8)
            .Any(line => ContentsLineRegex.IsMatch(line));

        bool hasLiberLiberMetadata = HasLiberLiberMetadata(lines.Take(80));
        if (!hasContentsNearTop && !hasLiberLiberMetadata)
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

    private static string RemoveLiberLiberLeadingMatter(string text)
    {
        LineInfo[] lines = GetLineInfos(text)
            .Where(line => line.Text.Length > 0)
            .ToArray();

        if (lines.Length == 0)
        {
            return text;
        }

        if (!HasLiberLiberMetadata(lines.Take(120).Select(line => line.Text)))
        {
            return text;
        }

        int lastMetadataIndex = FindLastLeadingLiberLiberMetadataLine(lines);
        if (lastMetadataIndex < 0)
        {
            return text;
        }

        int candidateIndex = FindFirstLikelyNarrativeStart(lines, lastMetadataIndex + 1);
        if (candidateIndex < 0)
        {
            return text;
        }

        int startOffset = lines[candidateIndex].Start;
        if (startOffset <= 0 || startOffset > 35_000)
        {
            return text;
        }

        return text[startOffset..];
    }

    private static bool HasLiberLiberMetadata(IEnumerable<string> lines)
    {
        string joined = string.Join('\n', lines).ToLowerInvariant();
        return joined.Contains("liber liber", StringComparison.Ordinal)
            || joined.Contains("questo e-book", StringComparison.Ordinal)
            || joined.Contains("licenza", StringComparison.Ordinal) && joined.Contains("liberliber", StringComparison.Ordinal)
            || joined.Contains("indice di affidabilità", StringComparison.Ordinal);
    }

    private static int FindLastLeadingLiberLiberMetadataLine(IReadOnlyList<LineInfo> lines)
    {
        int last = -1;
        int upperBound = Math.Min(lines.Count, 260);

        for (int i = 0; i < upperBound; i++)
        {
            string normalized = lines[i].Text.Trim().ToLowerInvariant();
            if (normalized.Length == 0)
            {
                continue;
            }

            if (normalized is "informazioni" or "copertina" or "colophon" or "liber liber" or "indice"
                || normalized.StartsWith("questo e-book", StringComparison.Ordinal)
                || normalized.StartsWith("titolo:", StringComparison.Ordinal)
                || normalized.StartsWith("autore:", StringComparison.Ordinal)
                || normalized.StartsWith("traduttore:", StringComparison.Ordinal)
                || normalized.StartsWith("curatore:", StringComparison.Ordinal)
                || normalized.StartsWith("note:", StringComparison.Ordinal)
                || normalized.StartsWith("codice isbn", StringComparison.Ordinal)
                || normalized.StartsWith("diritti d'autore", StringComparison.Ordinal)
                || normalized.StartsWith("licenza:", StringComparison.Ordinal)
                || normalized.StartsWith("copertina:", StringComparison.Ordinal)
                || normalized.StartsWith("tratto da:", StringComparison.Ordinal)
                || normalized.StartsWith("soggetto:", StringComparison.Ordinal)
                || normalized.StartsWith("digitalizzazione:", StringComparison.Ordinal)
                || normalized.StartsWith("revisione:", StringComparison.Ordinal)
                || normalized.StartsWith("impaginazione:", StringComparison.Ordinal)
                || normalized.StartsWith("pubblicazione:", StringComparison.Ordinal)
                || normalized.Contains("liberliber", StringComparison.Ordinal)
                || normalized.Contains("fai una donazione", StringComparison.Ordinal)
                || normalized.Contains("migliaia di ebook", StringComparison.Ordinal)
                || IsLikelyTocLine(normalized))
            {
                last = i;
            }
        }

        return last;
    }

    private static int FindFirstLikelyNarrativeStart(IReadOnlyList<LineInfo> lines, int startIndex)
    {
        int upperBound = Math.Min(lines.Count, startIndex + 80);
        for (int i = startIndex; i < upperBound; i++)
        {
            string text = lines[i].Text.Trim();
            if (text.Length == 0)
            {
                continue;
            }

            if (ItalianChapterMarkerRegex.IsMatch(text))
            {
                return i;
            }

            if (StandaloneRomanNumeralRegex.IsMatch(text) && HasFollowingNarrativeOrChapterTitle(lines, i))
            {
                return i;
            }
        }

        for (int i = startIndex; i < upperBound; i++)
        {
            if (CountWords(lines[i].Text) >= 12 && !IsLikelyMetadataLine(lines[i].Text))
            {
                return i;
            }
        }

        return -1;
    }

    private static bool HasFollowingNarrativeOrChapterTitle(IReadOnlyList<LineInfo> lines, int index)
    {
        int upperBound = Math.Min(lines.Count, index + 5);
        for (int i = index + 1; i < upperBound; i++)
        {
            string line = lines[i].Text.Trim();
            if (line.Length == 0 || IsLikelyMetadataLine(line) || IsLikelyTocLine(line.ToLowerInvariant()))
            {
                continue;
            }

            int wordCount = CountWords(line);
            if (wordCount is >= 1 and <= 12)
            {
                return true;
            }

            if (wordCount >= 12)
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsLikelyMetadataLine(string line)
    {
        string normalized = line.Trim().ToLowerInvariant();
        return normalized.StartsWith("titolo:", StringComparison.Ordinal)
            || normalized.StartsWith("autore:", StringComparison.Ordinal)
            || normalized.StartsWith("traduttore:", StringComparison.Ordinal)
            || normalized.StartsWith("curatore:", StringComparison.Ordinal)
            || normalized.StartsWith("licenza:", StringComparison.Ordinal)
            || normalized.StartsWith("codice isbn", StringComparison.Ordinal)
            || normalized.Contains("liberliber", StringComparison.Ordinal)
            || normalized.Contains("http://", StringComparison.Ordinal)
            || normalized.Contains("https://", StringComparison.Ordinal)
            || normalized.Contains("@", StringComparison.Ordinal);
    }

    private static bool IsLikelyTocLine(string normalizedLine)
    {
        return Regex.IsMatch(normalizedLine, @"^(?:[ivxlcdm]+|\d+)\s*[–—-]\s+.+$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)
            || normalizedLine.StartsWith("indice ", StringComparison.Ordinal)
            || normalizedLine.Contains("(questa pagina)", StringComparison.Ordinal);
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

    private static LineInfo[] GetLineInfos(string text)
    {
        List<LineInfo> lines = new();
        int lineStart = 0;

        for (int i = 0; i <= text.Length; i++)
        {
            if (i < text.Length && text[i] != '\n' && text[i] != '\r')
            {
                continue;
            }

            string line = text[lineStart..i].Trim();
            lines.Add(new LineInfo(lineStart, line));

            if (i < text.Length && text[i] == '\r' && i + 1 < text.Length && text[i + 1] == '\n')
            {
                i++;
            }

            lineStart = i + 1;
        }

        return lines.ToArray();
    }

    private static int CountWords(string line)
    {
        return Regex.Matches(line, @"[\p{L}\p{M}]+", RegexOptions.CultureInvariant).Count;
    }

    private readonly record struct LineInfo(int Start, string Text);
}
