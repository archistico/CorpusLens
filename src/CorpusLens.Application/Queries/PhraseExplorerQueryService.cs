using CorpusLens.Analysis.StopWords;
using CorpusLens.Domain.Storage;
using CorpusLens.Infrastructure.Storage;

namespace CorpusLens.Application.Queries;

public sealed class PhraseExplorerQueryService
{
    public async Task<PhraseExplorerResult> GetPhrasesAsync(
        PhraseExplorerRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.DatabasePath);

        int safeMinN = Math.Clamp(request.MinN, 2, 8);
        int safeMaxN = Math.Clamp(request.MaxN, safeMinN, 8);
        int minCount = Math.Max(1, request.MinCount);
        int minChapters = Math.Max(1, request.MinChapters);
        int limit = Math.Clamp(request.Limit, 1, 100);
        int fetchLimit = Math.Min(Math.Max(limit * 40, 500), 10_000);

        SqliteCorpusStore store = new(request.DatabasePath);
        IReadOnlyList<StoredAnalysisRunBook> sourceBooks = await store
            .ListAnalysisRunBooksAsync(request.AnalysisRunId, cancellationToken)
            .ConfigureAwait(false);
        IReadOnlyList<string> languageCodes = CorpusProfileQueryService.ReadLanguageCodes(sourceBooks);

        IReadOnlyList<StoredPhraseStatistic> allPhrases = await store
            .ListPhrasesAsync(request.AnalysisRunId, safeMinN, safeMaxN, minCount, fetchLimit, cancellationToken)
            .ConfigureAwait(false);

        IReadOnlyList<StoredPhraseStatistic> matchedPhrases = allPhrases
            .Where(phrase => phrase.ChapterCount >= minChapters)
            .Where(phrase => !request.ContentBoundaryOnly || HasContentWordBoundary(phrase.Phrase, languageCodes))
            .ToArray();

        if (request.LongestOnly)
        {
            matchedPhrases = KeepLongestOnlyPhrases(matchedPhrases);
        }

        PhraseExplorerItem[] shown = matchedPhrases
            .Take(limit)
            .Select(phrase => ToItem(phrase, languageCodes))
            .ToArray();

        return new PhraseExplorerResult(
            safeMinN,
            safeMaxN,
            minCount,
            minChapters,
            limit,
            request.ContentBoundaryOnly,
            request.LongestOnly,
            allPhrases.Count,
            matchedPhrases.Count,
            shown);
    }

    private static PhraseExplorerItem ToItem(
        StoredPhraseStatistic phrase,
        IReadOnlyList<string> languageCodes)
    {
        return new PhraseExplorerItem(
            phrase.Phrase,
            phrase.N,
            phrase.Count,
            phrase.ChapterCount,
            phrase.FrequencyPerMillion,
            PhraseBoundaryLabel(phrase.Phrase, languageCodes));
    }

    private static bool HasContentWordBoundary(string phrase, IReadOnlyList<string> languageCodes)
    {
        string[] words = SplitPhraseWords(phrase);
        return words.Length > 0
            && !StopWordProvider.IsStopWord(words[0], languageCodes)
            && !StopWordProvider.IsStopWord(words[^1], languageCodes);
    }

    private static string PhraseBoundaryLabel(string phrase, IReadOnlyList<string> languageCodes)
    {
        string[] words = SplitPhraseWords(phrase);
        if (words.Length == 0)
        {
            return "empty";
        }

        return $"{PhraseWordType(words[0], languageCodes)}/{PhraseWordType(words[^1], languageCodes)}";
    }

    private static string PhraseWordType(string word, IReadOnlyList<string> languageCodes)
    {
        return StopWordProvider.IsStopWord(word, languageCodes) ? "function" : "content";
    }

    private static string[] SplitPhraseWords(string phrase)
    {
        return phrase.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    private static IReadOnlyList<StoredPhraseStatistic> KeepLongestOnlyPhrases(
        IReadOnlyList<StoredPhraseStatistic> phrases)
    {
        return phrases
            .Where(candidate => !phrases.Any(other => IsRedundantNestedPhrase(candidate, other)))
            .OrderByDescending(phrase => phrase.Count)
            .ThenByDescending(phrase => phrase.N)
            .ThenBy(phrase => phrase.Phrase, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static bool IsRedundantNestedPhrase(
        StoredPhraseStatistic candidate,
        StoredPhraseStatistic other)
    {
        if (other.N <= candidate.N
            || other.Count != candidate.Count
            || other.ChapterCount != candidate.ChapterCount)
        {
            return false;
        }

        string[] candidateWords = SplitPhraseWords(candidate.Phrase);
        string[] otherWords = SplitPhraseWords(other.Phrase);
        if (otherWords.Length <= candidateWords.Length)
        {
            return false;
        }

        return ContainsContiguousWords(otherWords, candidateWords);
    }

    private static bool ContainsContiguousWords(string[] longer, string[] shorter)
    {
        for (int startIndex = 0; startIndex <= longer.Length - shorter.Length; startIndex++)
        {
            bool matched = true;
            for (int offset = 0; offset < shorter.Length; offset++)
            {
                if (!string.Equals(
                    longer[startIndex + offset],
                    shorter[offset],
                    StringComparison.OrdinalIgnoreCase))
                {
                    matched = false;
                    break;
                }
            }

            if (matched)
            {
                return true;
            }
        }

        return false;
    }
}
