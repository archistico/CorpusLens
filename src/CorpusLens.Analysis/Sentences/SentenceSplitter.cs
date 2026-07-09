using CorpusLens.Analysis.Normalization;
using CorpusLens.Domain.Text;

namespace CorpusLens.Analysis.Sentences;

public sealed class SentenceSplitter
{
    private static readonly HashSet<string> KnownAbbreviations = new(StringComparer.OrdinalIgnoreCase)
    {
        "Mr", "Mrs", "Ms", "Dr", "Prof", "Sr", "Jr", "St", "vs", "etc", "e.g", "i.e"
    };

    private readonly TextNormalizer _normalizer;

    public SentenceSplitter(TextNormalizer normalizer)
    {
        _normalizer = normalizer;
    }

    public IReadOnlyList<TextSentence> Split(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        string normalizedText = _normalizer.NormalizeForReading(text);
        List<TextSentence> sentences = new();

        int sentenceStart = 0;
        int index = 0;

        for (int i = 0; i < normalizedText.Length; i++)
        {
            char current = normalizedText[i];

            if (!IsSentenceTerminator(current))
            {
                continue;
            }

            if (current == '.' && IsPartOfKnownAbbreviation(normalizedText, i))
            {
                continue;
            }

            int sentenceEnd = IncludeTrailingQuotesAndBrackets(normalizedText, i);
            AddSentence(sentences, normalizedText, sentenceStart, sentenceEnd, index);
            index++;

            sentenceStart = sentenceEnd + 1;
            while (sentenceStart < normalizedText.Length && char.IsWhiteSpace(normalizedText[sentenceStart]))
            {
                sentenceStart++;
            }

            i = sentenceStart - 1;
        }

        if (sentenceStart < normalizedText.Length)
        {
            AddSentence(sentences, normalizedText, sentenceStart, normalizedText.Length - 1, index);
        }

        return sentences;
    }

    private static bool IsSentenceTerminator(char value)
    {
        return value is '.' or '?' or '!';
    }

    private static int IncludeTrailingQuotesAndBrackets(string text, int terminatorIndex)
    {
        int index = terminatorIndex;

        while (index + 1 < text.Length && text[index + 1] is '"' or '\'' or ')' or ']' or '}')
        {
            index++;
        }

        return index;
    }

    private static void AddSentence(
        List<TextSentence> sentences,
        string fullText,
        int startOffset,
        int endOffset,
        int index)
    {
        if (endOffset < startOffset)
        {
            return;
        }

        string sentenceText = fullText[startOffset..(endOffset + 1)].Trim();
        if (sentenceText.Length == 0)
        {
            return;
        }

        int leadingWhitespace = fullText[startOffset..(endOffset + 1)].Length - fullText[startOffset..(endOffset + 1)].TrimStart().Length;
        int actualStart = startOffset + leadingWhitespace;
        string normalized = sentenceText.ToLowerInvariant();

        sentences.Add(new TextSentence(index, sentenceText, normalized, actualStart, endOffset));
    }

    private static bool IsPartOfKnownAbbreviation(string text, int dotIndex)
    {
        int start = dotIndex - 1;
        while (start >= 0 && (char.IsLetter(text[start]) || text[start] == '.'))
        {
            start--;
        }

        string candidate = text[(start + 1)..dotIndex];
        return KnownAbbreviations.Contains(candidate);
    }
}
