using CorpusLens.Analysis.Statistics;
using CorpusLens.Domain.Text;
using CorpusLens.Infrastructure.Files;
using CorpusLens.Infrastructure.Reports;

namespace CorpusLens.Application.TextAnalysis;

public sealed class AnalyzeTextFileUseCase
{
    private readonly TextDocumentLoader _loader;
    private readonly CorpusAnalyzer _analyzer;
    private readonly MarkdownReportWriter _markdownReportWriter;
    private readonly CsvReportWriter _csvReportWriter;

    public AnalyzeTextFileUseCase()
        : this(new TextDocumentLoader(), new CorpusAnalyzer(), new MarkdownReportWriter(), new CsvReportWriter())
    {
    }

    public AnalyzeTextFileUseCase(
        TextDocumentLoader loader,
        CorpusAnalyzer analyzer,
        MarkdownReportWriter markdownReportWriter,
        CsvReportWriter csvReportWriter)
    {
        _loader = loader;
        _analyzer = analyzer;
        _markdownReportWriter = markdownReportWriter;
        _csvReportWriter = csvReportWriter;
    }

    public async Task<AnalyzeTextResult> ExecuteAsync(
        AnalyzeTextFileRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        TextDocument document = await _loader
            .LoadAsync(request.FilePath, request.LanguageCode, request.Title, cancellationToken)
            .ConfigureAwait(false);

        AnalyzeTextRequest textRequest = new(
            document.Content,
            document.Id,
            document.Title,
            document.LanguageCode,
            request.OutputDirectory,
            request.Settings);

        AnalyzeTextUseCase useCase = new(_analyzer, _markdownReportWriter, _csvReportWriter);
        return await useCase.ExecuteAsync(textRequest, cancellationToken).ConfigureAwait(false);
    }
}
