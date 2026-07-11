using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using CorpusLens.Domain.Books;
using CorpusLens.Domain.Storage;

namespace CorpusLens.Infrastructure.Reports;

public sealed class ImportDiagnosticsWriter
{
    private const int ShortChapterThreshold = 500;
    private const int LongChapterThreshold = 50000;

    private static readonly string[] SuspiciousTerms =
    {
        "project gutenberg",
        "gutenberg",
        "copyright",
        "license",
        "licence",
        "donation",
        "donations",
        "ebook",
        "e-book",
        "transcriber",
        "publisher",
        "archive",
        "liber liber",
        "liberliber",
        "licenza",
        "questo e-book",
        "indice",
        "sommario",
        "prefazione",
        "editore",
        "www",
        "https"
    };

    private static readonly string[] StrongSuspiciousTerms =
    {
        "project gutenberg",
        "gutenberg",
        "copyright",
        "license",
        "licence",
        "donation",
        "donations",
        "ebook",
        "e-book",
        "transcriber",
        "publisher",
        "archive",
        "liber liber",
        "liberliber",
        "licenza",
        "questo e-book",
        "www",
        "https"
    };

    private static readonly string[] WeakSuspiciousTerms =
    {
        "indice",
        "sommario",
        "prefazione",
        "editore"
    };

    public async Task WriteAsync(
        IReadOnlyList<ImportedBook> books,
        IReadOnlyList<EpubImportFailure> failures,
        string outputPath,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(books);
        ArgumentNullException.ThrowIfNull(failures);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);

