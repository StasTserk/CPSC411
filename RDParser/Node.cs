using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CPSC411.RDParser
{
    public class Node
    {
        private readonly ICollection<Node> _children = new List<Node>();

        public string Contents { get; set; }

        public Node AddChild(Node child)
        {
            _children.Add(child);
            return this;
        }

        public IEnumerable<Node> Children => _children;

        public override string ToString()
        {
            return ToString(0);
        }

        public string ToString(int indent)
        {
            var builder = new StringBuilder($"-{Contents}\n");

            var indentString = string.Concat(Enumerable.Repeat("|  ", indent));

            foreach (var child in _children)
            {
                builder.Append($"{indentString}{child.ToString(indent+1)}");
            }
            return builder.ToString();
        }
    }
}