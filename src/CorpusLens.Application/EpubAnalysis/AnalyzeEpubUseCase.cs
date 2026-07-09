using CorpusLens.Analysis.Statistics;
using CorpusLens.Domain.Analysis;
using CorpusLens.Domain.Books;
using CorpusLens.Domain.Text;
using CorpusLens.Infrastructure.Epub;
using CorpusLens.Infrastructure.Reports;

namespace CorpusLens.Application.EpubAnalysis;

public sealed class AnalyzeEpubUseCase
{
    private readonly EpubBookReader _bookReader;
    private readonly CorpusAnalyzer _analyzer;
    private readonly MarkdownReportWriter _markdownReportWriter;
    private readonly CsvReportWriter _csvReportWriter;

    public AnalyzeEpubUseCase()
        : this(new EpubBookReader(), new CorpusAnalyzer(), new MarkdownReportWriter(), new CsvReportWriter())
    {
    }

    public AnalyzeEpubUseCase(
        EpubBookReader bookReader,
        CorpusAnalyzer analyzer,
        MarkdownReportWriter markdownReportWriter,
        CsvReportWriter csvReportWriter)
    {
        _bookReader = bookReader;
        _analyzer = analyzer;
        _markdownReportWriter = markdownReportWriter;
        _csvReportWriter = csvReportWriter;
    }

    public async Task<AnalyzeEpubResult> ExecuteAsync(
        AnalyzeEpubRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.OutputDirectory);

        ImportedBook book = await _bookReader
            .ReadAsync(request.FilePath, request.LanguageCode, cancellationToken)
            .ConfigureAwait(false);

        Directory.CreateDirectory(request.OutputDirectory);

        string extractedTextPath = Path.Combine(request.OutputDirectory, "extracted_text.txt");
        await File.WriteAllTextAsync(extractedTextPath, book.Content, cancellationToken).ConfigureAwait(false);

        TextDocument document = new(book.Id, book.Title, book.LanguageCode, book.Content);
        CorpusAnalysisResult analysis = _analyzer.Analyze(document, request.Settings);

        string reportPath = Path.Combine(request.OutputDirectory, "report.md");
        await _markdownReportWriter
            .WriteAsync(analysis, reportPath, book.Title, cancellationToken)
            .ConfigureAwait(false);

        await _csvReportWriter
            .WriteAsync(analysis, request.OutputDirectory, cancellationToken)
            .ConfigureAwait(false);

        return new AnalyzeEpubResult(
            book,
            analysis,
            reportPath,
            Path.Combine(request.OutputDirectory, "words.csv"),
            Path.Combine(request.OutputDirectory, "ngrams.csv"),
            Path.Combine(request.OutputDirectory, "next_words.csv"),
            extractedTextPath);
    }
}