        string markdown = BuildMarkdown(books, failures);
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(outputPath)) ?? ".");
        await File.WriteAllTextAsync(outputPath, markdown, cancellationToken).ConfigureAwait(false);
    }

    public async Task WriteAsync(
        StoredAnalysisRunSummary run,
        IReadOnlyList<StoredChapter> chapters,
        string outputPath,
        CancellationToken cancellationToken = default)
    {
        await WriteAsync(run, chapters, outputPath, 1, cancellationToken).ConfigureAwait(false);
    }

    public async Task WriteAsync(
        StoredAnalysisRunSummary run,
        IReadOnlyList<StoredChapter> chapters,
        string outputPath,
        int bookCount,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(run);
        ArgumentNullException.ThrowIfNull(chapters);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);

        string markdown = BuildMarkdown(run, chapters, bookCount);
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(outputPath)) ?? ".");
        await File.WriteAllTextAsync(outputPath, markdown, cancellationToken).ConfigureAwait(false);
    }

    public string BuildMarkdown(IReadOnlyList<ImportedBook> books, IReadOnlyList<EpubImportFailure> failures)
    {
        ArgumentNullException.ThrowIfNull(books);
        ArgumentNullException.ThrowIfNull(failures);

        IReadOnlyList<DiagnosticChapter> chapters = books
            .SelectMany(book => book.Chapters.Select(chapter => new DiagnosticChapter(
                book.Title,
                chapter.OrderIndex,
                chapter.Title,
                chapter.SourcePath,
                chapter.CleanText)))
            .ToArray();

        StringBuilder builder = new();
        builder.AppendLine("# CorpusLens — Import diagnostics");
        builder.AppendLine();
        builder.AppendLine("## Summary");
        builder.AppendLine();
        AppendSummary(builder, books.Count, failures.Count, chapters);
        AppendFailures(builder, failures);
        AppendSuspiciousTerms(builder, chapters);
        AppendSuspiciousChapters(builder, chapters);
        AppendShortestChapters(builder, chapters);
        AppendLongestChapters(builder, chapters);
        return builder.ToString();
    }

    public string BuildMarkdown(StoredAnalysisRunSummary run, IReadOnlyList<StoredChapter> chapters)
    {
        return BuildMarkdown(run, chapters, 1);
    }

    public string BuildMarkdown(StoredAnalysisRunSummary run, IReadOnlyList<StoredChapter> chapters, int bookCount)
    {
        ArgumentNullException.ThrowIfNull(run);
        ArgumentNullException.ThrowIfNull(chapters);

        IReadOnlyList<DiagnosticChapter> diagnosticChapters = chapters
            .Select(chapter => new DiagnosticChapter(
                run.BookTitle,
                chapter.OrderIndex,
                chapter.Title,
                chapter.SourcePath,
                chapter.CleanText))
            .ToArray();

        StringBuilder builder = new();
        builder.AppendLine("# CorpusLens — Import diagnostics");
        builder.AppendLine();
        builder.AppendLine("## Run");
        builder.AppendLine();
        builder.AppendLine($"- Run Id: {run.Id.ToString(CultureInfo.InvariantCulture)}");
        builder.AppendLine($"- Corpus: {run.CorpusName}");
        builder.AppendLine($"- Book/Source: {run.BookTitle}");
        builder.AppendLine($"- Report: {run.ReportPath}");
        builder.AppendLine();
        builder.AppendLine("## Summary");
        builder.AppendLine();
        AppendSummary(builder, Math.Max(1, bookCount), 0, diagnosticChapters);
        AppendSuspiciousTerms(builder, diagnosticChapters);
        AppendSuspiciousChapters(builder, diagnosticChapters);
        AppendShortestChapters(builder, diagnosticChapters);
        AppendLongestChapters(builder, diagnosticChapters);
        return builder.ToString();
    }

    private static void AppendSummary(StringBuilder builder, int bookCount, int failureCount, IReadOnlyList<DiagnosticChapter> chapters)
    {
        int chapterCount = chapters.Count;
        int totalCharacters = chapters.Sum(chapter => chapter.CharacterCount);
        double averageCharacters = chapterCount == 0 ? 0 : (double)totalCharacters / chapterCount;
        int shortChapters = chapters.Count(chapter => chapter.CharacterCount > 0 && chapter.CharacterCount < ShortChapterThreshold);
        int emptyChapters = chapters.Count(chapter => chapter.CharacterCount == 0);
        int longChapters = chapters.Count(chapter => chapter.CharacterCount > LongChapterThreshold);
        int suspiciousChapters = chapters.Count(IsSuspiciousChapter);

        builder.AppendLine("| Metric | Value |");
        builder.AppendLine("| --- | ---: |");
        builder.AppendLine($"| Books imported | {bookCount.ToString(CultureInfo.InvariantCulture)} |");
        builder.AppendLine($"| Import failures | {failureCount.ToString(CultureInfo.InvariantCulture)} |");
        builder.AppendLine($"| Chapters extracted | {chapterCount.ToString(CultureInfo.InvariantCulture)} |");
        builder.AppendLine($"| Total characters | {totalCharacters.ToString(CultureInfo.InvariantCulture)} |");
        builder.AppendLine($"| Average characters per chapter | {averageCharacters.ToString("0.##", CultureInfo.InvariantCulture)} |");
        builder.AppendLine($"| Empty chapters | {emptyChapters.ToString(CultureInfo.InvariantCulture)} |");
        builder.AppendLine($"| Short chapters (< {ShortChapterThreshold.ToString(CultureInfo.InvariantCulture)} chars) | {shortChapters.ToString(CultureInfo.InvariantCulture)} |");
        builder.AppendLine($"| Long chapters (> {LongChapterThreshold.ToString(CultureInfo.InvariantCulture)} chars) | {longChapters.ToString(CultureInfo.InvariantCulture)} |");
        builder.AppendLine($"| Suspicious chapters | {suspiciousChapters.ToString(CultureInfo.InvariantCulture)} |");
        builder.AppendLine();
    }

    private static void AppendFailures(StringBuilder builder, IReadOnlyList<EpubImportFailure> failures)
    {
        builder.AppendLine("## Import failures");
        builder.AppendLine();
        if (failures.Count == 0)
        {
            builder.AppendLine("No import failures.");
            builder.AppendLine();
            return;
        }

        builder.AppendLine("| File | Exception | Message |");
        builder.AppendLine("| --- | --- | --- |");
        foreach (EpubImportFailure failure in failures.OrderBy(failure => failure.FileName, StringComparer.OrdinalIgnoreCase))
        {
            builder.AppendLine($"| {EscapeMarkdown(failure.FileName)} | {EscapeMarkdown(failure.ExceptionType)} | {EscapeMarkdown(failure.ErrorMessage)} |");
        }

        builder.AppendLine();
    }

    private static void AppendSuspiciousTerms(StringBuilder builder, IReadOnlyList<DiagnosticChapter> chapters)
    {
        builder.AppendLine("## Suspicious terms");
        builder.AppendLine();
        string combinedText = string.Join("\n", chapters.Select(chapter => chapter.Text));
        List<(string Term, int Count)> foundTerms = SuspiciousTerms
            .Select(term => (Term: term, Count: CountTerm(combinedText, term)))
            .Where(item => item.Count > 0)
            .OrderByDescending(item => item.Count)
            .ThenBy(item => item.Term, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (foundTerms.Count == 0)
        {
            builder.AppendLine("No suspicious boilerplate terms found.");
            builder.AppendLine();
            return;
        }

        builder.AppendLine("| Term | Count |");
        builder.AppendLine("| --- | ---: |");
        foreach ((string term, int count) in foundTerms)
        {
            builder.AppendLine($"| {EscapeMarkdown(term)} | {count.ToString(CultureInfo.InvariantCulture)} |");
        }

        builder.AppendLine();
    }

    private static void AppendSuspiciousChapters(StringBuilder builder, IReadOnlyList<DiagnosticChapter> chapters)
    {
        builder.AppendLine("## Suspicious chapters");
        builder.AppendLine();
        IReadOnlyList<DiagnosticChapter> suspiciousChapters = chapters
            .Where(IsSuspiciousChapter)
            .OrderBy(chapter => chapter.BookTitle, StringComparer.OrdinalIgnoreCase)
            .ThenBy(chapter => chapter.OrderIndex)
            .Take(25)
            .ToArray();

        if (suspiciousChapters.Count == 0)
        {
            builder.AppendLine("No suspicious chapters found.");
            builder.AppendLine();
            return;
        }

        builder.AppendLine("| Book | Chapter | Characters | Reason |");
        builder.AppendLine("| --- | --- | ---: | --- |");
        foreach (DiagnosticChapter chapter in suspiciousChapters)
        {
            builder.AppendLine($"| {EscapeMarkdown(chapter.BookTitle)} | {EscapeMarkdown(ChapterLabel(chapter))} | {chapter.CharacterCount.ToString(CultureInfo.InvariantCulture)} | {EscapeMarkdown(SuspiciousReason(chapter))} |");
        }

        builder.AppendLine();
    }

    private static void AppendShortestChapters(StringBuilder builder, IReadOnlyList<DiagnosticChapter> chapters)
    {
        builder.AppendLine("## Shortest chapters");
        builder.AppendLine();
        AppendChapterTable(builder, chapters.OrderBy(chapter => chapter.CharacterCount).Take(15));
    }

    private static void AppendLongestChapters(StringBuilder builder, IReadOnlyList<DiagnosticChapter> chapters)
    {
        builder.AppendLine("## Longest chapters");
        builder.AppendLine();
        AppendChapterTable(builder, chapters.OrderByDescending(chapter => chapter.CharacterCount).Take(15));
    }

    private static void AppendChapterTable(StringBuilder builder, IEnumerable<DiagnosticChapter> chapters)
    {
        DiagnosticChapter[] chapterList = chapters.ToArray();
        if (chapterList.Length == 0)
        {
            builder.AppendLine("No chapters available.");
            builder.AppendLine();
            return;
        }

        builder.AppendLine("| Book | Chapter | Characters | Preview |");
        builder.AppendLine("| --- | --- | ---: | --- |");
        foreach (DiagnosticChapter chapter in chapterList)
        {
            builder.AppendLine($"| {EscapeMarkdown(chapter.BookTitle)} | {EscapeMarkdown(ChapterLabel(chapter))} | {chapter.CharacterCount.ToString(CultureInfo.InvariantCulture)} | {EscapeMarkdown(Preview(chapter.Text))} |");
        }

        builder.AppendLine();
    }

    private static bool IsSuspiciousChapter(DiagnosticChapter chapter)
    {
        if (chapter.CharacterCount == 0)
        {
            return true;
        }

        string text = chapter.Text;
        if (chapter.CharacterCount < ShortChapterThreshold && LooksLikeTableOfContents(text))
        {
            return true;
        }

        if (StrongSuspiciousTerms.Any(term => ContainsTerm(text, term)))
        {
            return true;
        }

        return FindWeakSuspiciousTerm(chapter) is not null;
    }

    private static string SuspiciousReason(DiagnosticChapter chapter)
    {
        if (chapter.CharacterCount == 0)
        {
            return "empty chapter";
        }

        if (chapter.CharacterCount < ShortChapterThreshold && LooksLikeTableOfContents(chapter.Text))
        {
            return "short table-of-contents-like chapter";
        }

        string? strongTerm = StrongSuspiciousTerms.FirstOrDefault(term => ContainsTerm(chapter.Text, term));
        if (strongTerm is not null)
        {
            return $"contains '{strongTerm}'";
        }

        string? weakTerm = FindWeakSuspiciousTerm(chapter);
        return weakTerm is null ? "unknown" : $"contains possible front matter term '{weakTerm}'";
    }
    private static string? FindWeakSuspiciousTerm(DiagnosticChapter chapter)
    {
        if (!WeakSuspiciousTerms.Any(term => ContainsTerm(chapter.Text, term)))
        {
            return null;
        }

        if (LooksLikeTableOfContents(chapter.Text))
        {
            return WeakSuspiciousTerms.First(term => ContainsTerm(chapter.Text, term));
        }

        if (chapter.CharacterCount < 5_000 && ContainsWeakTermNearTop(chapter.Text))
        {
            return WeakSuspiciousTerms.First(term => ContainsTerm(chapter.Text, term));
        }

        return null;
    }


    private static bool LooksLikeTableOfContents(string text)
    {
        string[] lines = text
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Take(60)
            .ToArray();

        if (lines.Length == 0)
        {
            return false;
        }

        bool hasContents = lines.Any(line => string.Equals(line, "contents", StringComparison.OrdinalIgnoreCase)
            || string.Equals(line, "indice", StringComparison.OrdinalIgnoreCase)
            || string.Equals(line, "sommario", StringComparison.OrdinalIgnoreCase));
        int chapterLines = lines.Count(line => Regex.IsMatch(line, @"^(chapter|capitolo)\s+([ivxlcdm]+|\d+)", RegexOptions.IgnoreCase));
        return hasContents || chapterLines >= 5;
    }


    private static bool ContainsWeakTermNearTop(string text)
    {
        string preview = text.Length <= 1_500 ? text : text[..1_500];
        return WeakSuspiciousTerms.Any(term => ContainsTerm(preview, term));
    }

    private static int CountTerm(string text, string term)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return 0;
        }

        return Regex.Matches(text, Regex.Escape(term), RegexOptions.IgnoreCase | RegexOptions.CultureInvariant).Count;
    }

    private static bool ContainsTerm(string text, string term)
    {
        return CountTerm(text, term) > 0;
    }

    private static string ChapterLabel(DiagnosticChapter chapter)
    {
        return string.IsNullOrWhiteSpace(chapter.ChapterTitle)
            ? $"#{chapter.OrderIndex.ToString(CultureInfo.InvariantCulture)}"
            : chapter.ChapterTitle;
    }

    private static string Preview(string text)
    {
        string normalized = Regex.Replace(text, @"\s+", " ").Trim();
        return normalized.Length <= 90 ? normalized : normalized[..87] + "...";
    }

    private static string EscapeMarkdown(string value)
    {
        return value
            .Replace("|", "\\|", StringComparison.Ordinal)
            .Replace("\r", " ", StringComparison.Ordinal)
            .Replace("\n", " ", StringComparison.Ordinal);
    }

    private sealed record DiagnosticChapter(
        string BookTitle,
        int OrderIndex,
        string ChapterTitle,
        string SourcePath,
        string Text)
    {
        public int CharacterCount => Text.Length;
    }
}
