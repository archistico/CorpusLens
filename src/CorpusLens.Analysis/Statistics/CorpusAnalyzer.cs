using CorpusLens.Analysis.Classification;
using CorpusLens.Analysis.Normalization;
using CorpusLens.Analysis.Sentences;
using CorpusLens.Analysis.StopWords;
using CorpusLens.Analysis.Tokens;
using CorpusLens.Domain.Analysis;
using CorpusLens.Domain.Text;

namespace CorpusLens.Analysis.Statistics;

public sealed class CorpusAnalyzer
{
    private readonly TextNormalizer _normalizer;
    private readonly SentenceSplitter _sentenceSplitter;
    private readonly Tokenizer _tokenizer;
    private readonly SimplePhraseClassifier _phraseClassifier;

    public CorpusAnalyzer()
        : this(new TextNormalizer())
    {
    }

    public CorpusAnalyzer(TextNormalizer normalizer)
    {
        _normalizer = normalizer;
        _sentenceSplitter = new SentenceSplitter(normalizer);
        _tokenizer = new Tokenizer();
        _phraseClassifier = new SimplePhraseClassifier();
    }

    public CorpusAnalysisResult Analyze(IEnumerable<TextDocument> documents, AnalysisSettings settings)
    {
        ArgumentNullException.ThrowIfNull(documents);
        ArgumentNullException.ThrowIfNull(settings);

        List<DocumentAnalysis> analyzedDocuments = documents
            .Select(document => AnalyzeDocument(document, settings))
            .ToList();

        CorpusSummary summary = BuildSummary(analyzedDocuments);
        IReadOnlyList<WordFrequency> words = BuildWordFrequencies(analyzedDocuments, summary.WordTokenCount);
        IReadOnlyList<NGramFrequency> ngrams = BuildNGramFrequencies(analyzedDocuments, settings, summary.WordTokenCount);
        IReadOnlyList<NextWordFrequency> nextWords = BuildNextWordFrequencies(analyzedDocuments, settings, words);
        IReadOnlyList<AnalyzedSentence> sentences = analyzedDocuments
            .SelectMany(document => document.Sentences)
            .ToArray();

        return new CorpusAnalysisResult(summary, words, ngrams, nextWords, sentences);
    }

    public CorpusAnalysisResult Analyze(TextDocument document, AnalysisSettings settings)
    {
        ArgumentNullException.ThrowIfNull(document);

        return Analyze(new[] { document }, settings);
    }

    private DocumentAnalysis AnalyzeDocument(TextDocument document, AnalysisSettings settings)
    {
        string cleanText = _normalizer.NormalizeForReading(document.Content);
        IReadOnlyList<TextSentence> sentences = _sentenceSplitter.Split(cleanText);
        List<SentenceTokens> sentenceTokens = new();
        List<AnalyzedSentence> analyzedSentences = new();

        foreach (TextSentence sentence in sentences)
        {
            IReadOnlyList<TextToken> tokens = _tokenizer.Tokenize(sentence);
            sentenceTokens.Add(new SentenceTokens(sentence, tokens));

            PhraseCategory category = _phraseClassifier.Classify(sentence, tokens);
            int tokenCount = tokens.Count(token => settings.IncludePunctuationTokens || token.Kind != TokenKind.Punctuation);
            analyzedSentences.Add(new AnalyzedSentence(sentence.Text, sentence.NormalizedText, category, tokenCount));
        }

        return new DocumentAnalysis(document, sentenceTokens, analyzedSentences);
    }

    private static CorpusSummary BuildSummary(IReadOnlyList<DocumentAnalysis> documents)
    {
        int sentenceCount = documents.Sum(document => document.SentenceTokens.Count);
        IReadOnlyList<TextToken> allTokens = documents
            .SelectMany(document => document.SentenceTokens)
            .SelectMany(sentence => sentence.Tokens)
            .ToArray();
        IReadOnlyList<TextToken> wordTokens = allTokens
            .Where(token => token.Kind == TokenKind.Word)
            .ToArray();

        int distinctWordCount = wordTokens
            .Select(token => token.NormalizedText)
            .Distinct(StringComparer.Ordinal)
            .Count();

        double averageWordsPerSentence = sentenceCount == 0
            ? 0
            : wordTokens.Count / (double)sentenceCount;

        double averageCharactersPerWord = wordTokens.Count == 0
            ? 0
            : wordTokens.Average(token => token.NormalizedText.Length);

        return new CorpusSummary(
            documents.Count,
            sentenceCount,
            allTokens.Count,
            wordTokens.Count,
            distinctWordCount,
            averageWordsPerSentence,
            averageCharactersPerWord);
    }

    private static IReadOnlyList<WordFrequency> BuildWordFrequencies(
        IReadOnlyList<DocumentAnalysis> documents,
        int totalWordCount)
    {
        Dictionary<string, WordAccumulator> accumulators = new(StringComparer.Ordinal);
        IReadOnlyList<string> languageCodes = documents
            .Select(document => document.Document.LanguageCode)
            .Where(languageCode => !string.IsNullOrWhiteSpace(languageCode))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        foreach (DocumentAnalysis document in documents)
        {
            HashSet<string> wordsInDocument = new(StringComparer.Ordinal);
            foreach (TextToken token in document.WordTokens)
            {
                if (!accumulators.TryGetValue(token.NormalizedText, out WordAccumulator? accumulator))
                {
                    accumulator = new WordAccumulator(token.NormalizedText);
                    accumulators[token.NormalizedText] = accumulator;
                }

                accumulator.Count++;
                wordsInDocument.Add(token.NormalizedText);
            }

            foreach (string word in wordsInDocument)
            {
                accumulators[word].DocumentCount++;
            }
        }

        return accumulators.Values
            .Select(accumulator => new WordFrequency(
                accumulator.Word,
                accumulator.Count,
                accumulator.DocumentCount,
                PerMillion(accumulator.Count, totalWordCount),
                StopWordProvider.IsStopWord(accumulator.Word, languageCodes)))
            .OrderByDescending(word => word.Count)
            .ThenBy(word => word.Word, StringComparer.Ordinal)
            .ToArray();
    }

