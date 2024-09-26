using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crowl_Alpha.ViewModel.Helpers
{
    public class HtmlHelper
    {
        public static string FormatHtml(string rawHtml)
        {
            var formattedHtml = new StringBuilder();
            int indentLevel = 0;
            var inlineTags = new HashSet<string> { "a", "span", "strong", "em", "b", "i", "option", "li" };
            var selfClosingTags = new HashSet<string> { "br", "img", "hr", "input", "meta", "link" };

            foreach (var line in rawHtml.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var trimmedLine = line.Trim();

                if (string.IsNullOrEmpty(trimmedLine))
                {
                    continue;
                }

                // Determine if it's a closing tag
                if (trimmedLine.StartsWith("</"))
                {
                    // Ensure indentLevel does not go below zero
                    indentLevel = Math.Max(0, indentLevel - 1);
                    formattedHtml.AppendLine(new string(' ', indentLevel * 4) + trimmedLine);
                    continue;
                }

                // Determine if it's a self-closing tag
                bool isSelfClosing = trimmedLine.EndsWith("/>") || selfClosingTags.Contains(GetTagName(trimmedLine));

                // Write the line with the current indentation
                formattedHtml.AppendLine(new string(' ', indentLevel * 4) + trimmedLine);

                // Increase indentation only if it's an opening tag that isn't self-closing or inline
                if (trimmedLine.StartsWith("<") && !trimmedLine.StartsWith("</") && !isSelfClosing && !inlineTags.Contains(GetTagName(trimmedLine)))
                {
                    indentLevel++;
                }
            }

            return formattedHtml.ToString();
        }

        // Helper method to extract tag name from an HTML line
        private static string GetTagName(string line)
        {
            // Remove the angle brackets and split the line by space to extract the tag name
            var tag = line.Trim('<', '>', '/').Split(' ')[0].ToLower();
            return tag;
        }
    
    }
}

