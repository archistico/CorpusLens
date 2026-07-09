using CorpusLens.Analysis.Normalization;
using CorpusLens.Domain.Text;

namespace CorpusLens.Analysis.Sentences;

public sealed class SentenceSplitter
{
    private static readonly HashSet<string> KnownAbbreviations = new(StringComparer.OrdinalIgnoreCase)
    {
        "mr.", "mrs.", "ms.", "dr.", "prof.", "sr.", "jr.", "st.", "vs.", "etc.",
        "e.g.", "i.e.", "u.s.", "u.k."
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

            if (ShouldIgnoreTerminator(normalizedText, i))
            {
                continue;
            }

            int sentenceEnd = IncludeTrailingQuotesAndBrackets(normalizedText, i);
            sentenceEnd = TryExtendQuotedDialogueAttribution(normalizedText, i, sentenceEnd);

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

    private static bool ShouldIgnoreTerminator(string text, int terminatorIndex)
    {
        char current = text[terminatorIndex];

        if (current == '.' && IsDecimalPoint(text, terminatorIndex))
        {
            return true;
        }

        if (current == '.' && IsEllipsisDotBeforeLast(text, terminatorIndex))
        {
            return true;
        }

        if (current == '.' && IsPartOfKnownAbbreviation(text, terminatorIndex))
        {
            return true;
        }

        return false;
    }

    private static bool IsDecimalPoint(string text, int dotIndex)
    {
        return dotIndex > 0
            && dotIndex + 1 < text.Length
            && char.IsDigit(text[dotIndex - 1])
            && char.IsDigit(text[dotIndex + 1]);
    }

    private static bool IsEllipsisDotBeforeLast(string text, int dotIndex)
    {
        return dotIndex + 1 < text.Length && text[dotIndex + 1] == '.';
    }

    private static int IncludeTrailingQuotesAndBrackets(string text, int terminatorIndex)
    {
        int index = terminatorIndex;

        while (index + 1 < text.Length && IsClosingQuoteOrBracket(text[index + 1]))
        {
            index++;
        }

        return index;
    }

    private static bool IsClosingQuoteOrBracket(char value)
    {
        return value is '"' or '\'' or '”' or '’' or '»' or ')' or ']' or '}';
    }

    private static int TryExtendQuotedDialogueAttribution(string text, int terminatorIndex, int sentenceEnd)
    {
        if (sentenceEnd == terminatorIndex)
        {
            return sentenceEnd;
        }

        if (!IsClosingQuoteOrBracket(text[sentenceEnd]))
        {
            return sentenceEnd;
        }

        int nextIndex = sentenceEnd + 1;
        while (nextIndex < text.Length && (char.IsWhiteSpace(text[nextIndex]) || text[nextIndex] is ',' or ';'))
        {
            nextIndex++;
        }

        if (nextIndex >= text.Length || !char.IsLower(text[nextIndex]))
        {
            return sentenceEnd;
        }

        for (int i = nextIndex; i < text.Length; i++)
        {
            if (!IsSentenceTerminator(text[i]))
            {
                continue;
            }

            if (ShouldIgnoreTerminator(text, i))
            {
                continue;
            }

            return IncludeTrailingQuotesAndBrackets(text, i);
        }

        return sentenceEnd;
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

        string segment = fullText[startOffset..(endOffset + 1)];
        string sentenceText = segment.Trim();
        if (sentenceText.Length == 0)
        {
            return;
        }

        int leadingWhitespace = segment.Length - segment.TrimStart().Length;
        int actualStart = startOffset + leadingWhitespace;
        string normalized = sentenceText.ToLowerInvariant();

        sentences.Add(new TextSentence(index, sentenceText, normalized, actualStart, endOffset));
    }

    private static bool IsPartOfKnownAbbreviation(string text, int dotIndex)
    {
        string candidate = ExtractDottedToken(text, dotIndex);
        return KnownAbbreviations.Contains(candidate);
    }

    private static string ExtractDottedToken(string text, int dotIndex)
    {
        int start = dotIndex;
        while (start > 0 && IsDottedTokenCharacter(text[start - 1]))
        {
            start--;
        }

        int end = dotIndex;
        while (end + 1 < text.Length && IsDottedTokenCharacter(text[end + 1]))
        {
            end++;
        }

        return text[start..(end + 1)];
    }

    private static bool IsDottedTokenCharacter(char value)
    {
        return char.IsLetter(value) || value == '.';
    }
}
