using CorpusLens.Domain.Analysis;
using CorpusLens.Domain.Text;

namespace CorpusLens.Analysis.Classification;

public sealed class SimplePhraseClassifier
{
    private static readonly HashSet<string> Greetings = new(StringComparer.OrdinalIgnoreCase)
    {
        "hello", "hi", "hey", "good morning", "good evening", "good afternoon",
        "ciao", "buongiorno", "buonasera", "salve",
        "hallo", "guten morgen", "guten abend",
        "bonjour", "salut", "bonsoir"
    };

    private static readonly HashSet<string> Negations = new(StringComparer.OrdinalIgnoreCase)
    {
        "not", "don't", "doesn't", "didn't", "never", "no",
        "non", "mai", "nessuno", "niente",
        "nicht", "kein", "keine", "niemals",
        "ne", "pas", "jamais", "rien"
    };

    private static readonly HashSet<string> RequestMarkers = new(StringComparer.OrdinalIgnoreCase)
    {
        "please", "could", "would", "can", "puoi", "potresti", "favore", "bitte", "pouvez", "peux"
    };

    public PhraseCategory Classify(TextSentence sentence, IReadOnlyList<TextToken> tokens)
    {
        ArgumentNullException.ThrowIfNull(sentence);
        ArgumentNullException.ThrowIfNull(tokens);

        char? terminalMark = GetTerminalSentenceMark(sentence.Text);
        if (terminalMark == '?')
        {
            return PhraseCategory.Question;
        }

        if (terminalMark == '!')
        {
            return PhraseCategory.Exclamation;
        }

        IReadOnlyList<string> words = tokens
            .Where(token => token.Kind == TokenKind.Word)
            .Select(token => token.NormalizedText)
            .ToArray();

        if (ContainsGreeting(words))
        {
            return PhraseCategory.Greeting;
        }

        if (words.Any(word => Negations.Contains(word)))
        {
            return PhraseCategory.Negation;
        }

        if (words.Any(word => RequestMarkers.Contains(word)))
        {
            return PhraseCategory.Request;
        }

        return words.Count > 0 ? PhraseCategory.Statement : PhraseCategory.Other;
    }

    private static char? GetTerminalSentenceMark(string text)
    {
        string trimmed = TrimTrailingClosingPunctuation(text);
        if (trimmed.EndsWith("?", StringComparison.Ordinal))
        {
            return '?';
        }

        if (trimmed.EndsWith("!", StringComparison.Ordinal))
        {
            return '!';
        }

        return GetLeadingQuotedSpeechTerminalMark(text);
    }

    private static char? GetLeadingQuotedSpeechTerminalMark(string text)
    {
        string trimmed = text.TrimStart();
        if (trimmed.Length < 3 || !IsOpeningQuote(trimmed[0]))
        {
            return null;
        }

        char? mark = null;
        for (int index = 1; index < trimmed.Length; index++)
        {
            char current = trimmed[index];
            if (current == '?' || current == '!')
            {
                mark = current;
                continue;
            }

            if (mark is not null && IsClosingQuote(current))
            {
                return mark;
            }
        }

        return null;
    }

    private static string TrimTrailingClosingPunctuation(string text)
    {
        return text.Trim().TrimEnd('"', '\'', '”', '’', '»', ')', ']', '}');
    }

    private static bool IsOpeningQuote(char value)
    {
        return value is '"' or '\'' or '“' or '‘' or '«';
    }

    private static bool IsClosingQuote(char value)
    {
        return value is '"' or '\'' or '”' or '’' or '»';
    }

    private static bool ContainsGreeting(IReadOnlyList<string> words)
    {
        if (words.Count == 0)
        {
            return false;
        }

        if (Greetings.Contains(words[0]))
        {
            return true;
        }

        if (words.Count >= 2 && Greetings.Contains($"{words[0]} {words[1]}"))
        {
            return true;
        }

        return false;
    }
}
