using CorpusLens.Analysis.Statistics;
using CorpusLens.Domain.Analysis;
using CorpusLens.Domain.Text;
using Xunit;

namespace CorpusLens.Analysis.Tests;

public sealed class CorpusAnalyzerTests
{
    private const string SampleText = """
        Hello, Tom.
        Hello, Anna.
        I don't know.
        I don't know what you mean.
        Do you know Anna?
        No, I don't.
        """;

    [Fact]
    public void Analyze_ShouldCalculateWordFrequencies()
    {
        CorpusAnalyzer analyzer = new();
        TextDocument document = new("sample", "Sample", "en", SampleText);

        CorpusAnalysisResult result = analyzer.Analyze(document, Settings());

        WordFrequency hello = Assert.Single(result.Words, word => word.Word == "hello");
        Assert.Equal(2, hello.Count);

        WordFrequency i = Assert.Single(result.Words, word => word.Word == "i");
        Assert.Equal(3, i.Count);
    }

    [Fact]
    public void Analyze_ShouldCalculateNGramsWithinSentences()
    {
        CorpusAnalyzer analyzer = new();
        TextDocument document = new("sample", "Sample", "en", SampleText);

        CorpusAnalysisResult result = analyzer.Analyze(document, Settings());

        NGramFrequency iDont = Assert.Single(result.NGrams, ngram => ngram.N == 2 && ngram.Text == "i don't");
        Assert.Equal(3, iDont.Count);

        NGramFrequency dontKnow = Assert.Single(result.NGrams, ngram => ngram.N == 2 && ngram.Text == "don't know");
        Assert.Equal(2, dontKnow.Count);
    }

    [Fact]
    public void Analyze_ShouldCalculateNextWords()
    {
        CorpusAnalyzer analyzer = new();
        TextDocument document = new("sample", "Sample", "en", SampleText);

        CorpusAnalysisResult result = analyzer.Analyze(document, Settings());

        NextWordFrequency nextWord = Assert.Single(result.NextWords, item => item.Word == "i" && item.NextWord == "don't");
        Assert.Equal(3, nextWord.Count);
        Assert.Equal(1.0, nextWord.Probability, precision: 4);
    }



    [Fact]
    public void Analyze_ShouldCalculateFrequenciesAndNGramsForEnglishContractionsAndPossessives()
    {
        CorpusAnalyzer analyzer = new();
        TextDocument document = new(
            "sample",
            "Sample",
            "en",
            "Alice’s sister said, \"I’m sure Alice's cat won't go.\" Alice's cat won't go.");

        CorpusAnalysisResult result = analyzer.Analyze(document, Settings());

        WordFrequency possessive = Assert.Single(result.Words, word => word.Word == "alice's");
        WordFrequency im = Assert.Single(result.Words, word => word.Word == "i'm");
        WordFrequency wont = Assert.Single(result.Words, word => word.Word == "won't");
        NGramFrequency aliceCat = Assert.Single(result.NGrams, ngram => ngram.N == 2 && ngram.Text == "alice's cat");
        NGramFrequency wontGo = Assert.Single(result.NGrams, ngram => ngram.N == 2 && ngram.Text == "won't go");

        Assert.Equal(3, possessive.Count);
        Assert.Equal(1, im.Count);
        Assert.Equal(2, wont.Count);
        Assert.Equal(2, aliceCat.Count);
        Assert.Equal(2, wontGo.Count);
        Assert.True(im.IsStopWord);
        Assert.True(wont.IsStopWord);
        Assert.False(possessive.IsStopWord);
    }


    [Fact]
    public void Analyze_ShouldClassifyStopWordsForDocumentLanguage()
    {
        CorpusAnalyzer analyzer = new();
        TextDocument document = new("sample", "Sample", "en", "The rabbit and Alice ran.");

        CorpusAnalysisResult result = analyzer.Analyze(document, Settings());

        WordFrequency the = Assert.Single(result.Words, word => word.Word == "the");
        WordFrequency rabbit = Assert.Single(result.Words, word => word.Word == "rabbit");

        Assert.True(the.IsStopWord);
        Assert.False(rabbit.IsStopWord);
    }

    [Fact]
    public void Analyze_ShouldUseEachDocumentForDocumentCount()
    {
        CorpusAnalyzer analyzer = new();
        TextDocument first = new("first", "First", "en", "Alice smiled. Alice laughed.");
        TextDocument second = new("second", "Second", "en", "Alice wondered. Rabbit ran.");

        CorpusAnalysisResult result = analyzer.Analyze(new[] { first, second }, Settings());

        Assert.Equal(2, result.Summary.DocumentCount);
        WordFrequency alice = Assert.Single(result.Words, word => word.Word == "alice");
        Assert.Equal(3, alice.Count);
        Assert.Equal(2, alice.DocumentCount);
    }

    private static AnalysisSettings Settings()
    {
        return new AnalysisSettings
        {
            NGramMinN = 2,
            NGramMaxN = 3,
            MinNGramCount = 2,
            MinNextWordPairCount = 2
        };
    }
}
