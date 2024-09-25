using System.Collections.Generic;

namespace Crowl_Alpha.Model
{

    public class HtmlNodeInfo
    {
        public string NodeType { get; set; }  // Element, Comment, Text, etc.
        public string DataUid { get; set; } //Unique Identifier of the element
        public string Name { get; set; }      // Tag name (e.g., div, p, span, etc.)
        public string InnerText { get; set; } // The inner text of the node
        public List<HtmlAttributeInfo> Attributes { get; set; } = new List<HtmlAttributeInfo>(); // Attributes of the node
        public List<HtmlNodeInfo> Children { get; set; } = new List<HtmlNodeInfo>();  // Child nodes

        // Optional reference to the parent node
        public HtmlNodeInfo Parent { get; set; }
    }

    public class HtmlAttributeInfo
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

}
