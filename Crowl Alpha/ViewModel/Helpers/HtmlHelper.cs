using Crowl_Alpha.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Crowl_Alpha.ViewModel.Helpers
{
    public class HtmlHelper
    {
        public static string GenerateHtml(HtmlNodeInfo node)
        {
            StringBuilder htmlBuilder = new StringBuilder();

            // Recursively build the HTML string
            BuildHtml(node, htmlBuilder);

            return htmlBuilder.ToString();
        }

        private static void BuildHtml(HtmlNodeInfo node, StringBuilder htmlBuilder)
        {
            // Process only if it's an element node
            if (node.NodeType == "Element")
            {
                // Start tag with attributes
                htmlBuilder.Append("<").Append(node.Name);

                if (node.Attributes != null && node.Attributes.Count > 0)
                {
                    foreach (var attribute in node.Attributes)
                    {
                        htmlBuilder.Append($" {attribute.Name}=\"{attribute.Value}\"");
                    }
                }

                htmlBuilder.Append(">");

                // If there is any inner text, append it
                if (!string.IsNullOrEmpty(node.InnerText))
                {
                    htmlBuilder.Append(node.InnerText);
                }

                // Process children nodes recursively
                if (node.Children != null && node.Children.Count > 0)
                {
                    foreach (var child in node.Children)
                    {
                        BuildHtml(child, htmlBuilder);
                    }
                }

                // End tag
                htmlBuilder.Append($"</").Append(node.Name).Append(">");
            }
            else if (node.NodeType == "Text")
            {
                Debug.WriteLine("Here");
                // If it's a text node, just append the inner text
                htmlBuilder.Append(node.InnerText);
            }
            else if (node.NodeType == "Comment")
            {
                // For comments, wrap it in a comment tag
                htmlBuilder.Append("<!--").Append(node.InnerText).Append("-->");
            }
        }

    }
}

