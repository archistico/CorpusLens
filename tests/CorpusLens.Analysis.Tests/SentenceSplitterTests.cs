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

    [Fact]
    public void Split_ShouldNotBreakOnEnglishTitlesBeforeNames()
    {
        SentenceSplitter splitter = new(new TextNormalizer());

        var sentences = splitter.Split("Mr. Rabbit went away. Mrs. Smith stayed.");

        Assert.Collection(
            sentences,
            sentence => Assert.Equal("Mr. Rabbit went away.", sentence.Text),
            sentence => Assert.Equal("Mrs. Smith stayed.", sentence.Text));
    }

    [Fact]
    public void Split_ShouldNotBreakOnDottedAbbreviations()
    {
        SentenceSplitter splitter = new(new TextNormalizer());

        var sentences = splitter.Split("This is useful, e.g. in examples. This is another sentence.");

        Assert.Collection(
            sentences,
            sentence => Assert.Equal("This is useful, e.g. in examples.", sentence.Text),
            sentence => Assert.Equal("This is another sentence.", sentence.Text));
    }

    [Fact]
    public void Split_ShouldKeepQuotedQuestionWithDialogueAttribution()
    {
        SentenceSplitter splitter = new(new TextNormalizer());

        var sentences = splitter.Split("\"Who are you?\" said the Caterpillar. Alice replied.");

        Assert.Collection(
            sentences,
            sentence => Assert.Equal("\"Who are you?\" said the Caterpillar.", sentence.Text),
            sentence => Assert.Equal("Alice replied.", sentence.Text));
    }

    [Fact]
    public void Split_ShouldKeepQuotedExclamationWithDialogueAttribution()
    {
        SentenceSplitter splitter = new(new TextNormalizer());

        var sentences = splitter.Split("\"Oh dear!\" cried Alice. The Rabbit ran away.");

        Assert.Collection(
            sentences,
            sentence => Assert.Equal("\"Oh dear!\" cried Alice.", sentence.Text),
            sentence => Assert.Equal("The Rabbit ran away.", sentence.Text));
    }

    [Fact]
    public void Split_ShouldSplitChapterTitleAsOwnSentence()
    {
        SentenceSplitter splitter = new(new TextNormalizer());

        var sentences = splitter.Split("CHAPTER I. Down the Rabbit-Hole. Alice was beginning to get very tired.");

        Assert.Collection(
            sentences,
            sentence => Assert.Equal("CHAPTER I.", sentence.Text),
            sentence => Assert.Equal("Down the Rabbit-Hole.", sentence.Text),
            sentence => Assert.Equal("Alice was beginning to get very tired.", sentence.Text));
    }

    [Fact]
    public void Split_ShouldNotBreakOnDecimalNumbers()
    {
        SentenceSplitter splitter = new(new TextNormalizer());

        var sentences = splitter.Split("The value is 3.14. That is enough.");

        Assert.Collection(
            sentences,
            sentence => Assert.Equal("The value is 3.14.", sentence.Text),
            sentence => Assert.Equal("That is enough.", sentence.Text));
    }
}
