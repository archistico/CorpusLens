using CorpusLens.Analysis.Statistics;
using CorpusLens.Domain.Analysis;
using CorpusLens.Domain.Text;
using CorpusLens.Infrastructure.Reports;

namespace CorpusLens.Application.TextAnalysis;

public sealed class AnalyzeTextUseCase
{
    private readonly CorpusAnalyzer _analyzer;
    private readonly MarkdownReportWriter _markdownReportWriter;
    private readonly CsvReportWriter _csvReportWriter;

    public AnalyzeTextUseCase()
        : this(new CorpusAnalyzer(), new MarkdownReportWriter(), new CsvReportWriter())
    {
    }

    public AnalyzeTextUseCase(
        CorpusAnalyzer analyzer,
        MarkdownReportWriter markdownReportWriter,
        CsvReportWriter csvReportWriter)
    {
        _analyzer = analyzer;
        _markdownReportWriter = markdownReportWriter;
        _csvReportWriter = csvReportWriter;
    }

    public async Task<AnalyzeTextResult> ExecuteAsync(
        AnalyzeTextRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.OutputDirectory);

        Directory.CreateDirectory(request.OutputDirectory);

        TextDocument document = new(
            request.DocumentId,
            request.Title,
            request.LanguageCode,
            request.Text);

        CorpusAnalysisResult analysis = _analyzer.Analyze(document, request.Settings);

        string reportPath = Path.Combine(request.OutputDirectory, "report.md");
        await _markdownReportWriter
            .WriteAsync(analysis, reportPath, request.Title, cancellationToken)
            .ConfigureAwait(false);

        await _csvReportWriter
            .WriteAsync(analysis, request.OutputDirectory, cancellationToken)
            .ConfigureAwait(false);

        return new AnalyzeTextResult(
            analysis,
            reportPath,
            Path.Combine(request.OutputDirectory, "words.csv"),
            Path.Combine(request.OutputDirectory, "ngrams.csv"),
            Path.Combine(request.OutputDirectory, "next_words.csv"));
    }
}
