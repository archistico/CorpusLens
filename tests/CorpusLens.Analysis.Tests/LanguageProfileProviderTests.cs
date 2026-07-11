using CorpusLens.Analysis.Language;
using CorpusLens.Analysis.StopWords;
using Xunit;

namespace CorpusLens.Analysis.Tests;

public sealed class LanguageProfileProviderTests
{
    [Fact]
    public void ListProfiles_ShouldExposeSupportedLanguageProfiles()
    {
        IReadOnlyList<LanguageProfile> profiles = LanguageProfileProvider.ListProfiles();

        Assert.Contains(profiles, profile => profile.Code == "en");
        Assert.Contains(profiles, profile => profile.Code == "it");
        Assert.Contains(profiles, profile => profile.Code == "fr");
        Assert.Contains(profiles, profile => profile.Code == "de");
    }

    [Fact]
    public void GetProfile_ShouldNormalizeRegionalLanguageCodes()
    {
        LanguageProfile profile = LanguageProfileProvider.GetProfile("en-US");

        Assert.Equal("en", profile.Code);
        Assert.Equal("English", profile.Name);
        Assert.Equal(7, profile.DefaultLongWordLength);
        Assert.Equal(10, profile.DefaultVeryLongWordLength);
        Assert.True(profile.IsKnown);
    }

    [Fact]
    public void GetProfile_ShouldReturnItalianDifficultyThresholds()
    {
        LanguageProfile profile = LanguageProfileProvider.GetProfile("it");

        Assert.Equal(8, profile.DefaultLongWordLength);
        Assert.Equal(12, profile.DefaultVeryLongWordLength);
        Assert.InRange(profile.StopWordCount, 1, int.MaxValue);
    }

    [Fact]
    public void GetProfile_ShouldReturnGenericFallbackForUnknownLanguages()
    {
        LanguageProfile profile = LanguageProfileProvider.GetProfile("xx");

        Assert.Equal("xx", profile.Code);
        Assert.Equal(7, profile.DefaultLongWordLength);
        Assert.Equal(10, profile.DefaultVeryLongWordLength);
        Assert.False(profile.IsKnown);
    }

    [Fact]
    public void StopWordProvider_ShouldCountStopWordsByNormalizedLanguageCode()
    {
        Assert.Equal(
            StopWordProvider.CountStopWords("en"),
            StopWordProvider.CountStopWords("en-GB"));
    }
}
