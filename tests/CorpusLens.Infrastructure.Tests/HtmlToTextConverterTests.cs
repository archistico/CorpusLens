using CorpusLens.Infrastructure.Epub;
using Xunit;

namespace CorpusLens.Infrastructure.Tests;

public sealed class HtmlToTextConverterTests
{
    [Fact]
    public void Convert_ShouldExtractVisibleText()
    {
        HtmlToTextConverter converter = new();

        string text = converter.Convert("""
            <html>
              <body>
                <h1>Chapter 1</h1>
                <p>Hello, <strong>Tom</strong>.</p>
                <p>I don&apos;t know.</p>
              </body>
            </html>
            """);

        Assert.Contains("Chapter 1", text);
        Assert.Contains("Hello, Tom", text);
        Assert.Contains("I don't know.", text);
    }

    [Fact]
    public void Convert_ShouldIgnoreScriptAndStyleContent()
    {
        HtmlToTextConverter converter = new();

        string text = converter.Convert("""
            <html>
              <head>
                <style>.x { color: red; }</style>
                <script>const hidden = true;</script>
              </head>
              <body>
                <p>Visible text.</p>
              </body>
            </html>
            """);

        Assert.Equal("Visible text.", text);
    }

    [Fact]
    public void Convert_ShouldCollapseRepeatedWhitespace()
    {
        HtmlToTextConverter converter = new();

        string text = converter.Convert("<p>Hello,    Tom.</p><p>Good&nbsp;&nbsp;morning.</p>");

        Assert.Contains("Hello, Tom.", text);
        Assert.Contains("Good morning.", text);
    }
}
