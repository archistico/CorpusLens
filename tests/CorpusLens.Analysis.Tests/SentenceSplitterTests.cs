using CorpusLens.Analysis.Normalization;
using CorpusLens.Analysis.Sentences;
using Xunit;

namespace CorpusLens.Analysis.Tests;

public sealed class SentenceSplitterTests
{
    [Fact]
    public void Split_ShouldReturnExpectedSentences()
    {
        SentenceSplitter splitter = new(new TextNormalizer());

        var sentences = splitter.Split("Hello, Tom. I don't know. Do you know Anna? No, I don't.");

        Assert.Collection(
            sentences,
            sentence => Assert.Equal("Hello, Tom.", sentence.Text),
            sentence => Assert.Equal("I don't know.", sentence.Text),
            sentence => Assert.Equal("Do you know Anna?", sentence.Text),
            sentence => Assert.Equal("No, I don't.", sentence.Text));
    }

    [Fact]
    public void Split_ShouldNotBreakOnKnownAbbreviation()
    {
        SentenceSplitter splitter = new(new TextNormalizer());

        var sentences = splitter.Split("Dr. Smith is here. Hello!");

        Assert.Collection(
            sentences,
            sentence => Assert.Equal("Dr. Smith is here.", sentence.Text),
            sentence => Assert.Equal("Hello!", sentence.Text));
    }
}
