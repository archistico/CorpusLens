using System.Linq;
using CorpusLens.Domain.Books;
using CorpusLens.Domain.Storage;
using CorpusLens.Infrastructure.Reports;
using Xunit;

namespace CorpusLens.Infrastructure.Tests;

public sealed class ImportDiagnosticsWriterTests
{
    [Fact]
    public void BuildMarkdown_ShouldReportFailuresSuspiciousTermsAndShortChapters()
    {
        ImportedBook book = new(
            "book-1",
            "Italian Sample",
            "Author",
            "it",
            "sample.epub",
            new[]
            {
                new ImportedChapter(1, "Informazioni", "info.xhtml", string.Empty, "Informazioni\nQUESTO E-BOOK:\nLICENZA:\nLiber Liber"),
                new ImportedChapter(2, "Capitolo I", "chapter1.xhtml", string.Empty, "Era una bella giornata. Il principe guardò la casa.")
            });
        EpubImportFailure failure = new("bad.epub", "bad.epub", "A local file header is corrupt.", "InvalidDataException");

        ImportDiagnosticsWriter writer = new();
        string markdown = writer.BuildMarkdown(new[] { book }, new[] { failure });

        Assert.Contains("# CorpusLens — Import diagnostics", markdown);
        Assert.Contains("Import failures", markdown);
        Assert.Contains("bad.epub", markdown);
        Assert.Contains("liber liber", markdown.ToLowerInvariant());
        Assert.Contains("Suspicious chapters", markdown);
        Assert.Contains("Informazioni", markdown);
        Assert.Contains("Shortest chapters", markdown);
    }

    [Fact]
    public void BuildMarkdown_ForStoredRun_ShouldIncludeRunMetadataAndChapterDiagnostics()
    {
        StoredAnalysisRunSummary run = new(
            7,
            1,
            "Italian Literature",
            2,
            "EPUB folder: it",
            DateTimeOffset.Parse("2026-07-09T20:00:00+00:00", System.Globalization.CultureInfo.InvariantCulture),
            DateTimeOffset.Parse("2026-07-09T20:00:01+00:00", System.Globalization.CultureInfo.InvariantCulture),
            "Completed",
            10,
            100,
            80,
            40,
            8.0,
            4.0,
            "report.md");
        StoredChapter chapter = new(
            1,
            2,
            1,
            "Indice",
            "toc.xhtml",
            "Indice\nCapitolo I\nCapitolo II\nCapitolo III\nCapitolo IV\nCapitolo V",
            68);

        ImportDiagnosticsWriter writer = new();
        string markdown = writer.BuildMarkdown(run, new[] { chapter });

        Assert.Contains("Run Id: 7", markdown);
        Assert.Contains("Italian Literature", markdown);
        Assert.Contains("EPUB folder: it", markdown);
        Assert.Contains("Suspicious chapters", markdown);
        Assert.Contains("Indice", markdown);
    }
    [Fact]
    public void BuildMarkdown_ShouldNotFlagLongNarrativeChapterOnlyBecauseItMentionsIndice()
    {
        string longNarrative = "Il personaggio consultò l'indice del libro e poi uscì dalla stanza. "
            + string.Concat(Enumerable.Repeat("Questa è una frase narrativa abbastanza lunga e non contiene metadati editoriali. ", 180));

        ImportedBook book = new(
            "book-2",
            "Narrative Sample",
            "Author",
            "it",
            "sample.epub",
            new[]
            {
                new ImportedChapter(1, "Capitolo I", "chapter1.xhtml", string.Empty, longNarrative)
            });

        ImportDiagnosticsWriter writer = new();
        string markdown = writer.BuildMarkdown(new[] { book }, Array.Empty<EpubImportFailure>());

        Assert.DoesNotContain("contains possible front matter term 'indice'", markdown, StringComparison.OrdinalIgnoreCase);
    }

}
