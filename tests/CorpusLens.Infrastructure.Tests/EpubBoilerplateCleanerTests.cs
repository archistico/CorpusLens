using CorpusLens.Infrastructure.Epub;
using Xunit;

namespace CorpusLens.Infrastructure.Tests;

public sealed class EpubBoilerplateCleanerTests
{
    [Fact]
    public void Clean_ShouldRemoveProjectGutenbergHeaderAndFooter()
    {
        const string text = """
            The Project Gutenberg eBook of Sample
            *** START OF THE PROJECT GUTENBERG EBOOK SAMPLE ***
            Real book text.
            *** END OF THE PROJECT GUTENBERG EBOOK SAMPLE ***
            License text.
            """;

        EpubBoilerplateCleaner cleaner = new();

        string cleaned = cleaner.Clean(text);

        Assert.Contains("Real book text.", cleaned, StringComparison.Ordinal);
        Assert.DoesNotContain("Project Gutenberg", cleaned, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("License text", cleaned, StringComparison.Ordinal);
    }

    [Fact]
    public void Clean_ShouldRemoveLeadingDuplicatedChapterList()
    {
        const string text = """
            Alice's Adventures in Wonderland
            by Lewis Carroll
            Contents
            CHAPTER I.
            Down the Rabbit-Hole
            CHAPTER II.
            The Pool of Tears

            CHAPTER I.
            Down the Rabbit-Hole
            Alice was beginning to get very tired.
            """;

        EpubBoilerplateCleaner cleaner = new();

        string cleaned = cleaner.Clean(text);

        Assert.StartsWith("CHAPTER I.", cleaned, StringComparison.Ordinal);
        Assert.Contains("Alice was beginning", cleaned, StringComparison.Ordinal);
        Assert.DoesNotContain("Contents", cleaned, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("by Lewis Carroll", cleaned, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Clean_ShouldNotRemoveSingleChapterAtStart()
    {
        const string text = """
            CHAPTER I.
            Down the Rabbit-Hole
            Alice was beginning to get very tired.
            """;

        EpubBoilerplateCleaner cleaner = new();

        string cleaned = cleaner.Clean(text);

        Assert.StartsWith("CHAPTER I.", cleaned, StringComparison.Ordinal);
        Assert.Contains("Alice was beginning", cleaned, StringComparison.Ordinal);
    }

    [Fact]
    public void IsLikelyFrontMatterOnly_ShouldDetectStandaloneTableOfContents()
    {
        const string text = """
            Contents
            CHAPTER I.
            Down the Rabbit-Hole
            CHAPTER II.
            The Pool of Tears
            CHAPTER III.
            A Caucus-Race and a Long Tale
            """;

        EpubBoilerplateCleaner cleaner = new();

        Assert.True(cleaner.IsLikelyFrontMatterOnly(text));
    }


    [Fact]
    public void IsLikelyFrontMatterOnly_ShouldDetectTableOfContentsAfterTitleLines()
    {
        const string text = """
            Alice's Adventures in Wonderland
            by Lewis Carroll
            THE MILLENNIUM FULCRUM EDITION 3.0
            Contents
            CHAPTER I.
            Down the Rabbit-Hole
            CHAPTER II.
            The Pool of Tears
            CHAPTER III.
            A Caucus-Race and a Long Tale
            CHAPTER IV.
            The Rabbit Sends in a Little Bill
            """;

        EpubBoilerplateCleaner cleaner = new();

        Assert.True(cleaner.IsLikelyFrontMatterOnly(text));
    }

    [Fact]
    public void IsLikelyFrontMatterOnly_ShouldNotDetectChapterWithProse()
    {
        const string text = """
            Contents
            CHAPTER I.
            Alice was beginning to get very tired of sitting by her sister on the bank.
            """;

        EpubBoilerplateCleaner cleaner = new();

        Assert.False(cleaner.IsLikelyFrontMatterOnly(text));
    }
}
