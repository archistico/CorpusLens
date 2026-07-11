using System.Globalization;
using System.Text;
using CorpusLens.Domain.Analysis;

namespace CorpusLens.Infrastructure.Reports;

public sealed class MarkdownReportWriter
{
    public async Task WriteAsync(
        CorpusAnalysisResult result,
        string filePath,
        string title,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        StringBuilder builder = new();
        builder.AppendLine($"# CorpusLens Report — {title}");
        builder.AppendLine();
        AppendSummary(builder, result.Summary);
        AppendTopWords(builder, result.Words.Take(30));
        AppendTopContentWords(builder, result.Words.Where(word => !word.IsStopWord).Take(30));
        AppendTopFunctionWords(builder, result.Words.Where(word => word.IsStopWord).Take(30));
        AppendTopNGrams(builder, result.NGrams.Take(40));
        AppendNextWords(builder, result.NextWords
            .OrderByDescending(next => next.Count)
            .ThenBy(next => next.Word, StringComparer.Ordinal)
            .ThenBy(next => next.NextWord, StringComparer.Ordinal)
            .Take(50));
        AppendSentenceCategories(builder, result.Sentences);

        string? directory = Path.GetDirectoryName(Path.GetFullPath(filePath));
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await File.WriteAllTextAsync(filePath, builder.ToString(), Encoding.UTF8, cancellationToken).ConfigureAwait(false);
    }

    private static void AppendSummary(StringBuilder builder, CorpusSummary summary)
    {
        builder.AppendLine("## Summary");
        builder.AppendLine();
        builder.AppendLine("| Metric | Value |");
        builder.AppendLine("|---|---:|");
        builder.AppendLine($"| Documents | {summary.DocumentCount} |");
        builder.AppendLine($"| Sentences | {summary.SentenceCount} |");
        builder.AppendLine($"| Tokens | {summary.TokenCount} |");
        builder.AppendLine($"| Word tokens | {summary.WordTokenCount} |");
        builder.AppendLine($"| Distinct words | {summary.DistinctWordCount} |");
        builder.AppendLine($"| Average words per sentence | {FormatDouble(summary.AverageWordsPerSentence)} |");
        builder.AppendLine($"| Average characters per word | {FormatDouble(summary.AverageCharactersPerWord)} |");
        builder.AppendLine();
    }

    private static void AppendTopWords(StringBuilder builder, IEnumerable<WordFrequency> words)
    {
        builder.AppendLine("## Top words");
        builder.AppendLine();
        builder.AppendLine("| Word | Count | Documents | Per million |");
        builder.AppendLine("|---|---:|---:|---:|");
        foreach (WordFrequency word in words)
        {
            builder.AppendLine($"| {EscapeMarkdown(word.Word)} | {word.Count} | {word.DocumentCount} | {FormatDouble(word.FrequencyPerMillion)} |");
        }

        builder.AppendLine();
    }

    private static void AppendTopContentWords(StringBuilder builder, IEnumerable<WordFrequency> words)
    {
        builder.AppendLine("## Top content words");
        builder.AppendLine();
        builder.AppendLine("| Word | Count | Documents | Per million |");
        builder.AppendLine("|---|---:|---:|---:|");
        foreach (WordFrequency word in words)
        {
            builder.AppendLine($"| {EscapeMarkdown(word.Word)} | {word.Count} | {word.DocumentCount} | {FormatDouble(word.FrequencyPerMillion)} |");
        }

        builder.AppendLine();
    }

    private static void AppendTopFunctionWords(StringBuilder builder, IEnumerable<WordFrequency> words)
    {
        builder.AppendLine("## Top function words");
        builder.AppendLine();
        builder.AppendLine("| Word | Count | Documents | Per million |");
        builder.AppendLine("|---|---:|---:|---:|");
        foreach (WordFrequency word in words)
        {
            builder.AppendLine($"| {EscapeMarkdown(word.Word)} | {word.Count} | {word.DocumentCount} | {FormatDouble(word.FrequencyPerMillion)} |");
        }

        builder.AppendLine();
    }

    private static void AppendTopNGrams(StringBuilder builder, IEnumerable<NGramFrequency> ngrams)
    {
        builder.AppendLine("## Top n-grams");
        builder.AppendLine();
        builder.AppendLine("| N | Text | Count | Documents | Per million |");
        builder.AppendLine("|---:|---|---:|---:|---:|");
        foreach (NGramFrequency ngram in ngrams)
        {
            builder.AppendLine($"| {ngram.N} | {EscapeMarkdown(ngram.Text)} | {ngram.Count} | {ngram.DocumentCount} | {FormatDouble(ngram.FrequencyPerMillion)} |");
        }

        builder.AppendLine();
    }

    private static void AppendNextWords(StringBuilder builder, IEnumerable<NextWordFrequency> nextWords)
    {
        builder.AppendLine("## Next words");
        builder.AppendLine();
        builder.AppendLine("| Word | Next word | Count | Probability |");
        builder.AppendLine("|---|---|---:|---:|");
        foreach (NextWordFrequency nextWord in nextWords)
        {
            builder.AppendLine($"| {EscapeMarkdown(nextWord.Word)} | {EscapeMarkdown(nextWord.NextWord)} | {nextWord.Count} | {FormatDouble(nextWord.Probability)} |");
        }

        builder.AppendLine();
    }

    private static void AppendSentenceCategories(StringBuilder builder, IReadOnlyList<AnalyzedSentence> sentences)
    {
        builder.AppendLine("## Sentence categories");
        builder.AppendLine();
        builder.AppendLine("| Category | Count |");
        builder.AppendLine("|---|---:|");

        foreach (IGrouping<PhraseCategory, AnalyzedSentence> group in sentences
            .GroupBy(sentence => sentence.Category)
            .OrderByDescending(group => group.Count())
            .ThenBy(group => group.Key.ToString(), StringComparer.Ordinal))
        {
            builder.AppendLine($"| {group.Key} | {group.Count()} |");
        }

        builder.AppendLine();
    }

    private static string FormatDouble(double value)
    {
        return value.ToString("0.####", CultureInfo.InvariantCulture);
    }

    private static string EscapeMarkdown(string value)
    {
        return value.Replace("|", "\\|", StringComparison.Ordinal);
    }
}
