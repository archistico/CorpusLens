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

    private static void AssertToken(TextToken token, string text, string normalized, TokenKind kind)
    {
        Assert.Equal(text, token.Text);
        Assert.Equal(normalized, token.NormalizedText);
        Assert.Equal(kind, token.Kind);
    }
}
