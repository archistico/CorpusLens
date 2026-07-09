using CorpusLens.Analysis.Tokens;
using CorpusLens.Domain.Text;
using Xunit;

namespace CorpusLens.Analysis.Tests;

public sealed class TokenizerTests
{
    [Fact]
    public void Tokenize_ShouldKeepContractionsAsSingleWord()
    {
        Tokenizer tokenizer = new();
        TextSentence sentence = new(0, "I don't know.", "i don't know.", 0, 12);

        var tokens = tokenizer.Tokenize(sentence);

        Assert.Collection(
            tokens,
            token => AssertToken(token, "I", "i", TokenKind.Word),
            token => AssertToken(token, "don't", "don't", TokenKind.Word),
            token => AssertToken(token, "know", "know", TokenKind.Word),
            token => AssertToken(token, ".", ".", TokenKind.Punctuation));
    }

    [Theory]
    [InlineData("I'm sure you'll see.", "I'm", "i'm", "you'll", "you'll")]
    [InlineData("He couldn't believe it.", "couldn't", "couldn't", "it", "it")]
    [InlineData("They won't stop.", "won't", "won't", "stop", "stop")]
    public void Tokenize_ShouldKeepEnglishContractionsAsSingleWords(
        string text,
        string firstExpectedText,
        string firstExpectedNormalized,
        string secondExpectedText,
        string secondExpectedNormalized)
    {
        Tokenizer tokenizer = new();
        TextSentence sentence = new(0, text, text.ToLowerInvariant(), 0, text.Length);

        var words = tokenizer.Tokenize(sentence)
            .Where(token => token.Kind == TokenKind.Word)
            .ToArray();

        Assert.Contains(words, token => token.Text == firstExpectedText && token.NormalizedText == firstExpectedNormalized);
        Assert.Contains(words, token => token.Text == secondExpectedText && token.NormalizedText == secondExpectedNormalized);
    }

    [Fact]
    public void Tokenize_ShouldKeepEnglishPossessivesAsSingleWords()
    {
        Tokenizer tokenizer = new();
        TextSentence sentence = new(0, "Alice's sister heard the Queen's voice.", "alice's sister heard the queen's voice.", 0, 39);

        var words = tokenizer.Tokenize(sentence)
            .Where(token => token.Kind == TokenKind.Word)
            .Select(token => token.NormalizedText)
            .ToArray();

        Assert.Contains("alice's", words);
        Assert.Contains("queen's", words);
        Assert.DoesNotContain("s", words);
    }

    [Fact]
    public void Tokenize_ShouldNormalizeTypographicApostrophesInsideWords()
    {
        Tokenizer tokenizer = new();
        TextSentence sentence = new(0, "I’m sure Alice’s sister won’t go.", "i'm sure alice's sister won't go.", 0, 33);

        var words = tokenizer.Tokenize(sentence)
            .Where(token => token.Kind == TokenKind.Word)
            .Select(token => token.NormalizedText)
            .ToArray();

        Assert.Contains("i'm", words);
        Assert.Contains("alice's", words);
        Assert.Contains("won't", words);
    }

    [Fact]
    public void Tokenize_ShouldKeepHyphenatedWordsAsSingleWords()
    {
        Tokenizer tokenizer = new();
        TextSentence sentence = new(0, "A well-known rabbit ran away.", "a well-known rabbit ran away.", 0, 29);

        var words = tokenizer.Tokenize(sentence)
            .Where(token => token.Kind == TokenKind.Word)
            .Select(token => token.NormalizedText)
            .ToArray();

        Assert.Contains("well-known", words);
    }

    private static void AssertToken(TextToken token, string text, string normalized, TokenKind kind)
    {
        Assert.Equal(text, token.Text);
        Assert.Equal(normalized, token.NormalizedText);
        Assert.Equal(kind, token.Kind);
    }
}
