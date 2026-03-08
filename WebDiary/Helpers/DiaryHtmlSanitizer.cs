using HtmlAgilityPack;

namespace WebDiary.Helpers;

public static class DiaryHtmlSanitizer
{
    private static readonly HashSet<string> AllowedTags = new(StringComparer.OrdinalIgnoreCase)
    {
        "p", "br", "strong", "b", "em", "i", "u", "s", "strike",
        "ul", "ol", "li", "h1", "h2", "h3", "blockquote", "span", "div"
    };

    private static readonly HashSet<string> AllowedAttributes = new(StringComparer.OrdinalIgnoreCase)
    {
        "class",
        "data-list"
    };

    public static string Sanitize(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return string.Empty;
        }

        var document = new HtmlDocument();
        document.LoadHtml(html);
        SanitizeNode(document.DocumentNode);
        return document.DocumentNode.InnerHtml;
    }

    private static void SanitizeNode(HtmlNode node)
    {
        foreach (var child in node.ChildNodes.ToList())
        {
            SanitizeNode(child);
        }

        if (node.NodeType != HtmlNodeType.Element)
        {
            return;
        }

        if (!AllowedTags.Contains(node.Name))
        {
            if (node.Name.Equals("script", StringComparison.OrdinalIgnoreCase) ||
                node.Name.Equals("style", StringComparison.OrdinalIgnoreCase))
            {
                node.Remove();
                return;
            }

            var parent = node.ParentNode;
            if (parent != null)
            {
                foreach (var child in node.ChildNodes.ToList())
                {
                    parent.InsertBefore(child, node);
                }
            }

            node.Remove();
            return;
        }

        foreach (var attribute in node.Attributes.ToList())
        {
            if (attribute.Name.StartsWith("on", StringComparison.OrdinalIgnoreCase) ||
                !AllowedAttributes.Contains(attribute.Name))
            {
                node.Attributes.Remove(attribute);
            }
        }
    }
}
