﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using CPSC411.Lexer;

namespace CPSC411.RDParser
{
    public class Node
    {
        private readonly IList<Node> _children = new List<Node>();

        public string Contents { get; set; }

        public Node AddChild(Node child)
        {
            _children.Add(child);
            return this;
        }

        public IList<Node> Children => _children;
        public bool IsTerminal { get; set; }
        public string Data { get; set; }
        public NodeType Type { get; set; }
        public TokenType TerminalTokenType { get; set; }

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