using System.Net;
using System.Text;
using HtmlAgilityPack;

namespace CorpusLens.Infrastructure.Epub;

public sealed class HtmlToTextConverter
{
    private static readonly HashSet<string> BlockElementNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "address", "article", "aside", "blockquote", "body", "dd", "div", "dl", "dt", "figcaption",
        "figure", "footer", "h1", "h2", "h3", "h4", "h5", "h6", "header", "hr", "li", "main", "nav",
        "ol", "p", "pre", "section", "table", "tbody", "td", "tfoot", "th", "thead", "tr", "ul"
    };

    public string Convert(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return string.Empty;
        }

        HtmlDocument document = new();
        document.LoadHtml(html);

        RemoveNodes(document, "//script|//style|//noscript");

        HtmlNode root = document.DocumentNode.SelectSingleNode("//body") ?? document.DocumentNode;
        StringBuilder builder = new();
        AppendVisibleText(root, builder);

        return NormalizeExtractedText(builder.ToString());
    }

    private static void AppendVisibleText(HtmlNode node, StringBuilder builder)
    {
        if (node.NodeType == HtmlNodeType.Text)
        {
            AppendText(builder, WebUtility.HtmlDecode(node.InnerText));
            return;
        }

        if (node.NodeType != HtmlNodeType.Element && node.NodeType != HtmlNodeType.Document)
        {
            return;
        }

        bool isBlock = node.NodeType == HtmlNodeType.Element && BlockElementNames.Contains(node.Name);
        bool isLineBreak = node.NodeType == HtmlNodeType.Element && string.Equals(node.Name, "br", StringComparison.OrdinalIgnoreCase);

        if (isLineBreak)
        {
            AppendLineBreak(builder);
            return;
        }

        if (isBlock)
        {
            AppendLineBreak(builder);
        }

        foreach (HtmlNode child in node.ChildNodes)
        {
            AppendVisibleText(child, builder);
        }

        if (isBlock)
        {
            AppendLineBreak(builder);
        }
    }

    private static void AppendText(StringBuilder builder, string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        foreach (char character in text)
        {
            if (char.IsWhiteSpace(character))
            {
                if (builder.Length > 0 && !char.IsWhiteSpace(builder[^1]))
                {
                    builder.Append(' ');
                }

                continue;
            }

            builder.Append(character);
        }
    }

    private static void AppendLineBreak(StringBuilder builder)
    {
        if (builder.Length == 0)
        {
            return;
        }

        while (builder.Length > 0 && builder[^1] == ' ')
        {
            builder.Length--;
        }

        if (builder.Length > 0 && builder[^1] != '\n')
        {
            builder.AppendLine();
        }
    }

    private static void RemoveNodes(HtmlDocument document, string xpath)
    {
        HtmlNodeCollection? nodes = document.DocumentNode.SelectNodes(xpath);
        if (nodes is null)
        {
            return;
        }

        foreach (HtmlNode node in nodes.Cast<HtmlNode>().ToArray())
        {
            node.Remove();
        }
    }

    private static string NormalizeExtractedText(string text)
    {
        string[] lines = text
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n')
            .Split('\n')
            .Select(line => CollapseWhitespace(line).Trim())
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToArray();

        return string.Join(Environment.NewLine, lines);
    }

    private static string CollapseWhitespace(string value)
    {
        StringBuilder builder = new(value.Length);
        bool previousWasWhiteSpace = false;

        foreach (char character in value)
        {
            if (char.IsWhiteSpace(character))
            {
                if (!previousWasWhiteSpace)
                {
                    builder.Append(' ');
                    previousWasWhiteSpace = true;
                }

                continue;
            }

            builder.Append(character);
            previousWasWhiteSpace = false;
        }

        return builder.ToString();
    }
}
