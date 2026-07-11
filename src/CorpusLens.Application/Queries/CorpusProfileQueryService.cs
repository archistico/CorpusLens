using CorpusLens.Analysis.Language;
using CorpusLens.Analysis.StopWords;
using CorpusLens.Domain.Storage;
using CorpusLens.Infrastructure.Storage;

namespace CorpusLens.Application.Queries;

public sealed class CorpusProfileQueryService
{
    public async Task<CorpusProfileResult?> GetProfileAsync(
        CorpusProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        SqliteCorpusStore store = new(request.DatabasePath);
        StoredAnalysisRunSummary? run = await store
            .GetAnalysisRunSummaryAsync(request.AnalysisRunId, cancellationToken)
            .ConfigureAwait(false);
        if (run is null)
        {
            return null;
        }

        IReadOnlyList<StoredAnalysisRunBook> sourceBooks = await store
            .ListAnalysisRunBooksAsync(request.AnalysisRunId, cancellationToken)
            .ConfigureAwait(false);
        IReadOnlyList<string> languageCodes = ReadLanguageCodes(sourceBooks);
        LanguageProfile languageProfile = SelectDifficultyProfile(languageCodes);
        DifficultyThresholds thresholds = ResolveDifficultyThresholds(
            languageProfile,
            request.LongWordLength,
            request.VeryLongWordLength);

        StoredDifficultyProfile? difficultyProfile = await store
            .GetDifficultyProfileAsync(
                request.AnalysisRunId,
                thresholds.LongWordLength,
                thresholds.VeryLongWordLength,
                cancellationToken)
            .ConfigureAwait(false);

        int wordLimit = Math.Max(1, request.WordLimit);
        IReadOnlyList<StoredWordStatistic> contentWords = await store
            .ListTopWordsAsync(request.AnalysisRunId, wordLimit, StoredWordFilter.ContentOnly, cancellationToken)
            .ConfigureAwait(false);
        IReadOnlyList<StoredWordStatistic> functionWords = await store
            .ListTopWordsAsync(request.AnalysisRunId, wordLimit, StoredWordFilter.FunctionOnly, cancellationToken)
            .ConfigureAwait(false);

        int phraseLimit = Math.Max(1, request.PhraseLimit);
        int minPhraseCount = Math.Max(1, request.MinPhraseCount);
        int minPhraseChapters = Math.Max(1, request.MinPhraseChapters);
        int phraseFetchLimit = Math.Min(Math.Max(phraseLimit * 40, 500), 10_000);
        IReadOnlyList<StoredPhraseStatistic> candidatePhrases = await store
            .ListPhrasesAsync(request.AnalysisRunId, minN: 2, maxN: 5, minCount: minPhraseCount, limit: phraseFetchLimit, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        IReadOnlyList<StoredPhraseStatistic> phrases = KeepLongestOnlyPhrases(candidatePhrases
                .Where(phrase => phrase.ChapterCount >= minPhraseChapters)
                .Where(phrase => HasContentWordBoundary(phrase.Phrase, languageCodes))
                .ToArray())
            .Take(phraseLimit)
            .ToArray();

        return new CorpusProfileResult(
            run,
            sourceBooks,
            languageCodes,
            languageProfile,
            thresholds,
            difficultyProfile,
            contentWords,
            functionWords,
            phrases);
    }

    public static IReadOnlyList<string> ReadLanguageCodes(IReadOnlyList<StoredAnalysisRunBook> sourceBooks)
    {
        string[] languageCodes = sourceBooks
            .Select(book => book.LanguageCode)
            .Where(code => !string.IsNullOrWhiteSpace(code))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return languageCodes.Length == 0
            ? new[] { "en", "it", "fr", "de" }
            : languageCodes;
    }

    public static LanguageProfile SelectDifficultyProfile(IReadOnlyList<string> languageCodes)
    {
        string[] distinctCodes = languageCodes
            .Where(code => !string.IsNullOrWhiteSpace(code))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return distinctCodes.Length == 1
            ? LanguageProfileProvider.GetProfile(distinctCodes[0])
            : LanguageProfileProvider.GetProfile("unknown");
    }

    public static DifficultyThresholds ResolveDifficultyThresholds(
        LanguageProfile languageProfile,
        int? longWordLength,
        int? veryLongWordLength)
    {
        int resolvedLongWordLength = Math.Max(2, longWordLength ?? languageProfile.DefaultLongWordLength);
        int resolvedVeryLongWordLength = Math.Max(
            resolvedLongWordLength,
            veryLongWordLength ?? languageProfile.DefaultVeryLongWordLength);

        return new DifficultyThresholds(resolvedLongWordLength, resolvedVeryLongWordLength);
    }

    private static bool HasContentWordBoundary(string phrase, IReadOnlyList<string> languageCodes)
    {
        string[] words = phrase.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return words.Length > 0
            && !StopWordProvider.IsStopWord(words[0], languageCodes)
            && !StopWordProvider.IsStopWord(words[^1], languageCodes);
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

    private static bool IsRedundantNestedPhrase(StoredPhraseStatistic candidate, StoredPhraseStatistic other)
    {
        if (other.N <= candidate.N
            || other.Count != candidate.Count
            || other.ChapterCount != candidate.ChapterCount)
        {
            return false;
        }

        string[] candidateWords = candidate.Phrase.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        string[] otherWords = other.Phrase.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (otherWords.Length <= candidateWords.Length)
        {
            return false;
        }

        for (int startIndex = 0; startIndex <= otherWords.Length - candidateWords.Length; startIndex++)
        {
            bool matched = true;
            for (int offset = 0; offset < candidateWords.Length; offset++)
            {
                if (!string.Equals(otherWords[startIndex + offset], candidateWords[offset], StringComparison.OrdinalIgnoreCase))
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
