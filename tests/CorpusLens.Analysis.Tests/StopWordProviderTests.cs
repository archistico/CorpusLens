using CorpusLens.Analysis.StopWords;
using Xunit;

namespace CorpusLens.Analysis.Tests;

public sealed class StopWordProviderTests
{
    [Theory]
    [InlineData("i")]
    [InlineData("è")]
    [InlineData("me")]
    [InlineData("egli")]
    [InlineData("quel")]
    [InlineData("fu")]
    public void IsStopWord_ShouldRecognizeCommonItalianFunctionWords(string word)
    {
        Assert.True(StopWordProvider.IsStopWord(word, "it"));
    }

    [Theory]
    [InlineData("disse")]
    [InlineData("casa")]
    [InlineData("vita")]
    public void IsStopWord_ShouldNotRecognizeItalianContentWords(string word)
    {
        Assert.False(StopWordProvider.IsStopWord(word, "it"));
    }
}
