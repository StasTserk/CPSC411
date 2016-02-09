using System.Text;
using CPSC411.RDParser;

namespace CPSC411
{
    public class GraphVizGenerator
    {
        public string GenerateGraphString(Node headNode)
        {
            var builder = new StringBuilder("digraph G {");
            builder.AppendLine("  a ;");
            AddNodeText(
                node: headNode, 
                builder: builder, 
                prefix: "a");

            builder.AppendLine("}");

            return builder.ToString();
        }

        private void AddNodeText(Node node, StringBuilder builder, string prefix)
        {
            var prefixChar = 'a';
            
            builder.AppendLine($"  {prefix} [label=\"{node.Contents}\"]");
            foreach (var child in node.Children)
            {
                
                builder.AppendLine($"  {prefix} -> {prefix}{prefixChar}");
                AddNodeText(child, builder, prefix+prefixChar);
                prefixChar ++;
            }
        }
    }
}
