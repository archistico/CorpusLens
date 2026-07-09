using CorpusLens.Analysis.Normalization;
using Xunit;

namespace CorpusLens.Analysis.Tests;

public sealed class TextNormalizerTests
{
    [Fact]
    public void NormalizeForReading_ShouldNormalizeWhitespaceAndQuotes()
    {
        TextNormalizer normalizer = new();

        string result = normalizer.NormalizeForReading("  Hello\r\n\r\n\r\n“Tom”\u00A0said: ‘hi’.  ");

        Assert.Equal("Hello\n\n\"Tom\" said: 'hi'.", result);
    }
}
