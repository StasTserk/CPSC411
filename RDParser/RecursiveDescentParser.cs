using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CPSC411.Exceptions;
using CPSC411.Lexer;

namespace CPSC411.RDParser
{

    public delegate Node ProductionEvaluator(RecursiveDescentParser parser);

    public delegate bool ProductionDecider(RecursiveDescentParser parser);

    public class RecursiveDescentParser
    {
        private readonly ICollection<ParsingRule> _rules;
        private readonly IList<IToken> _tokens;
        private int _indent = 0;

        public RecursiveDescentParser(IList<IToken> tokens)
        {
            _tokens = tokens;
            _rules = new List<ParsingRule>();
        }

        public Node ParseTokens()
        {
            return _rules.First().Invoke(_tokens);
        }

        public Node InvokeRule(string ruleName)
        {
            
            LogIndented($"Invoking {ruleName}...");
            _indent++;
            var node = _rules.First(r => r.Name == ruleName).Invoke(_tokens);
            _indent--;
            LogIndented($"Done invoking {ruleName}");
            return node;
        }

        private void LogIndented(string s)
        {
            Console.WriteLine($"{new string(' ', _indent)}{s}");
        }

        public RecursiveDescentParser AddRule(string name, ProductionDecider decider, ProductionEvaluator evaluator)
        {
            var rule = _rules.FirstOrDefault(r => r.Name == name);
            if (rule == null)
            {
                rule = new ParsingRule(this)
                {
                    Name = name
                };
                _rules.Add(rule);
            }
            rule.AddProduction(decider, evaluator);

            return this;
        }

        public bool TryMatch(TokenType tokenType)
        {
            return _tokens.First().Type == tokenType;
        }

        public bool TryMatch(string ruleName)
        {
            return _rules.First(r => r.Name == ruleName).CanMatch();
        }

        public Node TryConsumeToken(TokenType tokenType)
        {
            LogIndented($"Consuming {tokenType}");
            if (!TryMatch(tokenType))
                throw new UnexpectedTokenException(
                    $"Unexpected token encountered! {_tokens.FirstOrDefault()}, Expected {tokenType}");

            _tokens.RemoveAt(0);
            return new Node {Contents = tokenType.ToString()};
        }


        public void AddSampleRule()
        {
            this.AddRule(
                name: "sample", 
                decider: tokenList => tokenList.TryMatch(TokenType.If),
                evaluator: parser =>
                {
                    var node = new Node {Contents = "IF Statement"};
                    node.AddChild(parser.TryConsumeToken(TokenType.If));
                    node.AddChild(parser.InvokeRule("Expression"));
                    node.AddChild(parser.TryConsumeToken(TokenType.Then));
                    node.AddChild(parser.InvokeRule("ThenExpression"));
                    return node;
                });
        }
    }

    public class ParsingRule
    {
        private readonly RecursiveDescentParser _parent;
        public string Name { get; set; }
        private readonly IDictionary<ProductionDecider, ProductionEvaluator> _productions;

        public ParsingRule(RecursiveDescentParser parent)
        {
            _parent = parent;
            _productions = new Dictionary<ProductionDecider, ProductionEvaluator>();
        }

        public Node Invoke(IList<IToken> tokens)
        {
            var rule = _productions.FirstOrDefault(p => p.Key.Invoke(_parent));

            if (rule.Value == null)
            {
                // invalid syntax!
                throw new UnexpectedTokenException($"Unexpected token at {tokens.First()} on line {tokens.FirstOrDefault()?.LineNumber}");
            }

            return rule.Value.Invoke(_parent);
        }

        public void AddProduction(ProductionDecider decider, ProductionEvaluator evaluator)
        {
            _productions.Add(decider, evaluator);
        }

        public bool CanMatch()
        {
            return _productions.Any(p => p.Key.Invoke(_parent));
        }
    }
}
