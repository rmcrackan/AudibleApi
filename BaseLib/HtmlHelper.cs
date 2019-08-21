using System;
using System.Collections.Generic;

namespace BaseLib
{
    public static class HtmlHelper
    {
        public static Dictionary<string, string> GetInputs(string body)
        {
            if (body is null)
                throw new ArgumentNullException(nameof(body));

            var inputs = new Dictionary<string, string>();
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(body);

            var nodes = doc.DocumentNode.SelectNodes(".//input");

            if (nodes is null)
                return inputs;

            foreach (var node in nodes)
            {
                var name = node.Attributes["name"]?.Value;
                var value = node.Attributes["value"]?.Value;

                if (!string.IsNullOrWhiteSpace(name))
                    inputs[name] = value;
            }

            return inputs;
        }
    }
}
