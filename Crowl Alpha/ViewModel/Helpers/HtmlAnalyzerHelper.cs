using HtmlAgilityPack;
using System.Linq;

namespace Crowl_Alpha.Model
{
    public static class HtmlAnalyzerHelper
    {
        private static readonly string[] InlineElements = { "a", "strong", "em", "span", "b", "i", "u", "small", "abbr", "cite", "code", "sub", "sup", "mark", "q", "s" };

        private static readonly string[] BlockElements = { "div", "p", "h1", "h2", "h3", "h4", "h5", "h6", "section", "article", "aside", "header", "footer", "blockquote", "label", "option" };

        private static readonly string[] IgnoredElements = { "script", "style" };

        // Analyzes the entire HTML document and returns the root HtmlNodeInfo
        public static HtmlNodeInfo AnalyzeHtml(HtmlDocument doc)
        {
            if (doc.DocumentNode == null)
                return null;

            return AnalyzeNode(doc.DocumentNode, null);
        }

        // Analyzes individual HTML nodes and builds the tree structure
        public static HtmlNodeInfo AnalyzeNode(HtmlNode node, HtmlNodeInfo parent)
        {
            // Skip ignored elements and text nodes that are purely whitespace
            if (IgnoredElements.Contains(node.Name.ToLower()) ||
               (node.NodeType == HtmlNodeType.Text && string.IsNullOrWhiteSpace(node.InnerText)))
            {
                return null;
            }

            // Initialize a new HtmlNodeInfo object
            var nodeInfo = new HtmlNodeInfo
            {
                NodeType = node.NodeType == HtmlNodeType.Element ? "Element" : "Text",
                Name = node.Name,
                DataUid= node.GetAttributeValue("data-uid",null),
                InnerText = node.NodeType == HtmlNodeType.Text ? node.InnerText.Trim() : string.Empty,
                Parent = parent
            };

            // Store attributes if any (only for element nodes)
            if (node.NodeType == HtmlNodeType.Element && node.HasAttributes)
            {
                foreach (var attribute in node.Attributes)
                {
                    nodeInfo.Attributes.Add(new HtmlAttributeInfo
                    {
                        Name = attribute.Name,
                        Value = attribute.Value
                    });
                }
            }

            // Concatenate text content for inline elements and their children
            string combinedInnerText = nodeInfo.InnerText;  // Initialize with existing text (for text-only elements)

            foreach (var childNode in node.ChildNodes)
            {
                if (childNode.NodeType == HtmlNodeType.Text && !string.IsNullOrWhiteSpace(childNode.InnerText))
                {
                    // Add text nodes directly
                    combinedInnerText += childNode.InnerText.Trim() + " ";
                }
                else if (childNode.NodeType == HtmlNodeType.Element)
                {
                    var childNodeInfo = AnalyzeNode(childNode, nodeInfo);
                    if (childNodeInfo != null)
                    {
                        nodeInfo.Children.Add(childNodeInfo);

                        // Combine the inner text for inline elements and their text children
                        if (InlineElements.Contains(childNode.Name.ToLower()))
                        {
                            combinedInnerText += childNodeInfo.InnerText + " ";
                        }
                    }
                }
            }

            // Set the combined inner text for the current node
            nodeInfo.InnerText = combinedInnerText.Trim();  // Ensure even text-only elements have their inner text set

            return nodeInfo;
        }


    }
}
