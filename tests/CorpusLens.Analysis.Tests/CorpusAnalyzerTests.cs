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
