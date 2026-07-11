using CorpusLens.Analysis.Classification;
using CorpusLens.Analysis.Tokens;
using CorpusLens.Domain.Analysis;
using CorpusLens.Domain.Text;
using Xunit;

namespace CorpusLens.Analysis.Tests;

public sealed class SimplePhraseClassifierTests
{
    private readonly SimplePhraseClassifier _classifier = new();
    private readonly Tokenizer _tokenizer = new();

    [Theory]
    [InlineData("Hello, Tom.", PhraseCategory.Greeting)]
    [InlineData("Do you know Anna?", PhraseCategory.Question)]
    [InlineData("No, I don't.", PhraseCategory.Negation)]
    [InlineData("Could you help me, please?", PhraseCategory.Question)]
    [InlineData("That is great!", PhraseCategory.Exclamation)]
    [InlineData("Do you know Anna?\"", PhraseCategory.Question)]
    [InlineData("That is great!\"", PhraseCategory.Exclamation)]
    [InlineData("\"Who are you?\" said the Caterpillar.", PhraseCategory.Question)]
    [InlineData("\"Oh dear!\" cried Alice.", PhraseCategory.Exclamation)]
    [InlineData("He said \"Why?\" and left.", PhraseCategory.Statement)]
    [InlineData("She shouted \"Stop!\" and ran.", PhraseCategory.Statement)]
    [InlineData("\"What?!\" cried Alice.", PhraseCategory.Exclamation)]
    public void Classify_ShouldReturnExpectedCategory(string text, PhraseCategory expected)
    {
        TextSentence sentence = new(0, text, text.ToLowerInvariant(), 0, text.Length - 1);
        var tokens = _tokenizer.Tokenize(sentence);

        PhraseCategory result = _classifier.Classify(sentence, tokens);

        Assert.Equal(expected, result);
    }
}
