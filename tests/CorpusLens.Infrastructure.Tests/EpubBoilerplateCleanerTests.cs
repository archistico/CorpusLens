using CorpusLens.Infrastructure.Epub;
using Xunit;

namespace CorpusLens.Infrastructure.Tests;

public sealed class EpubBoilerplateCleanerTests
{
    [Fact]
    public void Clean_ShouldRemoveProjectGutenbergHeaderAndFooter()
    {
        EpubBoilerplateCleaner cleaner = new();

        string text = """
            The Project Gutenberg eBook of Example
            License header text.
            *** START OF THE PROJECT GUTENBERG EBOOK EXAMPLE ***
            Chapter I.
            This is the real book text.
            *** END OF THE PROJECT GUTENBERG EBOOK EXAMPLE ***
            License footer text.
            """;

        string cleaned = cleaner.Clean(text);

        Assert.DoesNotContain("Project Gutenberg", cleaned, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("License header", cleaned, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("License footer", cleaned, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("This is the real book text.", cleaned);
    }

    [Fact]
    public void Clean_ShouldLeaveNormalTextUnchangedApartFromTrim()
    {
        EpubBoilerplateCleaner cleaner = new();

        string cleaned = cleaner.Clean("  Chapter I.\nAlice was here.  ");

        Assert.Equal("Chapter I.\nAlice was here.", cleaned);
    }
}
