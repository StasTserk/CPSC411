using System.Collections.Generic;

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

        public IEnumerable<Node> Children { get { return _children;} }
    }
}