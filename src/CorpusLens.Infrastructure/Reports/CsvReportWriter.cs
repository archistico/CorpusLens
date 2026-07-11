using CorpusLens.Domain.Analysis;

namespace CorpusLens.Infrastructure.Reports;

public sealed class CsvReportWriter
{
    public async Task WriteAsync(
        CorpusAnalysisResult result,
        string outputDirectory,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputDirectory);

        Directory.CreateDirectory(outputDirectory);

        await WriteWordsAsync(result, outputDirectory, cancellationToken).ConfigureAwait(false);
        await WriteNGramsAsync(result, outputDirectory, cancellationToken).ConfigureAwait(false);
        await WriteNextWordsAsync(result, outputDirectory, cancellationToken).ConfigureAwait(false);
    }

    private static Task WriteWordsAsync(
        CorpusAnalysisResult result,
        string outputDirectory,
        CancellationToken cancellationToken)
    {
        string filePath = Path.Combine(outputDirectory, "words.csv");
        IReadOnlyList<IReadOnlyList<string>> rows = result.Words
            .Select(word => (IReadOnlyList<string>)new[]
            {
                word.Word,
                word.Count.ToString(System.Globalization.CultureInfo.InvariantCulture),
                word.DocumentCount.ToString(System.Globalization.CultureInfo.InvariantCulture),
                CsvWriter.FormatDouble(word.FrequencyPerMillion),
                word.IsStopWord ? "true" : "false"
            })
            .ToArray();

        return CsvWriter.WriteAsync(
            filePath,
            new[] { "word", "count", "document_count", "frequency_per_million", "is_stop_word" },
            rows,
            cancellationToken);
    }

    private static Task WriteNGramsAsync(
        CorpusAnalysisResult result,
        string outputDirectory,
        CancellationToken cancellationToken)
    {
        string filePath = Path.Combine(outputDirectory, "ngrams.csv");
        IReadOnlyList<IReadOnlyList<string>> rows = result.NGrams
            .Select(ngram => (IReadOnlyList<string>)new[]
            {
                ngram.N.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ngram.Text,
                ngram.Count.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ngram.DocumentCount.ToString(System.Globalization.CultureInfo.InvariantCulture),
                CsvWriter.FormatDouble(ngram.FrequencyPerMillion)
            })
            .ToArray();

        return CsvWriter.WriteAsync(
            filePath,
            new[] { "n", "text", "count", "document_count", "frequency_per_million" },
            rows,
            cancellationToken);
    }

    private static Task WriteNextWordsAsync(
        CorpusAnalysisResult result,
        string outputDirectory,
        CancellationToken cancellationToken)
    {
        string filePath = Path.Combine(outputDirectory, "next_words.csv");
        IReadOnlyList<IReadOnlyList<string>> rows = result.NextWords
            .OrderByDescending(nextWord => nextWord.Count)
            .ThenBy(nextWord => nextWord.Word, StringComparer.Ordinal)
            .ThenBy(nextWord => nextWord.NextWord, StringComparer.Ordinal)
            .Select(nextWord => (IReadOnlyList<string>)new[]
            {
                nextWord.Word,
                nextWord.NextWord,
                nextWord.Count.ToString(System.Globalization.CultureInfo.InvariantCulture),
                CsvWriter.FormatDouble(nextWord.Probability)
            })
            .ToArray();

        return CsvWriter.WriteAsync(
            filePath,
            new[] { "word", "next_word", "count", "probability" },
            rows,
            cancellationToken);
    }
}