    private static IReadOnlyList<NGramFrequency> BuildNGramFrequencies(
        IReadOnlyList<DocumentAnalysis> documents,
        AnalysisSettings settings,
        int totalWordCount)
    {
        Dictionary<NGramKey, NGramAccumulator> accumulators = new();

        foreach (DocumentAnalysis document in documents)
        {
            HashSet<NGramKey> ngramsInDocument = new();

            foreach (SentenceTokens sentence in document.SentenceTokens)
            {
                IReadOnlyList<string> words = sentence.WordTokens
                    .Select(token => token.NormalizedText)
                    .ToArray();

                for (int n = settings.NGramMinN; n <= settings.NGramMaxN; n++)
                {
                    if (words.Count < n)
                    {
                        continue;
                    }

                    for (int i = 0; i <= words.Count - n; i++)
                    {
                        IReadOnlyList<string> ngramWords = words.Skip(i).Take(n).ToArray();
                        if (ngramWords.Any(word => word.Length < settings.MinWordLength))
                        {
                            continue;
                        }

                        string text = string.Join(' ', ngramWords);
                        NGramKey key = new(n, text);

                        if (!accumulators.TryGetValue(key, out NGramAccumulator? accumulator))
                        {
                            accumulator = new NGramAccumulator(key);
                            accumulators[key] = accumulator;
                        }

                        accumulator.Count++;
                        ngramsInDocument.Add(key);
                    }
                }
            }

            foreach (NGramKey key in ngramsInDocument)
            {
                accumulators[key].DocumentCount++;
            }
        }

        return accumulators.Values
            .Where(accumulator => accumulator.Count >= settings.MinNGramCount)
            .Select(accumulator => new NGramFrequency(
                accumulator.Key.N,
                accumulator.Key.Text,
                accumulator.Count,
                accumulator.DocumentCount,
                PerMillion(accumulator.Count, totalWordCount)))
            .OrderBy(ngram => ngram.N)
            .ThenByDescending(ngram => ngram.Count)
            .ThenBy(ngram => ngram.Text, StringComparer.Ordinal)
            .ToArray();
    }

    private static IReadOnlyList<NextWordFrequency> BuildNextWordFrequencies(
        IReadOnlyList<DocumentAnalysis> documents,
        AnalysisSettings settings,
        IReadOnlyList<WordFrequency> wordFrequencies)
    {
        HashSet<string> topWords = wordFrequencies
            .Take(settings.TopWordsForNextWordAnalysis)
            .Select(word => word.Word)
            .ToHashSet(StringComparer.Ordinal);

        Dictionary<string, int> transitionTotals = new(StringComparer.Ordinal);
        Dictionary<NextWordKey, int> pairCounts = new();

        foreach (DocumentAnalysis document in documents)
        {
            foreach (SentenceTokens sentence in document.SentenceTokens)
            {
                IReadOnlyList<string> words = sentence.WordTokens
                    .Select(token => token.NormalizedText)
                    .ToArray();

                for (int i = 0; i < words.Count - 1; i++)
                {
                    string word = words[i];
                    string nextWord = words[i + 1];

                    if (!topWords.Contains(word))
                    {
                        continue;
                    }

                    transitionTotals[word] = transitionTotals.GetValueOrDefault(word) + 1;
                    NextWordKey key = new(word, nextWord);
                    pairCounts[key] = pairCounts.GetValueOrDefault(key) + 1;
                }
            }
        }

        return pairCounts
            .Where(pair => pair.Value >= settings.MinNextWordPairCount)
            .Select(pair => new NextWordFrequency(
                pair.Key.Word,
                pair.Key.NextWord,
                pair.Value,
                pair.Value / (double)transitionTotals[pair.Key.Word]))
            .OrderBy(next => next.Word, StringComparer.Ordinal)
            .ThenByDescending(next => next.Count)
            .ThenBy(next => next.NextWord, StringComparer.Ordinal)
            .ToArray();
    }

    private static double PerMillion(int count, int totalWordCount)
    {
        return totalWordCount == 0 ? 0 : count * 1_000_000d / totalWordCount;
    }

    private sealed record SentenceTokens(TextSentence Sentence, IReadOnlyList<TextToken> Tokens)
    {
        public IReadOnlyList<TextToken> WordTokens { get; } = Tokens
            .Where(token => token.Kind == TokenKind.Word)
            .ToArray();
    }

    private sealed record DocumentAnalysis(
        TextDocument Document,
        IReadOnlyList<SentenceTokens> SentenceTokens,
        IReadOnlyList<AnalyzedSentence> Sentences)
    {
        public IReadOnlyList<TextToken> WordTokens { get; } = SentenceTokens
            .SelectMany(sentence => sentence.WordTokens)
            .ToArray();
    }

    private sealed record WordAccumulator(string Word)
    {
        public int Count { get; set; }

        public int DocumentCount { get; set; }
    }

    private sealed record NGramKey(int N, string Text);

    private sealed record NGramAccumulator(NGramKey Key)
    {
        public int Count { get; set; }

        public int DocumentCount { get; set; }
    }

    private sealed record NextWordKey(string Word, string NextWord);
}
