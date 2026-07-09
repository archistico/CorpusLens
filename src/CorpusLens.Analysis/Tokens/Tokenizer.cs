using System.Text.RegularExpressions;
using CorpusLens.Domain.Text;

namespace CorpusLens.Analysis.Tokens;

public sealed class Tokenizer
{
    private static readonly Regex TokenRegex = new(
        @"(?<word>[\p{L}\p{M}]+(?:['\-][\p{L}\p{M}]+)*)|(?<number>\p{N}+(?:[\.,]\p{N}+)*)|(?<punct>[\p{P}])|(?<symbol>[\p{S}])|(?<other>\S)",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public IReadOnlyList<TextToken> Tokenize(TextSentence sentence)
    {
        ArgumentNullException.ThrowIfNull(sentence);

        List<TextToken> tokens = new();
        MatchCollection matches = TokenRegex.Matches(sentence.Text);

        for (int i = 0; i < matches.Count; i++)
        {
            Match match = matches[i];
            TokenKind kind = DetermineKind(match);
            string text = match.Value;
            string normalized = NormalizeToken(text, kind);

            tokens.Add(new TextToken(
                sentence.Index,
                i,
                text,
                normalized,
                kind,
                sentence.StartOffset + match.Index,
                sentence.StartOffset + match.Index + match.Length));
        }

        return tokens;
    }

    private static TokenKind DetermineKind(Match match)
    {
        if (match.Groups["word"].Success)
        {
            return TokenKind.Word;
        }

        if (match.Groups["number"].Success)
        {
            return TokenKind.Number;
        }

        if (match.Groups["punct"].Success)
        {
            return TokenKind.Punctuation;
        }

        if (match.Groups["symbol"].Success)
        {
            return TokenKind.Symbol;
        }

        return TokenKind.Other;
    }

    private static string NormalizeToken(string text, TokenKind kind)
    {
        string normalized = text
            .Replace('’', '\'')
            .Replace('‘', '\'')
            .Replace('‐', '-')
            .Replace('‑', '-')
            .Replace('–', '-')
            .Replace('—', '-');

        return kind == TokenKind.Word
            ? normalized.ToLowerInvariant()
            : normalized;
    }
}
