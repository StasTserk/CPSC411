using System;
using System.Text;
using CPSC411.Lexer;
using CPSC411.RDParser;

namespace CPSC411.StackTranslator
{
    public class StackMachineConverter
    {
        private int _labelInt;
        
        public string ConvertAst(Node headNode)
        {
            var builder = new StringBuilder();

            ConvertNode(headNode, builder);

            return builder.ToString();
        }

        private string GenerateLabel()
        {
            return $"L{_labelInt++}";
        }

        private void ConvertNode(Node node, StringBuilder builder)
        {
            switch (node.Type)
            {
                case NodeType.If:
                    ProcessIf(node, builder);
                    break;
                case NodeType.Begin:
                    // of the form [BEGIN] [statement list] [END]
                    ConvertNode(node.Children[1], builder);
                    break;
                case NodeType.While:
                    ProcessWhile(node, builder);
                    break;
                case NodeType.Factor:
                    ProcessFactor(node, builder);
                    break;
                case NodeType.Expression:
                case NodeType.Term:
                case NodeType.StatementList:
                    // has the form [term] [expr']
                    ConvertNode(node.Children[0], builder);
                    ConvertNode(node.Children[1], builder);
                    break;
                case NodeType.MoreExpression:
                    // of the form [addop] [term] [moreexpr]
                    ConvertNode(node.Children[2], builder); // more expr
                    ConvertNode(node.Children[1], builder); // term
                    builder.AppendLine( // postfix the add/sub
                        node.Children[0].TerminalTokenType == TokenType.Add ?
                        "    OP1 +" : "    OP1 -");
                    break;
                case NodeType.MoreTerms:
                    // of the form [mulop] [factor] [moreterm]
                    ConvertNode(node.Children[2], builder); // more expr
                    ConvertNode(node.Children[1], builder); // term
                    builder.AppendLine( // postfix the add/sub
                        node.Children[0].TerminalTokenType == TokenType.Mul ?
                        "    OP1 *" : "    OP1 /");
                    // has the form 
                    break;
                case NodeType.Terminal: // may not be needed
                    break;
                case NodeType.Null: // nothing to add
                    break;
                case NodeType.Input:
                    // of the form [INPUT] [ID]
                    builder.AppendLine($"    READ {node.Children[1].Data}");
                    break;
                case NodeType.Assign:
                    // has the form [ID] [:=] [Expr]
                    ConvertNode(node.Children[2], builder);
                    builder.AppendLine($"    LOAD {node.Children[0].Data}");
                    break;
                case NodeType.Write:
                    // of the form [WRITE] [expr]
                    ConvertNode(node.Children[1], builder);
                    builder.AppendLine("    PRINT");
                    break;
                case NodeType.MoreStatements:
                    // of the form [;] [stmt] [morestmt]
                    ConvertNode(node.Children[1], builder);
                    ConvertNode(node.Children[2], builder);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void ProcessFactor(Node node, StringBuilder builder)
        {
            switch (node.Children.Count)
            {
                case 3:
                    ConvertNode(node.Children[1], builder);
                    break;
                case 2:
                    // [-] [num]
                    builder.AppendLine($"    cPUSH -{node.Children[1].Data}");
                    break;
                default:
                    builder.AppendLine(node.Children[0].TerminalTokenType == TokenType.Id
                        ? $"    rPUSH {node.Children[0].Data}"
                        : $"    cPUSH {node.Children[0].Data}");
                    break;
            }
        }

        private void ProcessIf(Node headNode, StringBuilder builder)
        {
// of the form [IF] [expr] [THEN] [stmt] [ELSE] [stmt]
            var elseLabel = GenerateLabel();
            var skipLabel = GenerateLabel();

            ConvertNode(headNode.Children[1], builder);
            builder.AppendLine($"    cJUMP {elseLabel}");
            ConvertNode(headNode.Children[3], builder);
            builder.AppendLine($"    JUMP {skipLabel}");
            builder.AppendLine($"{elseLabel}:");
            ConvertNode(headNode.Children[5], builder);
            builder.AppendLine($"{skipLabel}:");
        }

        private void ProcessWhile(Node headNode, StringBuilder builder)
        {
            // of the form [WHILE] [expr] [DO] [stmt]
            var headLabel = GenerateLabel();
            var exitLabel = GenerateLabel();
            // make labels first
            builder.AppendLine($"{headLabel}:");
            ConvertNode(headNode.Children[1], builder); // eval expr
            // decision branch
            builder.AppendLine($"    cJUMP {exitLabel}");
            ConvertNode(headNode.Children[3], builder); // do statement
            // skip back, and add follow label
            builder.AppendLine($"    JUMP {headLabel}");
            builder.AppendLine($"{exitLabel}:");
        }
    }
}
